using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using OpenAI;
using OpenAI.Embeddings;
namespace GeneXus.Utils
{
	public class GxEmbedding
	{
		IReadOnlyList<double> _embedding;
		public GxEmbedding()
		{
		}
		public GxEmbedding(string model, int dimensions)
		{
			Model = model;
			Dimensions = dimensions;
		}
		internal GxEmbedding(IReadOnlyList<double> embedding)
		{
			_embedding = embedding;
		}
		public static GxEmbedding GenerateEmbedding(GxEmbedding embeddingInfo, string text, GXBaseCollection<SdtMessages_Message> Messages)
		{
			try
			{
				IReadOnlyList<double> embedding = EmbeddingService.Instance.GenerateEmbeddingAsync(embeddingInfo.Model, embeddingInfo.Dimensions, text).GetAwaiter().GetResult();
				return new GxEmbedding(embedding);
			} catch (Exception ex)
			{
				GXUtil.ErrorToMessages("FromJson Error", ex, Messages, false);
				return embeddingInfo;
			}
		}
		public string Model { get; set; }
		public int Dimensions { get; set; }
	}
	internal interface IEmbeddingService
	{
		Task<IReadOnlyList<double>> GenerateEmbeddingAsync(string model, int dimensions, string input);
	}

	internal class EmbeddingService : IEmbeddingService
	{
		//static readonly IGXLogger log = GXLoggerFactory.GetLogger<EmbeddingService>();
		private static volatile EmbeddingService m_Instance;
		private static object m_SyncRoot = new Object();
		private readonly HttpClient _httpClient;
		private readonly OpenAIClient _openAIClient;
		private string API_KEY = "apitokenfortest";
		EmbeddingService()
		{
			_httpClient = new HttpClient(new SocketsHttpHandler
			{
				PooledConnectionLifetime = TimeSpan.FromMinutes(15.0)
			});
			_httpClient.DefaultRequestHeaders.Add("X-Saia-Source", "Embedding");
			OpenAIClientSettings openAIClientSettings = new OpenAIClientSettings(domain: "api.saia.ai", apiVersion: "chat");
			OpenAIAuthentication openAIAuthentication = new OpenAIAuthentication(API_KEY);
			_openAIClient = new OpenAIClient(openAIAuthentication, openAIClientSettings, _httpClient);

		}

		internal static EmbeddingService Instance
		{
			get
			{
				if (m_Instance == null)
				{
					lock (m_SyncRoot)
					{
						if (m_Instance == null)
							m_Instance = new EmbeddingService();
					}
				}
				return m_Instance;
			}
		}

		public async Task<IReadOnlyList<double>> GenerateEmbeddingAsync(string model, int dimensions, string input)
		{
			IReadOnlyList<Datum> data = await GenerateEmbeddingAsync(model, dimensions, new List<string> { input });
			return data.First().Embedding;
		}
		public async Task<IReadOnlyList<Datum>> GenerateEmbeddingAsync(string model, int dimensions, IEnumerable<string> input)
		{
			EmbeddingsRequest embeddingRequest = new EmbeddingsRequest(input, model, null, dimensions);
			EmbeddingsResponse embeddingResponse = await _openAIClient.EmbeddingsEndpoint.CreateEmbeddingAsync(embeddingRequest);
			return embeddingResponse.Data;
		}
	}
}
