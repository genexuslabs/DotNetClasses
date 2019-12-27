using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Runtime;
using GeneXus.Data.Dynamo;
using GeneXus.Data.NTier.DynamoDB;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text.RegularExpressions;
using System.Linq;
using GeneXus.Cache;

namespace GeneXus.Data.NTier
{

	public class DynamoDBService: GxService
	{
		public DynamoDBService(string id, string providerId): base(id, providerId, typeof(DynamoDBConnection))
		{

		}

		public override IDataReader GetCacheDataReader(CacheItem item, bool computeSize, string keyCache)
		{
			return new GxDynamoDBCacheDataReader(item, computeSize, keyCache);
		}
	}

	public class DynamoDBConnection : ServiceConnection
	{
		private readonly string CLIENT_ID = "User Id";
		private readonly string CLIENT_SECRET = "password";
		private readonly string REGION = "region";
		private AmazonDynamoDBClient mDynamoDB;
		private AWSCredentials mCredentials;
		private RegionEndpoint mRegion = RegionEndpoint.USEast1;

		public override string ConnectionString
		{
			get { return base.ConnectionString; }

			set
			{
				base.ConnectionString = value;
			}
		}

		private void InitializeDBConnection()
		{

			DbConnectionStringBuilder builder = new DbConnectionStringBuilder(false);
			builder.ConnectionString = this.ConnectionString;
			if (builder.TryGetValue(CLIENT_ID, out object clientId) && builder.TryGetValue(CLIENT_SECRET, out object clientSecret))
			{
				mCredentials = new BasicAWSCredentials(clientId.ToString(), clientSecret.ToString());
			}
			if (builder.TryGetValue(REGION, out object region))
			{
				mRegion = RegionEndpoint.GetBySystemName(region.ToString());
			}
		}
		private bool Initialize()
		{
			InitializeDBConnection();
			State = ConnectionState.Executing;
			mDynamoDB = new Amazon.DynamoDBv2.AmazonDynamoDBClient(mCredentials, mRegion);
			return true;
		}

		public override int ExecuteNonQuery(ServiceCursorDef cursorDef, IDataParameterCollection parms, CommandBehavior behavior)
		{
			Initialize();
			Query query = cursorDef.Query as Query;

			Dictionary<string, AttributeValue> values = new Dictionary<string, AttributeValue>();
			if (parms.Count > 0)
			{
				for (int i = 0; i < parms.Count; i++)
				{
					ServiceParameter parm = parms[i] as ServiceParameter;
					DynamoDBHelper.GXToDynamoQueryParameter("", values, parm);
				}
			}
			var pattern = @"\((.*) = :(.*)\)";
			Dictionary<string, AttributeValue> keyCondition = new Dictionary<string, AttributeValue>();
			List<string> filters = new List<string>();

			foreach (var keyFilter in query.Filters)
			{
				var match = Regex.Match(keyFilter, pattern);
				String varName = match.Groups[2].Value;
				if (match.Groups.Count > 1)
				{
					keyCondition[match.Groups[1].Value] = values[varName];
				}
			}
			AmazonDynamoDBRequest request = null;

			switch (query.CursorType)
			{
				case ServiceCursorDef.CursorType.Select:
					throw new NotImplementedException();

				case ServiceCursorDef.CursorType.Delete:
					request = new DeleteItemRequest()
					{
						TableName = query.TableName,
						Key = keyCondition
					};
					mDynamoDB.DeleteItem((DeleteItemRequest)request);

					break;
				case ServiceCursorDef.CursorType.Insert:
					request = new PutItemRequest
					{
						TableName = query.TableName,
						Item = values
					};
					mDynamoDB.PutItem((PutItemRequest)request);
					break;
				case ServiceCursorDef.CursorType.Update:
					request = new UpdateItemRequest
					{
						TableName = query.TableName,
						Key = keyCondition,
						AttributeUpdates = ToAttributeUpdates(keyCondition, values)
						
					};
					mDynamoDB.UpdateItem((UpdateItemRequest)request);
					break;

				default:
					break;
			}

			return 0;
		}

		private Dictionary<string, AttributeValueUpdate> ToAttributeUpdates(Dictionary<string, AttributeValue> keyConditions, Dictionary<string, AttributeValue> values)
		{
			Dictionary<string, AttributeValueUpdate> updates = new Dictionary<string, AttributeValueUpdate>();
			foreach (var item in values)
			{
				if (!keyConditions.ContainsKey(item.Key) && !item.Key.StartsWith("AV")) 
				{
					updates[item.Key] = new AttributeValueUpdate(item.Value, AttributeAction.PUT);
				}
			}
			return updates;
		}
		
		public override IDataReader ExecuteReader(ServiceCursorDef cursorDef, IDataParameterCollection parms, CommandBehavior behavior)
		{			
			
			Initialize();
			Query query = cursorDef.Query as Query;
			Dictionary<string, AttributeValue> valuesAux = new Dictionary<string, AttributeValue>();
			Dictionary<string, AttributeValue> values = new Dictionary<string, AttributeValue>();
			if (parms.Count > 0)
			{
				for (int i = 0; i < parms.Count; i++)
				{
					ServiceParameter parm = parms[i] as ServiceParameter;
					DynamoDBHelper.GXToDynamoQueryParameter(":", values, parm);
				}
			}
			
			List<string> filtersAux = new List<string>();
			List<string> filters = new List<string>();

			filters.AddRange(query.Filters);
			
			foreach (var item in query.Vars)
			{
				values.Add(item.Name, DynamoDBHelper.ToAttributeValue(item));
			}

			try
			{
				DynamoDBDataReader dataReader;
				AmazonDynamoDBRequest req;
				CreateDynamoQuery(query, values, filters.ToArray(), out dataReader, out req);
				RequestWrapper reqWrapper = new RequestWrapper(mDynamoDB, req);
				dataReader = new DynamoDBDataReader(cursorDef, reqWrapper, parms);
				return dataReader;
			}
			catch (AmazonDynamoDBException e) { throw e; }
			catch (AmazonServiceException e) { throw e; }
			catch (Exception e) { throw e; }
		}

		private static void CreateDynamoQuery(Query query, Dictionary<string, AttributeValue> values, String[] queryFilters, out DynamoDBDataReader dataReader, out AmazonDynamoDBRequest req)
		{
			dataReader = null;
			req = null;
			if (query is Scan)
			{
				req = new ScanRequest
				{
					TableName = query.TableName,
					ProjectionExpression = String.Join(",", query.Projection),
					
				};
				if (queryFilters.Length > 0)
				{
					((ScanRequest)req).FilterExpression = String.Join(" AND ", queryFilters);
					((ScanRequest)req).ExpressionAttributeValues = values;
				}
			}
			else
			{
				req = new QueryRequest
				{
					TableName = query.TableName,
					KeyConditionExpression = String.Join(" AND ", query.Filters),
					ExpressionAttributeValues = values,
					ProjectionExpression = String.Join(",", query.Projection),
					
				};
			}
		}

	}
}
