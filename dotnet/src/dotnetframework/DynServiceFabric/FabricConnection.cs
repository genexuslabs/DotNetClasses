
using GeneXus.Http.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Web.Script.Serialization;
using log4net;
using GeneXus.Data.NTier;
using System.Data.Common;
using GeneXus.Utils;
using System.Collections;
using System.Globalization;

namespace GeneXus.Data.DynService.Fabric
{
	public class FabricConnection : ServiceConnection
	{
		private GxHttpClient restClient = new GxHttpClient();
		private Dictionary<string, object> payLoad = new Dictionary<string, object>();
		static readonly ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public new int ConnectionTimeout { get; set; } = 240000;
		
		public override string ConnectionString
		{
			get { return base.ConnectionString; }

			set
			{
				base.ConnectionString = value;
			}
		}

		bool SetServiceData(string connStr, GxHttpClient restClient)
		{
			DbConnectionStringBuilder builder = new DbConnectionStringBuilder(false);
			builder.ConnectionString = connStr;
			object serviceUri = "";
			object userID = "";
			object password = "";
			if (builder.TryGetValue("User Id", out userID))
			{
				if (builder.TryGetValue("Password", out password))
				{
					restClient.AddAuthentication(0, "", userID.ToString(), password.ToString());
				}
			}
			object ds_data = "";
			object uri_data = "";
			if (builder.TryGetValue("Data Source", out ds_data))
				serviceUri = ds_data;
			else
				if (builder.TryGetValue("uri", out uri_data))
					serviceUri = uri_data;
				else
					serviceUri = null;

			if (serviceUri != null)
			{
				String urlstring = "";
				String[] parts = serviceUri.ToString().Split(new String[] { "://" }, StringSplitOptions.None);
				if (parts.Length > 1)
				{
					if (parts[0].Equals("https"))
						restClient.Secure = 1;
					urlstring = parts[1];
				}
				else
				{
					urlstring = parts[0];
				}
				int position = urlstring.IndexOf("/");
				restClient.Host = urlstring.Substring(0, position);
				restClient.BaseURL = urlstring.Substring(position);
				restClient.Timeout = ConnectionTimeout;
				restClient.AddHeader("Content-Type", "application/json");
				return true;
			}
			return false;
		}

		public override int ExecuteNonQuery(ServiceCursorDef cursorDef, IDataParameterCollection parms, CommandBehavior behavior)
		{
			DataStoreHelperFabric.Query fabricQuery = cursorDef.Query as DataStoreHelperFabric.Query;
			if (fabricQuery != null)
			{
				Dictionary<String, String> tempParms = new Dictionary<string, string>();
				payLoad.Clear();
				foreach (KeyValuePair<String, String> kvp in fabricQuery.Parms)
				{
					String parName = kvp.Value.ToString().Substring(1);
					if (parms.Contains(parName))
					{
						IDataParameter m_par = ((IDataParameter)parms[parName]);
						if (m_par.DbType == DbType.Date || m_par.DbType == DbType.DateTime || m_par.DbType == DbType.DateTime2)
						{						
							tempParms.Add(kvp.Key, ((DateTime)m_par.Value).ToString("yyyy-MM-ddTHH:mm:ss.fff", CultureInfo.InvariantCulture));
						}
						else
							tempParms.Add(kvp.Key, m_par.Value.ToString());
					}

				}
				
				payLoad.Add("function", fabricQuery.Method);
				payLoad.Add("type", "invoke");
				payLoad.Add("param", tempParms);
				JavaScriptSerializer serializer = new JavaScriptSerializer(); 
				string jsonPayLoad = serializer.Serialize((object)payLoad);

				if (SetServiceData(this.ConnectionString, restClient))
				{
					restClient.AddString(jsonPayLoad);
					restClient.Execute("POST", fabricQuery.TableName);
					if (restClient.StatusCode == 200)
					{
						GXLogging.Info(log, restClient.ToString());
						return 0;
					}
					else
					{
						if (processError(restClient.StatusCode, restClient.ToString(), out int statusCode, out string msg))
						{
							throw new FabricException(statusCode, msg);
						}
						else
						{
							throw new GxADODataException("Error executing: " + restClient.ToString());
						}
					}
				}
				else
				{
					throw new GxADODataException("Error connecting : " + this.ConnectionString );
				}
			}
			else
			{
				if(cursorDef != null)
					throw new GxADODataException("Error executing: " + cursorDef.Query.ToString());
				else
					throw new GxADODataException("Error executing:  Unkwown Error");
			}
		}

