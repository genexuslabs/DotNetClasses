using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text;
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
		public CosmosDBService(string id, string providerId) : base(id, providerId, typeof(CosmosDBConnection))
		{

		}
		/*public override IDataReader GetCacheDataReader(CacheItem item, bool computeSize, string keyCache)
		{
			return new GxDynamoDBCacheDataReader(item, computeSize, keyCache);
		}*/
	}

	public class CosmosDBConnection : ServiceConnection
	{
		private const string REGION = "ApplicationRegion";
		private const string DATABASE = "database";
		private const string SERVICE_URI = "serviceURI";
		private const string ACCOUNT_KEY = "AccountKey";
		private static CosmosClient cosmosClient;
		private static Database cosmosDatabase;
		private static string mapplicationRegion;
		private static string mdatabase;
		private static string mAccountKey;
		private static string mserviceURI;
		private static string mConnectionString;

		//https://learn.microsoft.com/en-us/azure/cosmos-db/nosql/query/select
		private const string SELECT_CMD = "SELECT";
		private const string FROM = "FROM";
		private const string TABLE_ALIAS = "t";
		private const string WHERE = "WHERE";

		private const string FILTER_PATTERN = @"\((.*) = :(.*)\)";

		//private const string DISTINCT = "DISTINCT";
		//private const string ORDER_BY = "ORDER BY";

		//static readonly ILog logger = log4net.LogManager.GetLogger(typeof(CosmosDBConnection));

		//TODO: Usar un Hashset para guardar los containers

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
			if (builder.TryGetValue(ACCOUNT_KEY, out object accountKey))
			{
				mAccountKey = accountKey.ToString();
				mserviceURI = $"{mserviceURI};AccountKey={mAccountKey}";
			}
			if (builder.TryGetValue(DATABASE, out object database))
			{
				mdatabase = database.ToString();
			}
			//TODO: check Mandatory parameters
			//TODO: Connect using connection string + connection key
		}

		private static void Initialize()
		{
			if (!string.IsNullOrEmpty(mserviceURI) && !string.IsNullOrEmpty(mapplicationRegion))
				cosmosClient = new CosmosClient(mserviceURI, new CosmosClientOptions() { ApplicationRegion = mapplicationRegion });

			if (!string.IsNullOrEmpty(mdatabase))
								cosmosDatabase = cosmosClient.GetDatabase(mdatabase);
		}

		private Container GetContainer(string containerName)
		{
			if (cosmosDatabase != null && !string.IsNullOrEmpty(containerName))
				return cosmosClient.GetContainer(cosmosDatabase.Id, containerName);
			return null;
		}

		public override int ExecuteNonQuery(ServiceCursorDef cursorDef, IDataParameterCollection parms, CommandBehavior behavior)
		{
			Initialize();
			CosmosDBQuery query = cursorDef.Query as CosmosDBQuery;
			bool isInsert = query.CursorType == ServiceCursorDef.CursorType.Insert;
			bool isUpdate = query.CursorType == ServiceCursorDef.CursorType.Update;

			Dictionary<string, object> values = new Dictionary<string, object>();
			string jsonData = string.Empty;

			string partitionKey = query.PartitionKey;
			string partitionKeyValue = string.Empty;
			foreach (KeyValuePair<string, string> asg in query.AssignAtts)
			{
				string name = asg.Key;
				string parmName = asg.Value.Substring(1);
				CosmosDBHelper.AddItemValue(name, parmName, values, parms, query.Vars, ref jsonData);
				if (name == partitionKey)
					partitionKeyValue = values[name].ToString();
			}
			
			Dictionary<string, Object> keyCondition = new Dictionary<string, Object>();

			foreach (string keyFilter in query.KeyFilters.Concat(query.Filters))
			{
				Match match = Regex.Match(keyFilter, FILTER_PATTERN);
				if (match.Groups.Count > 1)
				{
					string varName = match.Groups[2].Value;
					string name = match.Groups[1].Value;
					VarValue varValue = query.Vars.FirstOrDefault(v => v.Name == $":{varName}");

					string jsonDataKey = String.Empty;
					string jsonDataPartitionKey = string.Empty;
					if (varValue != null)
					{ 
						keyCondition[name] = varValue.Value;
						//keyCondition[name] = GeneXus.Data.Cosmos.CosmosDBHelper.ToItemValue(varValue.Type, varValue.Value);
						
						if (isUpdate && name == "id")
							jsonDataKey = GeneXus.Data.Cosmos.CosmosDBHelper.AddToJsonStream(varValue.Type, name, varValue.Value);
						if (isUpdate && name == partitionKey)
							jsonDataPartitionKey = GeneXus.Data.Cosmos.CosmosDBHelper.AddToJsonStream(varValue.Type, name, varValue.Value);

						if (name == partitionKey)
							//TODO Partition Key can be numeric 
							partitionKeyValue = varValue.Value.ToString();
					}
					else
					{
						if (parms[varName] is ServiceParameter serviceParm)
						{
							keyCondition[name] = serviceParm.Value;
							//keyCondition[name] = GeneXus.Data.Cosmos.CosmosDBHelper.ToItemValue(serviceParm.DbType, serviceParm.Value);
							
							if (isUpdate && name == "id")
								jsonDataKey = GeneXus.Data.Cosmos.CosmosDBHelper.AddToJsonStream(serviceParm.DbType, name, serviceParm.Value);
							if (isUpdate && name == partitionKey)
								jsonDataPartitionKey = GeneXus.Data.Cosmos.CosmosDBHelper.AddToJsonStream(serviceParm.DbType, name, serviceParm.Value);

							if (name == partitionKey)
								//TODO Partition Key can be numeric 
								partitionKeyValue = serviceParm.Value.ToString();
						}
					}
					if (!string.IsNullOrEmpty(jsonDataKey))
					{
						if (!string.IsNullOrEmpty(jsonData))
						{
							jsonData = $"{jsonData},{jsonDataKey}";
						}
						else
							jsonData = jsonDataKey;
					}

					if (!string.IsNullOrEmpty(jsonDataPartitionKey))
					{
						if (!string.IsNullOrEmpty(jsonData))
							jsonData = $"{jsonData},{jsonDataPartitionKey}";
						else
							jsonData = jsonDataPartitionKey;
					}

				}
			}

			jsonData = "{" + jsonData + "}";

			//TODO: Get container from HashSet for performance
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
							if (keyCondition != null && keyCondition["id"] != null)
							{
								if (string.IsNullOrEmpty(partitionKeyValue))
									partitionKeyValue = keyCondition["id"].ToString(); // partitionKeyValue = id
									Task<ResponseMessage> task = Task.Run<ResponseMessage>(async () => await container.DeleteItemStreamAsync(keyCondition["id"].ToString(), new PartitionKey(partitionKeyValue)).ConfigureAwait(false));
								if (task.Result.IsSuccessStatusCode)
								{
									//ResponseMessage wrapps the delete record
									return 1;
								}
								else
								{
									if (task.Result.ErrorMessage.Contains("404"))
										throw new ServiceException(ServiceError.RecordNotFound, null);
									else
										throw new Exception($"Delete item from stream failed. Status code: {task.Result.StatusCode}. Message: {task.Result.ErrorMessage}");
								}
							}
							else
								throw new Exception($"Delete item failed: error parsing the query.");

						}
						catch (Exception ex)
						{ throw ex; }
					}

					else
					{
						throw new Exception("CosmosDB Execution failed. Container not found.");
					}

				case ServiceCursorDef.CursorType.Insert:
					if (container != null)
					{
						try
						{
							using (MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(jsonData)))
							{
								
								Task<ResponseMessage> task = Task.Run<ResponseMessage>(async () => await container.CreateItemStreamAsync(stream, new PartitionKey(partitionKeyValue)).ConfigureAwait(false));
								if (task.Result.IsSuccessStatusCode)
								{
									return 1;
								}
								else
								{
									if (task.Result.ErrorMessage.Contains("Conflict (409)"))
										throw new ServiceException(ServiceError.RecordAlreadyExists, null);
									else
										throw new Exception($"Create item from stream failed. Status code: {task.Result.StatusCode}. Message: {task.Result.ErrorMessage}");

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
						throw new Exception("CosmosDB Execution failed. Container not found.");
					}
				case ServiceCursorDef.CursorType.Update:
					if (container != null)
					{
						try
						{
							if (string.IsNullOrEmpty(partitionKeyValue))
								partitionKeyValue = keyCondition["id"].ToString(); // partitionKeyValue = id
								using (MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(jsonData)))
								{
									Task<ResponseMessage> task = Task.Run<ResponseMessage>(async () => await container.UpsertItemStreamAsync(stream, new PartitionKey(partitionKeyValue)).ConfigureAwait(false));
									if (task.Result.IsSuccessStatusCode)
									{
										return 1;
									}
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
					else
					{
						throw new Exception("CosmosDB Execution failed. Container not found.");
					}
			}

			return 0;
		}
		public override IDataReader ExecuteReader(ServiceCursorDef cursorDef, IDataParameterCollection parms, CommandBehavior behavior)
		{

			Initialize();
			CosmosDBQuery query = cursorDef.Query as CosmosDBQuery;
			//Get container from hashset for performance
			Container container = GetContainer(query.TableName);
			try
			{
				CreateCosmosQuery(query,cursorDef, parms, container, out CosmosDBDataReader dataReader, out RequestWrapper requestWrapper);
				return dataReader;
			}
			catch (CosmosException cosmosException)
			{
				//TODO: Handle cases
				throw cosmosException;
			}

			catch (Exception e) { throw e; }
		}
		private void CreateCosmosQuery(CosmosDBQuery query,ServiceCursorDef cursorDef, IDataParameterCollection parms, Container container, out CosmosDBDataReader cosmosDBDataReader,out RequestWrapper requestWrapper)
		{
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

			string regex1 = @"\(([^\)\(]+)\)";
			string regex2 = @"(.*)[^<>!=]\s*(=|!=|<|>|<=|>=|<>)\s*(:.*)";
			
			string keyFilterS;
			string condition = string.Empty;
			IEnumerable<string> keyFilterQ = Array.Empty<string>();

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
							//Get the value for this item
							string varValuestr = string.Empty;
							if (match2.Groups.Count > 0)
							{
								string column = match2.Groups[1].Value.Trim();
								string attName = match2.Groups[3].Value.Trim();
								if (attName.StartsWith(":"))
									attName = attName.Substring(1);

								string op = match2.Groups[2].Value.Trim();

								//look at IDataParameterCollection parms
								if (parms[attName] is ServiceParameter serviceParm)
									if (GeneXus.Data.Cosmos.CosmosDBHelper.FormattedAsStringDbType(serviceParm.DbType))
									//Nvarchar, etc?
									//Date?
									{
										varValuestr = '"' + $"{serviceParm.Value.ToString()}" + '"';
									}
									else
										varValuestr = serviceParm.Value.ToString();


								//look at query.vars
								foreach (VarValue item in query.Vars)
								{
									if (item.Name == match2.Groups[3].Value)
									{
										//if (item.Type == GXType.Char || item.Type == GXType.LongVarChar || item.Type == GXType.VarChar || item.Type == GXType.Text)
										if (GeneXus.Data.Cosmos.CosmosDBHelper.FormattedAsStringGXType(item.Type))
										//Nvarchar, etc?
										//Date?
										{
											varValuestr = '"' + $"{item.Value.ToString()}" + '"';
										}
										else
										{ 
											varValuestr = item.Value.ToString();
											varValuestr = varValuestr.Equals("True") ? "true" : varValuestr;
											varValuestr = varValuestr.Equals("False") ? "false" : varValuestr;
										}
										break;
									}
								}

								//Controlar antes de mandar la sentencia a ejecutar

								condition = condition.Replace(cond, $"{column}{op}{varValuestr}");
							}
							else
							{
								//Check that cond is a valid attribute - boolean

							}
						}
					}
				}

				foreach (string d in projection)
				{
					condition = condition.Replace(d, $"{TABLE_ALIAS}.{d}");
				}
				keyFilterQ = new string[] { condition };
				allFiltersQuery = allFiltersQuery.Concat(keyFilterQ);
				
			}
			string FilterExpression = allFiltersQuery.Any() ? String.Join(" AND ", allFiltersQuery) : null;

			string sqlQuery = string.Empty;
			if (FilterExpression != null)
				sqlQuery = $"{SELECT_CMD} {projectionList} {FROM} {tableName} {TABLE_ALIAS} {WHERE} {FilterExpression}";
			else
				sqlQuery = $"{SELECT_CMD} {projectionList} {FROM} {tableName} {TABLE_ALIAS}";

			QueryDefinition queryDefinition = new QueryDefinition(sqlQuery);
			requestWrapper = new RequestWrapper(cosmosClient, container, queryDefinition);
			cosmosDBDataReader = new CosmosDBDataReader(cursorDef, requestWrapper);
		}
		internal static IOServiceContext NewServiceContext() => null;
	}
	public class CosmosDBErrors
	{
		public const string ValidationException = "ValidationException";
		public const string ValidationExceptionMessageKey = "The AttributeValue for a key attribute cannot contain an empty string value.";
	}
}
