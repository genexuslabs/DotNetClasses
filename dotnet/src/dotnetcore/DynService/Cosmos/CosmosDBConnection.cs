using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GeneXus.Data.Cosmos;
using GeneXus.Data.NTier.CosmosDB;
using log4net;
using Microsoft.Azure.Cosmos;

namespace GeneXus.Data.NTier
{

	public class CosmosDBService : GxService
	{
		public CosmosDBService(string id, string providerId) : base(id, providerId, typeof(CosmosDBConnection)){}
	}

	public class CosmosDBConnection : ServiceConnection
	{
		private const string REGION = "ApplicationRegion";
		private const string DATABASE = "database";
		private const string SERVICE_URI = "serviceURI";
		private const string ACCOUNT_KEY = "AccountKey";
		private const string ACCOUNT_DATASOURCE = "data source";
		private static CosmosClient cosmosClient;
		private static Database cosmosDatabase;
		private static string mapplicationRegion;
		private static string mdatabase;
		private static string mAccountKey;
		private static string mAccountEndpoint;
		private static string mserviceURI;
		private static string mConnectionString;
		private const string TABLE_ALIAS = "t";
		
		//Options not supported by the spec yet
		//private const string DISTINCT = "DISTINCT";
		
		static readonly ILog logger = log4net.LogManager.GetLogger(typeof(CosmosDBConnection));
		public override string ConnectionString
		{
			get
			{
				return mConnectionString;
			}

			set
			{
				mConnectionString = value;
				State = ConnectionState.Executing;
				InitializeDBConnection();
			}
		}
		private static void InitializeDBConnection()
		{
			DbConnectionStringBuilder builder = new DbConnectionStringBuilder(false);
			builder.ConnectionString = mConnectionString;

			if (builder.TryGetValue(SERVICE_URI, out object serviceURI))
			{
				mserviceURI = serviceURI.ToString();
			}
			if (builder.TryGetValue(REGION, out object region))
			{
				mapplicationRegion = region.ToString();
			}
			if (string.IsNullOrEmpty(mserviceURI) && (builder.TryGetValue(ACCOUNT_DATASOURCE, out object accountEndpoint)))
			{
				mAccountEndpoint = accountEndpoint.ToString();
				mserviceURI = mAccountEndpoint;
			}
			if (builder.TryGetValue(ACCOUNT_KEY, out object accountKey))
			{
				mAccountKey = accountKey.ToString();
				mserviceURI = $"{mserviceURI};AccountKey={mAccountKey}";
			}
			if (builder.TryGetValue(DATABASE, out object database))
			{
				mdatabase = database.ToString();
			}
		}

		private static void Initialize()
		{
			if (!string.IsNullOrEmpty(mserviceURI) && !string.IsNullOrEmpty(mapplicationRegion) && (!string.IsNullOrEmpty(mdatabase)))
				cosmosClient = new CosmosClient(mserviceURI, new CosmosClientOptions() { EnableContentResponseOnWrite = false, ApplicationRegion = mapplicationRegion });
			else
			{
				if (string.IsNullOrEmpty(mapplicationRegion))
					throw new Exception("Application Region is a mandatory additional connection string attribute.");
				else
					if (string.IsNullOrEmpty(mdatabase))
						throw new Exception("Database is a mandatory additional connection string attribute.");
					else
						throw new Exception("Connection string is not set or is not valid. Unable to connect.");
			}
			cosmosDatabase = cosmosClient.GetDatabase(mdatabase);
		}
		private Container GetContainer(string containerName)
		{
			if (cosmosDatabase != null && !string.IsNullOrEmpty(containerName))
				return cosmosClient.GetContainer(cosmosDatabase.Id, containerName);
			return null;
		}
		private string SetupQuery(string projectionList, string filterExpression, string tableName, string orderbys)
		{
			string sqlSelect = string.Empty;
			string sqlFrom = string.Empty;
			string sqlWhere = string.Empty;
			string sqlOrder = string.Empty;

			string SELECT_TEMPLATE = "select {0}";
			string FROM_TEMPLATE = "from {0} t";
			string WHERE_TEMPLATE = "where {0}";
			string ORDER_TEMPLATE = "order by {0}";

			if (!string.IsNullOrEmpty(projectionList))
				sqlSelect = string.Format(SELECT_TEMPLATE, projectionList);
			else
			{ 
				throw new Exception("Error setting up the query. Projection list is empty.");
			}
			if (!string.IsNullOrEmpty(tableName))
				sqlFrom = string.Format(FROM_TEMPLATE, tableName);
			else
			{
				throw new Exception("Error setting up the query. Table name is empty.");
			}
			if (!string.IsNullOrEmpty(filterExpression))
				sqlWhere = string.Format(WHERE_TEMPLATE, filterExpression);
			if (!string.IsNullOrEmpty(orderbys))
				sqlOrder = string.Format(ORDER_TEMPLATE, orderbys);

			return $"{sqlSelect} {sqlFrom} {sqlWhere} {sqlOrder}";
		}