		private bool  processError(short code, string msg, out int statusCode, out string emsg )
		{

			if (code >= 400)
			{
				JavaScriptSerializer jss = new JavaScriptSerializer();
				Dictionary<string, object> queryresponse = new Dictionary<string, object>();
				try
				{
					dynamic dynresponse = jss.Deserialize(msg, queryresponse.GetType());
					var resultdata = (dynresponse as Dictionary<string, object>)["error"];
					if (resultdata != null)
					{
						String resultmessage = resultdata as String;
						if (resultmessage.Contains("101"))
						{
							statusCode = 101;
							emsg = ServiceError.RecordNotFound;
							return true;
						}
						else if (resultmessage.Contains("102"))
						{
							statusCode = 102;
							emsg = ServiceError.RecordAlreadyExists;
							return true;
						}
						else
						{
							
							statusCode = code;
							emsg = resultmessage;
							return true;
						}
					}
					else
					{
						statusCode = 1;
						emsg = msg;
						return true;
					}
				}
				catch (Exception e)
				{
					statusCode = 1;
					emsg = msg + " " + e.ToString();
					return true;
				}
			}
			else
			{
				statusCode = -1;
				emsg = msg;
				return false;
			}
		}

		public override IDataReader ExecuteReader(ServiceCursorDef cursorDef, IDataParameterCollection parms, CommandBehavior behavior)
		{
			State = ConnectionState.Executing;
			FabricDataReader reader = null;
			DataStoreHelperFabric.Query query = cursorDef.Query as DataStoreHelperFabric.Query;
			payLoad.Clear();
			if (query.Filters.Length == 0)
			{
				Dictionary<String, String> tempParms = new Dictionary<string, string>();
				foreach (KeyValuePair<String, String> kvp in query.Parms)
				{
					String parName = kvp.Value.ToString().Substring(1);
					if (parms.Contains(parName))
					{
						tempParms.Add(kvp.Key, ((IDataParameter)parms[parName]).Value.ToString());
					}

				}
				payLoad.Add("function", query.Method);
				payLoad.Add("type", "query");
				payLoad.Add("param", tempParms);
				reader = runFabricQuery(query, payLoad, parms);
			}
			else
			{
				if (query.GetKeyVal().Length > 0)
				{
					ArrayList keyPars = new ArrayList();
					foreach (String parString in query.GetKeyVal())
					{
						
						if (parString.Substring(0, 1).Equals("@"))
						{
							String parName = parString.Substring(1);
							if (parms.Contains(parName))
							{
								keyPars.Add(((IDataParameter)parms[parName]).Value.ToString());
							}
						}
						else
						{
							keyPars.Add(parString);
						}
					}
					payLoad.Add("function", query.Method);
					payLoad.Add("type", "query");
					payLoad.Add("param", keyPars);
					reader = runFabricQuery(query, payLoad, parms);

				}
				else
				{
					reader = new FabricDataReader(new List<IDictionary<string, object>>(), parms, query.ColumnList);
				}
			}
			
			return reader;
		}

