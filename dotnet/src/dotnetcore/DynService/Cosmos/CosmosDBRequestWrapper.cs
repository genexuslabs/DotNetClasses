using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using GeneXus.Data.Cosmos;
using log4net;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;

namespace GeneXus.Data.NTier.CosmosDB
{
	public class RequestWrapper
	{
		private readonly Container m_container;
		private readonly CosmosClient m_cosmosClient;
		private readonly QueryDefinition m_queryDefinition;
		static readonly ILog logger = log4net.LogManager.GetLogger(typeof(RequestWrapper));
		public string idValue { get; set; }
		public object partitionKeyValue { get; set; }
		public bool queryByPK { get; set; }
		public RequestWrapper(CosmosClient cosmosClient, Container container, QueryDefinition queryDefinition)
		{
			m_container = container;
			m_cosmosClient = cosmosClient;
			m_queryDefinition = queryDefinition;
		}

		private List<Dictionary<string,object>> ProcessPKStream(Stream stream)
		{
			//Query by PK -> only one record

			List <Dictionary<string, object>> Items = new List<Dictionary<string, object>>();
			if (stream != null)
			{
				using (StreamReader sr = new StreamReader(stream))
				using (JsonTextReader jtr = new JsonTextReader(sr))
				{
					Newtonsoft.Json.JsonSerializer jsonSerializer = new Newtonsoft.Json.JsonSerializer();
					object array = jsonSerializer.Deserialize<object>(jtr);

					Dictionary<string, object> result = JsonConvert.DeserializeObject<Dictionary<string, object>>(array.ToString());

					//remove metadata
					result.Remove("_rid");
					result.Remove("_self");
					result.Remove("_etag");
					result.Remove("_attachments");
					result.Remove("_ts");
					Items.Add(result);
				}
			}
			return Items;
		}
		private async Task<ResponseWrapper> ReadItemAsyncByPK(string idValue, object partitionKeyValue)
		{
			List<Dictionary<string, object>> Items = new List<Dictionary<string, object>>();
			using (ResponseMessage responseMessage = await m_container.ReadItemStreamAsync(
				partitionKey: CosmosDBHelper.ToPartitionKey(partitionKeyValue),
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
				else
				{
					Items = ProcessPKStream(responseMessage.Content);
				}
			}
			return new ResponseWrapper(Items);
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
			using (FeedIterator feedIterator = m_container.GetItemQueryStreamIterator(m_queryDefinition, null, requestOptions))

			return new ResponseWrapper(feedIterator);
		}
		internal void Close()
		{
			m_cosmosClient.Dispose();
		}
	}
}