		/// <summary>
		/// Execute insert, update and delete queries.
		/// </summary>
		/// <param name="cursorDef"></param>
		/// <param name="parms"></param>
		/// <param name="behavior"></param>
		/// <returns></returns>
		/// <exception cref="NotImplementedException"></exception>
		/// <exception cref="Exception"></exception>
		public override int ExecuteNonQuery(ServiceCursorDef cursorDef, IDataParameterCollection parms, CommandBehavior behavior)
		{
			Initialize();
			CosmosDBQuery query = cursorDef.Query as CosmosDBQuery;
			if (query == null)
				return 0;

			bool isInsert = query.CursorType == ServiceCursorDef.CursorType.Insert;
			bool isUpdate = query.CursorType == ServiceCursorDef.CursorType.Update;

			Dictionary<string, object> values = new Dictionary<string, object>();
			string jsonData = string.Empty;

			string partitionKey = query.PartitionKey;
			JsonObject jsonObject = new JsonObject();

			Dictionary<string, Object> keyCondition = new Dictionary<string, Object>();

			//Setup the json payload to execute the insert or update query.
			foreach (KeyValuePair<string, string> asg in query.AssignAtts)
			{
				string name = asg.Key;
				string parmName = asg.Value.Substring(1).Remove(asg.Value.Length - 2);
				CosmosDBHelper.AddItemValue(name, parmName, values, parms, query.Vars, ref jsonObject);
				if (name == partitionKey)
					keyCondition[partitionKey] = values[name];
			}

			//Get the values for id and partitionKey
			string regex1 = @"\(([^\)\(]+)\)";
			string regex2 = @"(.*)[^<>!=]\s*(=|!=|<|>|<=|>=|<>)\s*(:.*:)";

			string keyFilterS;
			string condition = string.Empty;
			IEnumerable<string> keyFilterQ = Array.Empty<string>();
			IEnumerable<string> allFilters = query.KeyFilters.Concat(query.Filters);

			foreach (string keyFilter in allFilters)
			{
				keyFilterS = keyFilter;
				condition = keyFilter;
				MatchCollection matchCollection = Regex.Matches(keyFilterS, regex1);
				foreach (Match match in matchCollection)
				{
					if (match.Groups.Count > 0)
					{
						string cond = match.Groups[1].Value;
						Match match2 = Regex.Match(cond, regex2);
						if (match2.Success)
						{
							string varName = match2.Groups[3].Value;
							varName = varName.Remove(varName.Length - 1).Substring(1);
							string name = match2.Groups[1].Value;
							VarValue varValue = query.Vars.FirstOrDefault(v => v.Name == $":{varName}");

							if (varValue != null)
							{
								keyCondition[name] = varValue.Value;
								if (isUpdate && name == "id")
									jsonObject.Add(name, JsonValue.Create(varValue.Value));
								
								if (isUpdate && name == partitionKey && partitionKey != "id")
									jsonObject.Add(name, JsonValue.Create(varValue.Value));
							}
							else
							{
								if (parms[varName] is ServiceParameter serviceParm)
								{
									keyCondition[name] = serviceParm.Value;
								
									if (isUpdate && name == "id")
										jsonObject.Add(name, JsonValue.Create(serviceParm.Value));
									
									if (isUpdate && name == partitionKey && partitionKey != "id")
										jsonObject.Add(name, JsonValue.Create(serviceParm.Value));									
								}
							}
						}
					}
				}
			}
			jsonData = jsonObject.ToJsonString();
			Container container = GetContainer(query.TableName);
			switch (query.CursorType)
			{
				case ServiceCursorDef.CursorType.Select:
					throw new NotImplementedException();

				case ServiceCursorDef.CursorType.Delete:

					if (container != null)
					{
						try
						{
							if (!keyCondition.Any() || !keyCondition.ContainsKey("id") || !keyCondition.ContainsKey(partitionKey))
							{
								throw new Exception($"Delete item failed: error parsing the query.");	
							}
							else
							{
								object idField = keyCondition["id"];

								GXLogging.Debug(logger,$"Delete : id= {idField.ToString()}, partitionKey= {keyCondition[partitionKey].ToString()}");
								Task<ResponseMessage> task = Task.Run<ResponseMessage>(async () => await container.DeleteItemStreamAsync(idField.ToString(), CosmosDBHelper.ToPartitionKey(keyCondition[partitionKey])).ConfigureAwait(false));
								if (task.Result.IsSuccessStatusCode)
								{
									return 1;
								}
								else
								{
									if (task.Result.ErrorMessage.Contains("404"))
									{
										throw new ServiceException(ServiceError.RecordNotFound, null);
									}
									else
									{
										throw new Exception($"Delete item from stream failed. Status code: {task.Result.StatusCode}. Message: {task.Result.ErrorMessage}");
									}
								}
							}
						}
						catch (Exception ex)
							{ throw ex; }
					}	
					else
					{
						throw new Exception("CosmosDB Delete Execution failed. Container not found.");
					}

					case ServiceCursorDef.CursorType.Insert:
						if (container != null)
						{
							try
							{
								if (!keyCondition.Any() || !keyCondition.ContainsKey(partitionKey))
								{
									throw new Exception($"Insert item failed: error parsing the query.");
								}
								else
								{
									GXLogging.Debug(logger,$"Insert : {jsonData}");
									using (MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(jsonData)))
									{
										Task<ResponseMessage> task = Task.Run<ResponseMessage>(async () => await container.CreateItemStreamAsync(stream, CosmosDBHelper.ToPartitionKey(keyCondition[partitionKey])).ConfigureAwait(false));
										if (task.Result.IsSuccessStatusCode)
											return 1;
										else
										{
											if (task.Result.ErrorMessage.Contains("Conflict (409)"))
											{
												throw new ServiceException(ServiceError.RecordAlreadyExists, null);
											}
											else
											{
												throw new Exception($"Create item from stream failed. Status code: {task.Result.StatusCode}. Message: {task.Result.ErrorMessage}");
											}
										}
									}
								}

							}
							catch (Exception ex)
							{
								throw ex;
							}
						}
						else
						{
							throw new Exception("CosmosDB Insert Execution failed. Container not found.");
						}
					case ServiceCursorDef.CursorType.Update:
					if (container != null)
					{
						if (!keyCondition.Any() || !keyCondition.ContainsKey("id") || !keyCondition.ContainsKey(partitionKey))
						{
							throw new Exception($"Update item failed: error parsing the query.");
						}
						else
						{
							try
							{
								GXLogging.Debug(logger,$"Update : {jsonData}");

								using (MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(jsonData)))
								{
									Task<ResponseMessage> task = Task.Run<ResponseMessage>(async () => await container.UpsertItemStreamAsync(stream, CosmosDBHelper.ToPartitionKey(keyCondition[partitionKey])).ConfigureAwait(false));
									if (task.Result.IsSuccessStatusCode)
										return 1;
									else
									{
										throw new Exception($"Update item from stream failed. Status code: {task.Result.StatusCode}. Message: {task.Result.ErrorMessage}");
									}
								}
							}
							catch (Exception ex)
							{
								throw ex;
							}
						}
					}
					else
					{
						throw new Exception("CosmosDB Update Execution failed. Container not found.");
					}
			}
			return 0;
			
		}
		public override IDataReader ExecuteReader(ServiceCursorDef cursorDef, IDataParameterCollection parms, CommandBehavior behavior)
		{

			Initialize();
			CosmosDBQuery query = cursorDef.Query as CosmosDBQuery;
			Container container = GetContainer(query?.TableName);
			if (container == null)
			{
				throw new Exception("Container not found.");
			}
			else
			{ 
				try
				{
					CreateCosmosQuery(query,cursorDef, parms, container, out CosmosDBDataReader dataReader, out RequestWrapper requestWrapper);
					return dataReader;
				}
				catch (CosmosException cosmosException)
				{
					throw cosmosException;
				}
				catch (Exception e) { throw e; }
			}
		}

		private VarValue GetDataEqualParameterfromQueryVars(string varName, IEnumerable<VarValue> values)
		{
			return values.FirstOrDefault(v => v.Name == $":{varName}");
		}
		private object GetDataEqualParameterfromCollection(string varName, IDataParameterCollection parms)
		{		
			if (parms[varName] is ServiceParameter serviceParm)
			{
				return serviceParm.Value;
			}
			return null;
		}
		private CosmosDBDataReader GetDataReaderQueryByPK(ServiceCursorDef cursorDef, Container container, string idValue, object partitionKeyValue,out RequestWrapper requestWrapper)
		{
			requestWrapper = new RequestWrapper(cosmosClient, container, null);
			requestWrapper.idValue = idValue;
			requestWrapper.partitionKeyValue = partitionKeyValue;

			GXLogging.Debug(logger,$"Execute PK query id = {requestWrapper.idValue}, partitionKey = {requestWrapper.partitionKeyValue}");
			requestWrapper.queryByPK = true;
			return new CosmosDBDataReader(cursorDef, requestWrapper);			
		}

		/// <summary>
		/// Create object for querying the database.
		/// </summary>
		/// <param name="query"></param>
		/// <param name="cursorDef"></param>
		/// <param name="parms"></param>
		/// <param name="container"></param>
		/// <param name="cosmosDBDataReader"></param>
		/// <param name="requestWrapper"></param>
		private void CreateCosmosQuery(CosmosDBQuery query,ServiceCursorDef cursorDef, IDataParameterCollection parms, Container container, out CosmosDBDataReader cosmosDBDataReader,out RequestWrapper requestWrapper)
		{
			//Check if the filters are the Primary Key	
			if (query.Filters.Count() == 1)
			{
				string regex = @"^\(\(id = :([a-zA-Z0-9]+):\) and \(([a-zA-Z0-9]+) = :([a-zA-Z0-9]+):\)\)";
				Match match = Regex.Match(query.Filters.ElementAt(0), regex);	
				if (match.Groups.Count > 0)
				{
					string pkParmValue;
					string idParmValue;
					string attItem = match.Groups[2].Value;
					if (attItem.Equals(query.PartitionKey))
					{
						pkParmValue = match.Groups[3].Value;
						VarValue fieldValue2 = GetDataEqualParameterfromQueryVars(pkParmValue, query.Vars);
						object pkValue;
						if (fieldValue2 != null)
							pkValue = fieldValue2.Value;
						else
							pkValue = GetDataEqualParameterfromCollection(pkParmValue, parms);

							
						idParmValue = match.Groups[1].Value;
						VarValue fieldValue1 = GetDataEqualParameterfromQueryVars(idParmValue, query.Vars);
						object idValue;
						if (fieldValue1 != null)
							idValue = fieldValue1.Value;
						else
							idValue = GetDataEqualParameterfromCollection(idParmValue, parms);
						if (idValue != null && pkValue != null)
						{ 
							cosmosDBDataReader = GetDataReaderQueryByPK(cursorDef, container, idValue.ToString(), pkValue, out requestWrapper);
							return;
						}
					}
				}
			}
			//Create the query
			string tableName = query.TableName;
			IEnumerable<string> projection = query.Projection;
			string element;
			string projectionList = string.Empty;
			foreach (string key in projection)
			{
				element = $"{TABLE_ALIAS}.{key}";
				if (!string.IsNullOrEmpty(projectionList))
					projectionList = $"{element},{projectionList}";
				else
					projectionList = $"{element}";
			}

			IEnumerable<string> allFilters = query.KeyFilters.Concat(query.Filters);
			IEnumerable<string> allFiltersQuery = Array.Empty<string>();	
			IEnumerable<string> keyFilterQ = Array.Empty<string>();

			foreach (string keyFilter in allFilters)
			{		
				string filterProcess = keyFilter.ToString();
				filterProcess = filterProcess.Replace("[", "(");
				filterProcess = filterProcess.Replace("]", ")");

				foreach (VarValue item in query.Vars)
				{
					string varValuestr = string.Empty;
					if (filterProcess.Contains(string.Format($"{item.Name}:")))
					{
						if (GeneXus.Data.Cosmos.CosmosDBHelper.FormattedAsStringGXType(item.Type))
							varValuestr = '"' + $"{item.Value.ToString()}" + '"';
						else
						{						
							if (item.Value is double)
							{
								NumberFormatInfo nfi = new NumberFormatInfo();
								nfi.NumberDecimalSeparator = ".";
								double dValue = (double)item.Value;
								varValuestr = dValue.ToString(nfi);
							}
							else
								varValuestr = item.Value.ToString();

							varValuestr = varValuestr.Equals("True") ? "true" : varValuestr;
							varValuestr = varValuestr.Equals("False") ? "false" : varValuestr;
						}
						filterProcess = filterProcess.Replace(string.Format($"{item.Name}:"), varValuestr);
					}
				}
				foreach (object p in parms)
				{
					if (p is ServiceParameter)
					{
						ServiceParameter p1 = (ServiceParameter)p;
						string varValuestr = string.Empty;
						if (filterProcess.Contains(string.Format($":{p1.ParameterName}:")))
						{
							if (GeneXus.Data.Cosmos.CosmosDBHelper.FormattedAsStringDbType(p1.DbType))
								varValuestr = '"' + $"{p1.Value.ToString()}" + '"';
							else
								varValuestr = p1.Value.ToString();
									
							filterProcess = filterProcess.Replace(string.Format($":{p1.ParameterName}:"), varValuestr);
						}
					}
				}

				filterProcess = filterProcess.Replace("Func.", "");
				foreach (string d in projection)
				{
					string wholeWordPattern = String.Format(@"\b{0}\b", d);
					filterProcess = Regex.Replace(filterProcess, wholeWordPattern, $"{TABLE_ALIAS}.{d}");
				}
				keyFilterQ = new string[] { filterProcess };
				allFiltersQuery = allFiltersQuery.Concat(keyFilterQ);

			}
			string filterExpression = allFiltersQuery.Any() ? String.Join(" AND ", allFiltersQuery) : null;

			IEnumerable<string> orderExpressionList = Array.Empty<string>();
			string expression = string.Empty;

			foreach (string orderAtt in query.OrderBys)
			{
				expression = orderAtt.StartsWith("(") ? $"{TABLE_ALIAS}.{orderAtt.Remove(orderAtt.Length-1,1).Remove(0,1)} DESC" : $"{TABLE_ALIAS}.{orderAtt} ASC";
				orderExpressionList = orderExpressionList.Concat(new string[] { expression });
			}

			string orderExpression = String.Join(",", orderExpressionList);
			string sqlQuery = SetupQuery(projectionList, filterExpression, tableName, orderExpression);

			GXLogging.Debug(logger,sqlQuery);

			QueryDefinition queryDefinition = new QueryDefinition(sqlQuery);
			requestWrapper = new RequestWrapper(cosmosClient, container, queryDefinition);
			requestWrapper.queryByPK = false;
			
			cosmosDBDataReader = new CosmosDBDataReader(cursorDef, requestWrapper);
		}
		internal static IOServiceContext NewServiceContext() => null;
	}
}
