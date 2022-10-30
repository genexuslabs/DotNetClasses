using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;

namespace GeneXus.Data.NTier.CosmosDB
{
	public class RequestWrapper
	{
		private readonly Container mcontainer;
		private readonly CosmosClient mcosmosClient;
		private readonly QueryDefinition mqueryDefinition;
		//wrapper string idValue, string partitionKeyValue
		public RequestWrapper(CosmosClient cosmosClient, Container container, QueryDefinition queryDefinition)
		{
			mcontainer = container;
			mcosmosClient = cosmosClient;
			mqueryDefinition = queryDefinition;
		}

        //TODO: Por PK usar este metodo que es mas performante
		/*private async Task<ResponseWrapper> ReadItemAsyncByPK(string idValue, string partitionKeyValue)
		{
			using (ResponseMessage responseMessage = await mcontainer.ReadItemStreamAsync(
				partitionKey: new PartitionKey(partitionKeyValue),
				id: idValue).ConfigureAwait(false))
			{
				
				if (responseMessage.IsSuccessStatusCode)
				{
					return null;
					// Log the diagnostics
					//Console.WriteLine($"\n1.2.2 - Item Read Diagnostics: {responseMessage.Diagnostics.ToString()}");
				}
				else
				{
					return new ResponseWrapper(responseMessage);
					//Console.WriteLine($"Read item from stream failed. Status code: {responseMessage.StatusCode} Message: {responseMessage.ErrorMessage}");
				}
			}
		}*/
		public ResponseWrapper Read()
		{
		/*	if (queryByPK)
			{
				Task<ResponseWrapper> task = Task.Run<ResponseWrapper>(async () => await ReadItemAsyncByPK(idValue, partitionKeyValue).ConfigureAwait(false));
				return task.Result;
			}*/
			//GetItemQueryStreamIterator
			QueryRequestOptions requestOptions = new QueryRequestOptions() { MaxBufferedItemCount = 100 };
			//options.MaxConcurrency = 1;
			//TODO Cancelation Token + request options 
			using (FeedIterator feedIterator = mcontainer.GetItemQueryStreamIterator(mqueryDefinition, null, requestOptions))

			return new ResponseWrapper(feedIterator);
		}
		internal void Close()
		{
			mcosmosClient.Dispose();
		}
	}
}
