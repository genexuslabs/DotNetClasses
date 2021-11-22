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
		private readonly string LOCAL_URL = "LocalUrl";
		private readonly char[] SHARP_CHARS = new char[] { '#' };
		private AmazonDynamoDBClient mDynamoDB;
		private AmazonDynamoDBConfig mConfig;
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
			mConfig = new AmazonDynamoDBConfig();
			string mLocalUrl = null;
			if (builder.TryGetValue(LOCAL_URL, out object localUrl))
			{
				mLocalUrl = localUrl.ToString();
			}
			if (builder.TryGetValue(CLIENT_ID, out object clientId) && builder.TryGetValue(CLIENT_SECRET, out object clientSecret))
			{
				mCredentials = new BasicAWSCredentials(clientId.ToString(), clientSecret.ToString());
			}
			if (builder.TryGetValue(REGION, out object region))
			{
				mRegion = RegionEndpoint.GetBySystemName(region.ToString());
			}
			if (localUrl != null)
				mConfig.ServiceURL = mLocalUrl;
			else mConfig.RegionEndpoint = mRegion;
		}

		private bool Initialize()
		{
			InitializeDBConnection();
			State = ConnectionState.Executing;

			mDynamoDB = new AmazonDynamoDBClient(mCredentials, mConfig);
			return true;
		}

		public override int ExecuteNonQuery(ServiceCursorDef cursorDef, IDataParameterCollection parms, CommandBehavior behavior)
		{
			Initialize();
			Query query = cursorDef.Query as Query;

			Dictionary<string, AttributeValue> values = new Dictionary<string, AttributeValue>();
			foreach (KeyValuePair<string, string> asg in query.AssignAtts)
			{
				string name = asg.Key.TrimStart(SHARP_CHARS);
				string parmName = asg.Value.Substring(1);
				DynamoDBHelper.AddAttributeValue(name, values, parms[parmName] as ServiceParameter);
			}

			string pattern = @"\((.*) = :(.*)\)";
			Dictionary<string, AttributeValue> keyCondition = new Dictionary<string, AttributeValue>();
			List<string> filters = new List<string>();

			foreach (string keyFilter in query.Filters)
			{
				Match match = Regex.Match(keyFilter, pattern);
				string varName = match.Groups[2].Value;
				if (match.Groups.Count > 1)
				{
					string name = match.Groups[1].Value.TrimStart(SHARP_CHARS);
					DynamoDBHelper.AddAttributeValue(name, values, parms[varName] as ServiceParameter);
					keyCondition[name] = values[name];
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
			DynamoQuery query = cursorDef.Query as DynamoQuery;
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
					
			foreach (VarValue item in query.Vars)
			{
				values.Add(item.Name, DynamoDBHelper.ToAttributeValue(item));
			}

			try
			{
				DynamoDBDataReader dataReader;
				AmazonDynamoDBRequest req;
				CreateDynamoQuery(query, values, out dataReader, out req);
				RequestWrapper reqWrapper = new RequestWrapper(mDynamoDB, req);
				dataReader = new DynamoDBDataReader(cursorDef, reqWrapper, parms);
				return dataReader;
			}
			catch (AmazonDynamoDBException e) { throw e; }
			catch (AmazonServiceException e) { throw e; }
			catch (Exception e) { throw e; }
		}

		private static void CreateDynamoQuery(DynamoQuery query, Dictionary<string, AttributeValue> values, out DynamoDBDataReader dataReader, out AmazonDynamoDBRequest req)
		{
			dataReader = null;
			req = null;
			ScanRequest scanReq = null;
			QueryRequest queryReq = null;
			if (query is DynamoScan)
			{
				req = scanReq = new ScanRequest
				{
					TableName = query.TableName,
					ProjectionExpression = String.Join(",", query.Projection),					
				};
				if (query.Filters.Length > 0)
				{
					scanReq.FilterExpression = String.Join(" AND ", query.Filters);
					scanReq.ExpressionAttributeValues = values;
				}
			}
			else
			{
				req = queryReq = new QueryRequest
				{
					TableName = query.TableName,
					KeyConditionExpression = String.Join(" AND ", query.Filters),
					ExpressionAttributeValues = values,
					ProjectionExpression = String.Join(",", query.Projection),
					IndexName = query.Index,
					ScanIndexForward = query.ScanIndexForward,
				};
			}
			Dictionary<string, string> expressionAttributeNames = null;
			foreach (string mappedName in query.SelectList.Where(selItem => (selItem as DynamoDBMap)?.NeedsAttributeMap == true).Select(selItem => selItem.GetName(NewServiceContext())))
			{
				expressionAttributeNames = expressionAttributeNames ?? new Dictionary<string, string>();
				string key = $"#{ mappedName }";
				string value = mappedName;
				expressionAttributeNames.Add(key, value);
			}
			if(expressionAttributeNames != null)
			{
				if(scanReq != null)
					scanReq.ExpressionAttributeNames = expressionAttributeNames;
				else if(queryReq != null)
					queryReq.ExpressionAttributeNames = expressionAttributeNames;
			}
		}
		internal static IOServiceContext NewServiceContext() => null;

	}
}
