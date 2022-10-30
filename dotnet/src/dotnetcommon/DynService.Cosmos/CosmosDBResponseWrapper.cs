using Microsoft.Azure.Cosmos;
using System.Collections.Generic;
using System.IO;

namespace GeneXus.Data.NTier.CosmosDB
{
	public class ResponseWrapper
	{

		public FeedIterator feedIterator;
		Stream stream;

		public ResponseWrapper(ResponseMessage responseMessage, FeedIterator feedIter)
		{
			feedIterator = feedIter;
			stream = responseMessage.Content;
			//Items = queryResponse.Items;
			//ItemCount = queryResponse.Items.Count;
		}
		public ResponseWrapper(FeedIterator feedIter)
		{
			feedIterator = feedIter;
			//stream = responseMessage.Content; 
			//Items = queryResponse.Items;
			//ItemCount = queryResponse.Items.Count;
		}

		public List<Dictionary<string, object>> Items { get; set; }
		public int ItemCount { get; set; }
		
	}
}
