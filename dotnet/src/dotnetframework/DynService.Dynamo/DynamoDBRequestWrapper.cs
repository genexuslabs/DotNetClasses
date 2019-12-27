using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;

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
				return new ResponseWrapper(mDynamoDB.Scan((ScanRequest)mReq));
			}
			if (mReq is QueryRequest)
			{
				((QueryRequest)mReq).ExclusiveStartKey = lastEvaluatedKey;
				return new ResponseWrapper(mDynamoDB.Query((QueryRequest)mReq));
			}
			throw new NotImplementedException();
		}

		internal void Close()
		{
			mDynamoDB.Dispose();
		}
	}
}
