using System;
using System.Collections.Generic;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
#if NETCORE
using GeneXus.Data.Dynamo;
#endif

namespace GeneXus.Data.NTier.DynamoDB
{
	public class RequestWrapper
	{
		private readonly AmazonDynamoDBClient mDynamoDB;
		private readonly AmazonDynamoDBRequest mReq;

		public RequestWrapper(AmazonDynamoDBClient mDynamoDB, AmazonDynamoDBRequest req)
		{
			this.mDynamoDB = mDynamoDB;
			this.mReq = req;
		}

		public ResponseWrapper Read()
		{
			return Read(null);
		}

		public ResponseWrapper Read(Dictionary<string, AttributeValue> lastEvaluatedKey)
		{
			if (mReq is ScanRequest scanReq)
			{
				scanReq.ExclusiveStartKey = lastEvaluatedKey;
#if NETCORE
				ScanResponse scanResponse = DynamoDBHelper.RunSync<ScanResponse>(() => mDynamoDB.ScanAsync(scanReq));
#else
				ScanResponse scanResponse = mDynamoDB.Scan(scanReq);
#endif
				return new ResponseWrapper(scanResponse);
			}
			if (mReq is QueryRequest queryReq)
			{
				queryReq.ExclusiveStartKey = lastEvaluatedKey;
#if NETCORE
				QueryResponse queryResponse = DynamoDBHelper.RunSync<QueryResponse>(() => mDynamoDB.QueryAsync(queryReq));
#else
				QueryResponse queryResponse = mDynamoDB.Query(queryReq);
#endif
				return new ResponseWrapper(queryResponse);
			}
			throw new NotImplementedException();
		}

		internal void Close()
		{
			mDynamoDB.Dispose();
		}
	}
}
