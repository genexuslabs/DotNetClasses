using Microsoft.Azure.Cosmos;
using System.Collections.Generic;
using System.IO;

namespace GeneXus.Data.NTier.CosmosDB
{
	public class ResponseWrapper
	{

		public FeedIterator feedIterator;
		public Stream stream;

		public ResponseWrapper(ResponseMessage responseMessage, FeedIterator feedIter)
		{
			feedIterator = feedIter;
			stream = responseMessage.Content;
		}
		public ResponseWrapper(FeedIterator feedIter)
		{
			feedIterator = feedIter;
		}
		public ResponseWrapper(ResponseMessage responseMessage)
		{
			stream = responseMessage.Content; 
		}
		public List<Dictionary<string, object>> Items { get; set; }
		public int ItemCount { get; set; }
		
	}
}
