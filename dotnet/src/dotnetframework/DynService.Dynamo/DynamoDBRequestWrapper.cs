using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using GeneXus.Data.Dynamo;

namespace GeneXus.Data.NTier.DynamoDB
{
	public class RequestWrapper
	{
		private AmazonDynamoDBClient mDynamoDB;
		private AmazonDynamoDBRequest mReq;

		public RequestWrapper()
		{

		}

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
			if (mReq is ScanRequest)
			{
				((ScanRequest)mReq).ExclusiveStartKey = lastEvaluatedKey;
				ScanResponse scanResponse;
#if NETCORE
				scanResponse = DynamoDBHelper.RunSync<ScanResponse>(() => mDynamoDB.ScanAsync((ScanRequest)mReq));
#else
				scanResponse = mDynamoDB.Scan((ScanRequest)mReq);
#endif
				return new ResponseWrapper(scanResponse);
			}
			if (mReq is QueryRequest)
			{
				((QueryRequest)mReq).ExclusiveStartKey = lastEvaluatedKey;
				QueryResponse queryResponse;
#if NETCORE
				queryResponse = DynamoDBHelper.RunSync<QueryResponse>(() => mDynamoDB.QueryAsync((QueryRequest)mReq));
#else
				queryResponse = mDynamoDB.Query((QueryRequest)mReq);
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
