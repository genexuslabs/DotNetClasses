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
		private const string TABLE_ALIAS = "t";
		
		//Options not supported by the spec yet
		//private const string DISTINCT = "DISTINCT";
		
		static readonly ILog logger = log4net.LogManager.GetLogger(typeof(CosmosDBConnection));

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
			{ //ERROR

			}
			if (!string.IsNullOrEmpty(tableName))
				sqlFrom = string.Format(FROM_TEMPLATE, tableName);
			else
			{
				//ERROR
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
			string partitionKeyValue = string.Empty;

			//Setup the json payload to execute the insert or update query.
			foreach (KeyValuePair<string, string> asg in query.AssignAtts)
			{
				string name = asg.Key;
				string parmName = asg.Value.Substring(1).Remove(asg.Value.Length - 2);
				CosmosDBHelper.AddItemValue(name, parmName, values, parms, query.Vars, ref jsonData);
				if (name == partitionKey)
					partitionKeyValue = values[name].ToString();
			}

			Dictionary<string, Object> keyCondition = new Dictionary<string, Object>();
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
									//TODO Partition Key can be double, bool 
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
									jsonData = $"{jsonData},{jsonDataKey}";

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
							if (!keyCondition.Any() || !keyCondition.ContainsKey("id") || !keyCondition.ContainsKey(partitionKey))
							{
								logger.Error($"Delete item failed: error parsing the query.");
								throw new Exception($"Delete item failed: error parsing the query.");
							}
							else
							{
								object idField = keyCondition["id"];

								logger.Debug($"Delete : id= {idField.ToString()}, partitionKey= {partitionKeyValue}");
								Task<ResponseMessage> task = Task.Run<ResponseMessage>(async () => await container.DeleteItemStreamAsync(idField.ToString(), new PartitionKey(partitionKeyValue)).ConfigureAwait(false));
								if (task.Result.IsSuccessStatusCode)
								{
									//ResponseMessage wrapps the delete record
									return 1;
								}
								else
								{
									if (task.Result.ErrorMessage.Contains("404"))
									{
										logger.Debug(ServiceError.RecordNotFound);
										throw new ServiceException(ServiceError.RecordNotFound, null);
									}
									else
									{
										logger.Error($"Delete item from stream failed. Status code: {task.Result.StatusCode}. Message: {task.Result.ErrorMessage}");
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
						logger.Error("CosmosDB Delete Execution failed. Container not found.");
						throw new Exception("CosmosDB Delete Execution failed. Container not found.");
					}

					case ServiceCursorDef.CursorType.Insert:
						if (container != null)
						{
							try
							{
								logger.Debug($"Insert : {jsonData}");
								using (MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(jsonData)))
								{

									Task<ResponseMessage> task = Task.Run<ResponseMessage>(async () => await container.CreateItemStreamAsync(stream, new PartitionKey(partitionKeyValue)).ConfigureAwait(false));
									if (task.Result.IsSuccessStatusCode)
										return 1;
									else
									{
										if (task.Result.ErrorMessage.Contains("Conflict (409)"))
										{
											logger.Debug(ServiceError.RecordAlreadyExists);
											throw new ServiceException(ServiceError.RecordAlreadyExists, null);
										}
										else
										{
											logger.Error($"Create item from stream failed. Status code: {task.Result.StatusCode}. Message: {task.Result.ErrorMessage}");
											throw new Exception($"Create item from stream failed. Status code: {task.Result.StatusCode}. Message: {task.Result.ErrorMessage}");
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
							logger.Error("CosmosDB Insert Execution failed. Container not found.");
							throw new Exception("CosmosDB Insert Execution failed. Container not found.");
						}
					case ServiceCursorDef.CursorType.Update:
					if (container != null)
					{
						if (!keyCondition.Any() || !keyCondition.ContainsKey("id") || !keyCondition.ContainsKey(partitionKey))
						{
							logger.Error($"Update item failed: error parsing the query.");
							throw new Exception($"Update item failed: error parsing the query.");
						}
						else
						{
							try
							{
								logger.Debug($"Update : {jsonData}");

								using (MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(jsonData)))
								{
									Task<ResponseMessage> task = Task.Run<ResponseMessage>(async () => await container.UpsertItemStreamAsync(stream, new PartitionKey(partitionKeyValue)).ConfigureAwait(false));
									if (task.Result.IsSuccessStatusCode)
										return 1;
									else
									{
										logger.Error($"Update item from stream failed. Status code: {task.Result.StatusCode}. Message: {task.Result.ErrorMessage}");
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
						logger.Error("CosmosDB Update Execution failed. Container not found.");
						throw new Exception("CosmosDB Update Execution failed. Container not found.");
					}
			}
			return 0;
			
		}
		public override IDataReader ExecuteReader(ServiceCursorDef cursorDef, IDataParameterCollection parms, CommandBehavior behavior)
		{

			Initialize();
			CosmosDBQuery query = cursorDef.Query as CosmosDBQuery;
			//Get container from hashset for performance
			Container container = GetContainer(query?.TableName);
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

		private VarValue GetDataEqualParameterfromQueryVars(string filter, IEnumerable<VarValue> values, out string name)
		{
			string Equal_Filter_pattern = @"\((.*) = :(.*)\)";
			VarValue varValue = null;
			name = string.Empty;
			Match match = Regex.Match(filter, Equal_Filter_pattern);
			if (match.Groups.Count > 1)
			{
				string varName = match.Groups[2].Value;
				varName = varName.Remove(varName.Length - 1);
				name = match.Groups[1].Value;
				varValue = values.FirstOrDefault(v => v.Name == $":{varName}");
			}
			return varValue;
		}

		private string GetDataEqualParameterfromCollection(string filter, IDataParameterCollection parms, out string name)
		{
			string Equal_Filter_pattern = @"\((.*) = :(.*)\)";
			name = string.Empty;
			Match match = Regex.Match(filter, Equal_Filter_pattern);
			if (match.Groups.Count > 1)
			{
				string varName = match.Groups[2].Value;
				name = match.Groups[1].Value;
				varName = varName.Remove(varName.Length - 1);
				if (parms[varName] is ServiceParameter serviceParm)
				{
					return serviceParm.Value.ToString();
				}
			}
			return string.Empty;
		}

		private string GetDataParameterfromCollectionFormatted(string attName, IDataParameterCollection parms)
		{
			string varValuestr = string.Empty;
			if (parms[attName] is ServiceParameter serviceParm)
				if (GeneXus.Data.Cosmos.CosmosDBHelper.FormattedAsStringDbType(serviceParm.DbType))
				{
					varValuestr = '"' + $"{serviceParm.Value.ToString()}" + '"';
				}
				else
					varValuestr = serviceParm.Value.ToString();
			return varValuestr;
		}

		private CosmosDBDataReader GetDataReaderQueryByPK(ServiceCursorDef cursorDef, Container container, string idValue, string partitionKeyValue,out RequestWrapper requestWrapper)
		{
			requestWrapper = new RequestWrapper(cosmosClient, container, null);
			requestWrapper.idValue = idValue;
			requestWrapper.partitionKeyValue = partitionKeyValue;

			logger.Debug($"Execute PK query id = {requestWrapper.idValue}, partitionKey = {requestWrapper.partitionKeyValue}");
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
			if (query.Filters.Any())
			{
				if (query.Filters.Count() == 2)
				{
					string fieldValue1 = string.Empty;
					string fieldValue2 = string.Empty;

					fieldValue1 = GetDataEqualParameterfromQueryVars(query.Filters.First(), query.Vars, out string fieldName1)?.Value.ToString();
					fieldValue1 = fieldValue1 ?? GetDataEqualParameterfromCollection(query.Filters.First(), parms, out fieldName1);

					fieldValue2 = GetDataEqualParameterfromQueryVars(query.Filters.Skip(1).First(), query.Vars, out string fieldName2)?.Value.ToString();
					fieldValue2 = fieldValue2 ?? GetDataEqualParameterfromCollection(query.Filters.Skip(1).First(), parms, out fieldName2);

					if (fieldName1 == "id" && fieldName2 == query.PartitionKey)
					{
						cosmosDBDataReader = GetDataReaderQueryByPK(cursorDef, container, fieldValue1, fieldValue2, out requestWrapper);
						return;
					}
					else
					{
						if (fieldName1 == query.PartitionKey && fieldName2 == "id")
						{
							cosmosDBDataReader = GetDataReaderQueryByPK(cursorDef, container, fieldValue2, fieldValue1, out requestWrapper);
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
				foreach (VarValue item in query.Vars)
				{
					string varValuestr = string.Empty;
					if (filterProcess.Contains(string.Format($"{item.Name}:")))
					{
						if (GeneXus.Data.Cosmos.CosmosDBHelper.FormattedAsStringGXType(item.Type))
							varValuestr = '"' + $"{item.Value.ToString()}" + '"';
						else
						{
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
				filterProcess = filterProcess.Replace("[", "(");
				filterProcess = filterProcess.Replace("]", ")");
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

			logger.Debug(sqlQuery);

			QueryDefinition queryDefinition = new QueryDefinition(sqlQuery);
			requestWrapper = new RequestWrapper(cosmosClient, container, queryDefinition);
			requestWrapper.queryByPK = false;
			
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
