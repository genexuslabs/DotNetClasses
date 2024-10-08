using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using GeneXus.Configuration;
using OpenAI;
using OpenAI.Embeddings;
namespace GeneXus.Utils
{
	public class GxEmbedding
	{

		ReadOnlyMemory<float> _embedding;
		public GxEmbedding()
		{
		}
		public GxEmbedding(string model, int dimensions)
		{
			Model = model;
			Dimensions = dimensions;
			_embedding = new ReadOnlyMemory<float>(new float[dimensions]);
		}
		internal GxEmbedding(IReadOnlyList<double> embedding)
		{
			_embedding = embedding.Select(f => (float)f).ToArray();
		}
		internal GxEmbedding(float[] embedding)
		{
			_embedding = embedding;
		}

		internal static GxEmbedding Empty(string model, int dimensions)
		{
			return new GxEmbedding(model, dimensions);
		}
		public override string ToString()
		{
			return $"[{string.Join(",", _embedding.Span.ToArray())}]";
		}
		public static GxEmbedding GenerateEmbedding(GxEmbedding embeddingInfo, string text, GXBaseCollection<SdtMessages_Message> Messages)
		{
			try
			{
				IReadOnlyList<double> embedding = EmbeddingService.Instance.GenerateEmbeddingAsync(embeddingInfo.Model, embeddingInfo.Dimensions, text).GetAwaiter().GetResult();
				return new GxEmbedding(embedding);
			} catch (Exception ex)
			{
				GXUtil.ErrorToMessages("GenerateEmbedding Error", ex, Messages, false);
				return embeddingInfo;
			}
		}
		public string Model { get; set; }
		public int Dimensions { get; set; }

		internal ReadOnlyMemory<float> Data => _embedding; 
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
		private const string DEFAULT_DOMAIN = "api.saia.ai";
		private const string DEFAULT_VERSION = "chat";
		private const string AI_PROVIDER = "AI_PROVIDER";
		private const string AI_PROVIDER_API_KEY= "AI_PROVIDER_API_KEY";
		EmbeddingService()
		{
			_httpClient = new HttpClient(new SocketsHttpHandler
			{
				PooledConnectionLifetime = TimeSpan.FromMinutes(15.0)
			});

			string val, domain= DEFAULT_DOMAIN;
			string version = DEFAULT_VERSION;
			if (Config.GetValueOf(AI_PROVIDER, out val))
			{
				Uri providerUri = new Uri(val);
				domain = providerUri.GetLeftPart(UriPartial.Authority);
				version = providerUri.AbsolutePath;
			}
			if (Config.GetValueOf(AI_PROVIDER_API_KEY, out val))
			{
				API_KEY = val;
			}
			OpenAIClientSettings openAIClientSettings = new OpenAIClientSettings(domain, version);
			_httpClient.DefaultRequestHeaders.Add("X-Saia-Source", "Embedding");
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
