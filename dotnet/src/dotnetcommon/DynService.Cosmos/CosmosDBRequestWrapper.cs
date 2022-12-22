using System;
using System.Threading.Tasks;
using log4net;
using Microsoft.Azure.Cosmos;

namespace GeneXus.Data.NTier.CosmosDB
{
	public class RequestWrapper
	{
		private readonly Container m_container;
		private readonly CosmosClient m_cosmosClient;
		private readonly QueryDefinition m_queryDefinition;
		static readonly ILog logger = log4net.LogManager.GetLogger(typeof(RequestWrapper));
		public string idValue { get; set; }
		public string partitionKeyValue { get; set; }
		public bool queryByPK { get; set; }
		public RequestWrapper(CosmosClient cosmosClient, Container container, QueryDefinition queryDefinition)
		{
			m_container = container;
			m_cosmosClient = cosmosClient;
			m_queryDefinition = queryDefinition;
		}

		private async Task<ResponseWrapper> ReadItemAsyncByPK(string idValue, string partitionKeyValue)
		{
			using (ResponseMessage responseMessage = await m_container.ReadItemStreamAsync(
				partitionKey: new PartitionKey(partitionKeyValue),
				id: idValue).ConfigureAwait(false))
			{

				if (!responseMessage.IsSuccessStatusCode)
				{
					if (!responseMessage.ErrorMessage.Contains("404"))
					{ 
						if (responseMessage.Diagnostics != null)
							GXLogging.Debug(logger, $"Read ReadItemAsyncByPK Diagnostics: {responseMessage.Diagnostics.ToString()}");
						throw new Exception(GeneXus.Data.Cosmos.CosmosDBHelper.FormatExceptionMessage(responseMessage.StatusCode.ToString(), responseMessage.ErrorMessage));
					}
				}
				return new ResponseWrapper(responseMessage);
			}
		}
			public ResponseWrapper Read()
		{
			if (queryByPK)
			{
				Task<ResponseWrapper> task = Task.Run<ResponseWrapper>(async () => await ReadItemAsyncByPK(idValue, partitionKeyValue).ConfigureAwait(false));
				return task.Result;
			}
			
			QueryRequestOptions requestOptions = new QueryRequestOptions() { MaxBufferedItemCount = 100 };
			//options.MaxConcurrency = 1;
			//TODO Cancelation Token + request options 
			using (FeedIterator feedIterator = m_container.GetItemQueryStreamIterator(m_queryDefinition, null, requestOptions))

			return new ResponseWrapper(feedIterator);
		}
		internal void Close()
		{
			m_cosmosClient.Dispose();
		}
	}
}