		FabricDataReader runFabricQuery(DataStoreHelperFabric.Query query, Dictionary<string, object> payLoad, IDataParameterCollection parcollection)
		{

			JavaScriptSerializer serializer = new JavaScriptSerializer(); 
			string jsonPayLoad = serializer.Serialize((object)payLoad);
			List<IDictionary<string, object>> rowData = new List<IDictionary<string, object>>();
			if (SetServiceData(this.ConnectionString, restClient))
			{
				restClient.AddString(jsonPayLoad);
				restClient.Execute("POST", query.TableName);
				if (restClient.StatusCode == 200)
				{
					JavaScriptSerializer jss = new JavaScriptSerializer();
					Dictionary<string, object> queryresponse = new Dictionary<string, object>();
					try
						{
						dynamic dynresponse = jss.Deserialize(restClient.ToString(), queryresponse.GetType());
						Dictionary<string, object> response = (dynresponse as Dictionary<string, object>);
						var resultdata =  (response.ContainsKey("result"))? response["result"]: response["value"];												
						if (resultdata != null)
						{
							System.Collections.ArrayList records;

							if ((resultdata as Dictionary<string, object>) != null)
								records = (ArrayList)(resultdata as Dictionary<string, object>)["data"];
							else {
								dynamic dynrecs = jss.Deserialize(resultdata.ToString(), typeof(Dictionary<string, object>));
								records = (ArrayList)dynrecs["data"];
							}

							if (query.Filters.Length > 0 && !query.Filters[0].Contains("=="))
							{
								List<String>  queryCond = new List<string>();
								List<String>  queryVal = new List<string>();
								List<String> queryVar = new List<string>();
								foreach (String filter in query.Filters)
								{
									String SOperator = "";
									if (filter.Contains(">="))
										SOperator = ">=";
									else if (filter.Contains("<="))
										SOperator = "<=";									
									else if (filter.Contains("<"))
										SOperator = "<";
									else if (filter.Contains(">"))
										SOperator = ">";									
									queryCond.Add(SOperator);
									foreach (IDataParameter parm in parcollection)
									{
										if (filter.Contains("@" + parm.ParameterName))
											queryVal.Add( parm.Value.ToString());
									}
									
									String[] parts = filter.Replace("(","").Replace(")","").Split(SOperator.ToCharArray());
									if (parts.Length > 1)
									{
										queryVar.Add(parts[0].Trim());
									}
								}
								for (int i = 0; i < records.Count; i++)
								{
									
									Dictionary<string, object> record = ((Dictionary<string, object>)records[i]) as Dictionary<string, object>;
									if (fabricEval(record, queryCond, queryVal, queryVar))
									{
										rowData.Add(record);
									}
								}
									
							}
							else
							{
								for (int i = 0; i < records.Count; i++)
								{
									Dictionary<string, object> record = ((Dictionary<string, object>)records[i]) as Dictionary<string, object>;
									rowData.Add(record);
								}
							}
						}						
					}
					catch (Exception ex)
					{
						throw new GxADODataException("error receiving data ", ex);
					}
					FabricDataReader reader = new FabricDataReader(rowData, parcollection, query.ColumnList);
					GXLogging.Info(log, restClient.ToString());
					return reader;
				}
				else
				{
					throw new GxADODataException("Error executing: " + restClient.ToString());
				}
			}
			else
				return null;
		}

		bool fabricEval(Dictionary<string, object> record, List<String> queryCond, List<String> queryVal, List<String> queryVar)
		{
			bool evalresult = false;
			int i = 0;
			foreach (String cond in queryCond)
			{
				switch( cond)
				{
					case ">":
						if ((record[queryVar[i]]).ToString().CompareTo(queryVal[i])>0)
						{
							evalresult = true;
						}
						break;
					case "<":
						if ((record[queryVar[i]]).ToString().CompareTo(queryVal[i]) < 0)
						{
							evalresult = true;
						}
						break;
					case ">=":
						if ((record[queryVar[i]]).ToString().CompareTo(queryVal[i]) >= 0)
						{
							evalresult = true;
						}
						break;
					case "<=":
						if ((record[queryVar[i]]).ToString().CompareTo(queryVal[i]) <= 0)
						{
							evalresult = true;
						}
						break;
					default:
						evalresult = true;
						break;
				}
				i++;
			}
			return evalresult;
		}
			
	}
}
