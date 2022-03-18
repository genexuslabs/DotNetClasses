using Amazon.DynamoDBv2.Model;
using System.Collections.Generic;

namespace GeneXus.Data.NTier.DynamoDB
{
	public class ResponseWrapper
	{

		public ResponseWrapper(ScanResponse scanResponse)
		{
			Items = scanResponse.Items;
			ItemCount = scanResponse.Items.Count;
			LastEvaluatedKey = scanResponse.LastEvaluatedKey;
		}

		public ResponseWrapper(QueryResponse queryResponse)
		{
			Items = queryResponse.Items;
			ItemCount = queryResponse.Items.Count;
			LastEvaluatedKey = queryResponse.LastEvaluatedKey;
		}

		public List<Dictionary<string, AttributeValue>> Items { get; set; }
		public int ItemCount { get; set; }
		public Dictionary<string, AttributeValue> LastEvaluatedKey { get; set; }
		
	}
}
