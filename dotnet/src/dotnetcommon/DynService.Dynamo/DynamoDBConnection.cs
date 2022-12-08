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
using System.Net;
using System.Text;
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
			{
				mConfig.ServiceURL = mLocalUrl;
				if (region != null)
					mConfig.AuthenticationRegion = region as string;
			}
			else
				mConfig.RegionEndpoint = mRegion;
		}

		private void Initialize()
		{
			InitializeDBConnection();
			State = ConnectionState.Executing;

			mDynamoDB = new AmazonDynamoDBClient(mCredentials, mConfig);
		}

		private const string FILTER_PATTERN = @"\((.*) = :(.*)\)";

		public override int ExecuteNonQuery(ServiceCursorDef cursorDef, IDataParameterCollection parms, CommandBehavior behavior)
		{
			Initialize();
			DynamoQuery query = cursorDef.Query as DynamoQuery;
			bool isInsert = query!=null && query.CursorType == ServiceCursorDef.CursorType.Insert;

			Dictionary<string, AttributeValue> values = new Dictionary<string, AttributeValue>();
			Dictionary<string, string> expressionAttributeNames = null;
			HashSet<string> mappedNames = null;
			string keyItemForUpd = query.PartitionKey;
			if (keyItemForUpd != null && keyItemForUpd.StartsWith("#"))
			{
				expressionAttributeNames = new Dictionary<string, string>();
				mappedNames = mappedNames ?? new HashSet<string>();
				string keyName = keyItemForUpd.Substring(1);
				expressionAttributeNames.Add(keyItemForUpd, keyName);
				mappedNames.Add(keyName);
			}
			foreach (KeyValuePair<string, string> asg in query.AssignAtts)
			{
				string name = asg.Key;
				if (name.StartsWith("#"))
				{
					if (!isInsert)
					{
						expressionAttributeNames = new Dictionary<string, string>();
						mappedNames = mappedNames ?? new HashSet<string>();
						string keyName = name.Substring(1);
						expressionAttributeNames.Add(name, keyName);
						mappedNames.Add(keyName);
					}
					name = name.Substring(1);
				}
				string parmName = asg.Value.Substring(1);
				DynamoDBHelper.AddAttributeValue(isInsert ? name : $":{ name }", parmName, values, parms, query.Vars);
			}

			Dictionary<string, AttributeValue> keyCondition = new Dictionary<string, AttributeValue>();

			foreach (string keyFilter in query.KeyFilters.Concat(query.Filters))
			{
				Match match = Regex.Match(keyFilter, FILTER_PATTERN);
				if (match.Groups.Count > 1)
				{
					string varName = match.Groups[2].Value;
					string name = match.Groups[1].Value.TrimStart(SHARP_CHARS);
					VarValue varValue = query.Vars.FirstOrDefault(v => v.Name == $":{varName}");
					if (varValue != null)
						keyCondition[name] = DynamoDBHelper.ToAttributeValue(varValue);
					else
					{
						if (parms[varName] is ServiceParameter serviceParm)
							keyCondition[name] = DynamoDBHelper.ToAttributeValue(serviceParm.DbType, serviceParm.Value);
					}
				}
			}
			AmazonDynamoDBRequest request;
			AmazonWebServiceResponse response = null;

			switch (query.CursorType)
			{
				case ServiceCursorDef.CursorType.Select:
					throw new NotImplementedException();

				case ServiceCursorDef.CursorType.Delete:
					request = new DeleteItemRequest()
					{
						TableName = query.TableName,
						Key = keyCondition,
						ConditionExpression = $"attribute_exists({ keyItemForUpd })",
						ExpressionAttributeNames = expressionAttributeNames
					};
					try
					{
#if NETCORE
						response = DynamoDBHelper.RunSync<DeleteItemResponse>(() => mDynamoDB.DeleteItemAsync((DeleteItemRequest)request));
#else
						response = mDynamoDB.DeleteItem((DeleteItemRequest)request);
#endif
					}
					catch (ConditionalCheckFailedException recordNotFound)
					{
						throw new ServiceException(ServiceError.RecordNotFound, recordNotFound);
					}
					break;
				case ServiceCursorDef.CursorType.Insert:
					request = new PutItemRequest
					{
						TableName = query.TableName,
						Item = values,
						ConditionExpression = $"attribute_not_exists({ keyItemForUpd })",
						ExpressionAttributeNames = expressionAttributeNames
					};
					try
					{
#if NETCORE
						response = DynamoDBHelper.RunSync<PutItemResponse>(() => mDynamoDB.PutItemAsync((PutItemRequest)request));
#else
						response = mDynamoDB.PutItem((PutItemRequest)request);
#endif
					}
					catch (ConditionalCheckFailedException recordAlreadyExists)
					{
						throw new ServiceException(ServiceError.RecordAlreadyExists, recordAlreadyExists);
					}
					break;
				case ServiceCursorDef.CursorType.Update:
					request = new UpdateItemRequest
					{
						TableName = query.TableName,
						Key = keyCondition,
						UpdateExpression = ToAttributeUpdates(keyCondition, values, mappedNames),
						ConditionExpression = $"attribute_exists({ keyItemForUpd })",
						ExpressionAttributeValues = values,
						ExpressionAttributeNames = expressionAttributeNames
					};
					try
					{
#if NETCORE
						response = DynamoDBHelper.RunSync<UpdateItemResponse>(() => mDynamoDB.UpdateItemAsync((UpdateItemRequest)request));
#else
						response = mDynamoDB.UpdateItem((UpdateItemRequest)request);
#endif
					}
					catch (ConditionalCheckFailedException recordNotFound)
					{
						throw new ServiceException(ServiceError.RecordNotFound, recordNotFound);
					}
					break;
			}

			return response?.HttpStatusCode == HttpStatusCode.OK ? 1 : 0;
		}

		private string ToAttributeUpdates(Dictionary<string, AttributeValue> keyConditions, Dictionary<string, AttributeValue> values, HashSet<string> mappedNames)
		{
			StringBuilder updateExpression = new StringBuilder();
			foreach (var item in values)
			{
				string keyName = item.Key.Substring(1);
				if (!keyConditions.ContainsKey(keyName) && !keyName.StartsWith("AV", StringComparison.InvariantCulture))
				{
					if (mappedNames?.Contains(keyName) == true)
						keyName = $"#{keyName}";
					updateExpression.Append(updateExpression.Length == 0 ? "SET " : ", ");
					updateExpression.Append($"{ keyName } = { item.Key }");
				}
			}
			return updateExpression.ToString();
		}
		
		public override IDataReader ExecuteReader(ServiceCursorDef cursorDef, IDataParameterCollection parms, CommandBehavior behavior)
		{			
			
			Initialize();
			DynamoQuery query = cursorDef.Query as DynamoQuery;

			try
			{
				CreateDynamoQuery(query, GetQueryValues(query, parms), out DynamoDBDataReader dataReader, out AmazonDynamoDBRequest req);
				RequestWrapper reqWrapper = new RequestWrapper(mDynamoDB, req);
				dataReader = new DynamoDBDataReader(cursorDef, reqWrapper);
				return dataReader;
			}
			catch (AmazonDynamoDBException e)
			{
				if (e.ErrorCode == DynamoDBErrors.ValidationException &&
				    e.Message.Contains(DynamoDBErrors.ValidationExceptionMessageKey))
					throw new ServiceException(ServiceError.RecordNotFound); // Handles special case where a string key attribute is filtered with an empty value which is not supported on DynamoDB but should yield a not record found in GX
				throw e;
			}
			catch (AmazonServiceException e) { throw e; }
			catch (Exception e) { throw e; }
		}

		private Dictionary<string, AttributeValue> GetQueryValues(DynamoQuery query, IDataParameterCollection parms)
		{
			Dictionary<string, AttributeValue> values = new Dictionary<string, AttributeValue>();
			foreach (object parm in parms)
				DynamoDBHelper.GXToDynamoQueryParameter(values, parm as ServiceParameter);
			foreach (VarValue item in query.Vars)
				values.Add(item.Name, DynamoDBHelper.ToAttributeValue(item));
			return values;
		}

		private static void CreateDynamoQuery(DynamoQuery query, Dictionary<string, AttributeValue> values, out DynamoDBDataReader dataReader, out AmazonDynamoDBRequest req)
		{
			dataReader = null;
			req = null;
			Dictionary<string, string> expressionAttributeNames = null;
			foreach (string mappedName in query.SelectList.Where(selItem => (selItem as DynamoDBMap)?.NeedsAttributeMap == true).Select(selItem => selItem.GetName(NewServiceContext())))
			{
				expressionAttributeNames = expressionAttributeNames ?? new Dictionary<string, string>();
				string key = $"#{ mappedName }";
				string value = mappedName;
				expressionAttributeNames.Add(key, value);
			}

			bool issueScan = query is DynamoScan;
			if (!issueScan)
			{ // Check whether a query has to be demoted to scan due to empty parameters
				foreach (string keyFilter in query.KeyFilters)
				{
					Match match = Regex.Match(keyFilter, @".*(:.*)\).*");
					if (match.Groups.Count > 0)
					{
						string varName = match.Groups[1].Value;
						if(values.TryGetValue(varName, out AttributeValue value) && value.S?.Length == 0)
						{
							issueScan = true;
							break;
						}
					}
				}
			}

			if (issueScan)
			{
				ScanRequest scanReq;
				IEnumerable<string> allFilters = query.KeyFilters.Concat(query.Filters);
				req = scanReq = new ScanRequest
				{
					TableName = query.TableName,
					ProjectionExpression = String.Join(",", query.Projection),
					FilterExpression = allFilters.Any() ? String.Join(" AND ", allFilters) : null,
					ExpressionAttributeValues = values,
				};
				if (expressionAttributeNames != null)
					scanReq.ExpressionAttributeNames = expressionAttributeNames;
			}
			else
			{
				QueryRequest queryReq;
				req = queryReq = new QueryRequest
				{
					TableName = query.TableName,
					KeyConditionExpression = String.Join(" AND ", query.KeyFilters),
					FilterExpression = query.Filters.Any() ? String.Join(" AND ", query.Filters) : null,
					ExpressionAttributeValues = values,
					ProjectionExpression = String.Join(",", query.Projection),
					IndexName = query.Index,
					ScanIndexForward = query.ScanIndexForward,
				};
				if (expressionAttributeNames != null)
					queryReq.ExpressionAttributeNames = expressionAttributeNames;
			}
		}
		internal static IOServiceContext NewServiceContext() => null;

	}

	public class DynamoDBErrors
	{
		public const string ValidationException = "ValidationException";
		public const string ValidationExceptionMessageKey = "The AttributeValue for a key attribute cannot contain an empty string value.";
	}
}
