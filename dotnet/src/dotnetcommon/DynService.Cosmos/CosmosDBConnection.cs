using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using GeneXus.Data.Cosmos;
using GeneXus.Data.NTier.CosmosDB;
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
		//private const string WHERE = "WHERE";
		//private const string DISTINCT = "DISTINCT";

		//LOG?

		//TODO: Usar un Hashset para guardar los containers

		private static readonly object _lock = new object();
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
			if (cosmosClient == null)
			{
				lock (_lock)
				{
					if (cosmosClient == null)
					{	
						if (!string.IsNullOrEmpty(mserviceURI) && !string.IsNullOrEmpty(mapplicationRegion))
							cosmosClient = new CosmosClient(mserviceURI, new CosmosClientOptions() { ApplicationRegion = mapplicationRegion });

						if (!string.IsNullOrEmpty(mdatabase))
							cosmosDatabase = cosmosClient.GetDatabase(mdatabase);
					}
				}				
			}
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

			Dictionary<string, object> values = new Dictionary<string, object>();
			string jsonData = string.Empty;

			string partitionKey = query.PartitionKey;
			string partitionKeyValue = string.Empty;
			foreach (KeyValuePair<string, string> asg in query.AssignAtts)
			{
				string name = asg.Key;
				string parmName = asg.Value.Substring(1);
				CosmosDBHelper.AddItemValue(isInsert ? name : $":{name}", parmName, values, parms, query.Vars, ref jsonData);
				if (name == partitionKey)
					partitionKeyValue = values[name].ToString();
			}
			jsonData = "{" + jsonData + "}";
			
			switch (query.CursorType)
			{
				case ServiceCursorDef.CursorType.Select:
					throw new NotImplementedException();

				case ServiceCursorDef.CursorType.Delete:

					//TODO
					throw new NotImplementedException();
				
				case ServiceCursorDef.CursorType.Insert:
					//TODO: Get container from HashSet for performance
					Container container = GetContainer(query.TableName);
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
									throw new Exception($"Create item from stream failed. Status code: {task.Result.StatusCode} Message: {task.Result.ErrorMessage}");

								}

							}
						}
						catch (Exception ex) // TODO check CosmosDB Exception https://learn.microsoft.com/en-us/dotnet/api/microsoft.azure.cosmos.cosmosexception?view=azure-dotnet
						{
							throw ex;
						}
					}
					else
					{
						throw new Exception("Execution failed. Container not found.");
					}
				case ServiceCursorDef.CursorType.Update:
					throw new NotImplementedException();
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
				CreateCosmosQuery(query,cursorDef, GetQueryValues(query, parms), container, out CosmosDBDataReader dataReader, out RequestWrapper requestWrapper);
				return dataReader;
			}
			catch (CosmosException cosmosException)
			{
				//TODO: Handle cases
				throw cosmosException;
			}

			catch (Exception e) { throw e; }
		}
		private void CreateCosmosQuery(CosmosDBQuery query,ServiceCursorDef cursorDef,Dictionary<string,object> queryValues, Container container, out CosmosDBDataReader cosmosDBDataReader,out RequestWrapper requestWrapper)
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
		
			string sqlQuery = $"{SELECT_CMD} {projectionList} {FROM} {tableName} {TABLE_ALIAS}";

			QueryDefinition queryDefinition = new QueryDefinition(sqlQuery);
			requestWrapper = new RequestWrapper(cosmosClient, container, queryDefinition);
			cosmosDBDataReader = new CosmosDBDataReader(cursorDef, requestWrapper);
		}
		private Dictionary<string, object> GetQueryValues(CosmosDBQuery query, IDataParameterCollection parms)
		{
			Dictionary<string, object> values = new Dictionary<string, object>();
			//TODO: Handle IDataParameterCollection params
			
			return values;
		}
		internal static IOServiceContext NewServiceContext() => null;
	}
	public class CosmosDBErrors
	{
		public const string ValidationException = "ValidationException";
		public const string ValidationExceptionMessageKey = "The AttributeValue for a key attribute cannot contain an empty string value.";
	}
}
