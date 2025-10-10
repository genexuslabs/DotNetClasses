using System;
using System.ClientModel;
using System.ClientModel.Primitives;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GeneXus.Configuration;
using GeneXus.Utils;
using OpenAI;
using OpenAI.Embeddings;
namespace GeneXus.AI
{
	public class EmbeddingService : IEmbeddingService
	{
		private static readonly IGXLogger log = GXLoggerFactory.GetLogger<EmbeddingService>();
		protected OpenAIClient _openAIClient;
		protected string API_KEY;
		protected virtual string DEFAULT_PROVIDER => "https://api.saia.ai";
		protected virtual string DEFAULT_API_KEY => "apitokenfortest_";
		protected const string AI_PROVIDER = "AI_PROVIDER";
		protected const string AI_PROVIDER_API_KEY = "AI_PROVIDER_API_KEY";
		private static volatile EmbeddingService m_instance;
		private static object m_SyncRoot = new Object();

		public EmbeddingService()
		{
			string val;
			Uri providerUri = new Uri(DEFAULT_PROVIDER);
			API_KEY = DEFAULT_API_KEY;
			if (Config.GetValueOf(AI_PROVIDER, out val))
			{
				providerUri = new Uri(val);
			}
			if (Config.GetValueOf(AI_PROVIDER_API_KEY, out val))
			{
				API_KEY = val;
			}
			OpenAIClientOptions options = new OpenAIClientOptions()
			{
				Endpoint = providerUri
			};

			_openAIClient = new OpenAIClient(new ApiKeyCredential(API_KEY), options);
		}
		public async Task<ReadOnlyMemory<float>> GenerateEmbeddingAsync(string model, int dimensions, string input)
		{
			OpenAIEmbeddingCollection data = await GenerateEmbeddingAsync(model, dimensions, new List<string> { input });
			return data.First().ToFloats();
		}
		async Task<OpenAIEmbeddingCollection> GenerateEmbeddingAsync(string model, int dimensions, IEnumerable<string> input)
		{
			try
			{
				EmbeddingClient client = _openAIClient.GetEmbeddingClient(model);
				EmbeddingGenerationOptions options = new EmbeddingGenerationOptions() { Dimensions = dimensions };
				ClientResult<OpenAIEmbeddingCollection> clientResult = await client.GenerateEmbeddingsAsync(input, options);
				return clientResult.Value;
			}
			catch (ClientResultException ex)
			{
				PipelineResponse response = ex.GetRawResponse();
				string status = response?.Status.ToString() ?? string.Empty;
				string reasonPhrase = response?.ReasonPhrase ?? string.Empty;
				string content = response?.Content?.ToString() ?? string.Empty;
				string headers = response?.Headers != null ? string.Join(", ", response.Headers.Select(h => $"{h.Key}: {h.Value}")) : string.Empty;
				GXLogging.Error(log, ex, $"GenerateEmbedding request failed. Status: {status} ReasonPhrase: {reasonPhrase} Content: {content} Headers: {headers}");
				throw;
			}
		}
		internal static EmbeddingService Instance
		{
			get
			{
				if (m_instance == null)
				{
					lock (m_SyncRoot)
					{
						if (m_instance == null)
							m_instance = new EmbeddingService();
					}
				}
				return m_instance;
			}
		}


	}
	
}
