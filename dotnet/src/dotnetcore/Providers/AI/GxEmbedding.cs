using System;
using System.ClientModel;
using System.ClientModel.Primitives;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using GeneXus.Configuration;
using OpenAI;
using OpenAI.Chat;
using OpenAI.Embeddings;
namespace GeneXus.Utils
{
	internal class OpenAiService
	{
		protected OpenAIClient _openAIClient;
		protected string API_KEY;
		protected virtual string DEFAULT_PROVIDER => "https://api.saia.ai/embeddings";
		protected virtual string DEFAULT_API_KEY => "apitokenfortest_";
		protected const string AI_PROVIDER = "AI_PROVIDER";
		protected const string AI_PROVIDER_API_KEY = "AI_PROVIDER_API_KEY";
		internal OpenAiService()
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
		public async Task<OpenAIEmbeddingCollection> GenerateEmbeddingAsync(string model, int dimensions, IEnumerable<string> input)
		{
			EmbeddingClient client = _openAIClient.GetEmbeddingClient(model);
			EmbeddingGenerationOptions options = new EmbeddingGenerationOptions() { Dimensions = dimensions };
			ClientResult<OpenAIEmbeddingCollection> clientResult = await client.GenerateEmbeddingsAsync(input, options);

			return clientResult.Value;
		}

	}
	internal class AgentService
	{
		private HttpClient _httpClient;
		protected string API_KEY;
		protected OpenAIClient _openAIClient;
		protected const string AI_PROVIDER = "AI_PROVIDER";
		protected const string AI_PROVIDER_API_KEY = "AI_PROVIDER_API_KEY";

		protected string DEFAULT_API_KEY => "default_";
		protected string DEFAULT_PROVIDER => "https://api.beta.saia.ai/chat";
		internal AgentService()
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

			var handler = new SocketsHttpHandler
			{
				PooledConnectionLifetime = TimeSpan.FromMinutes(15.0),
				//Proxy = new WebProxy("http://localhost:8888", false),
				//UseProxy = true
			};

			var noAuthHandler = new NoAuthHeaderHandler
			{
				InnerHandler = handler
			};
			_httpClient = new HttpClient(noAuthHandler);
			_httpClient.DefaultRequestHeaders.Add("Saia-Auth", API_KEY);

			OpenAIClientOptions options = new OpenAIClientOptions()
			{
				Endpoint = providerUri
			};
			options.Transport = new HttpClientPipelineTransport(_httpClient);

			_openAIClient = new OpenAIClient(new ApiKeyCredential(API_KEY), options);
		}

		internal async Task<ChatCompletion> Assistant(string userMessage, string context)
		{

			List<ChatMessage> messages = new List<ChatMessage>
			{
				new SystemChatMessage($"$context: {context}"),
				new UserChatMessage(userMessage)
			};

			ChatCompletionOptions customOptions = new CustomChatCompletionOptions();
			PropertyInfo fieldInfo = customOptions.GetType().GetProperty("SerializedAdditionalRawData", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
			IDictionary<string, BinaryData> SerializedAdditionalRawData = (IDictionary<string, BinaryData>)fieldInfo.GetValue(customOptions);
			SerializedAdditionalRawData = new Dictionary<string, BinaryData>();
			List<object> variables = new List<object>
			{
				new { key = "$context", value = context },
				new { key = "$location", value = "location" }
			};
			SerializedAdditionalRawData.Add("variables", BinaryData.FromString("{\"key\": \"$context\", \"value\": \"context string\"}"));
			fieldInfo.SetValue(customOptions, SerializedAdditionalRawData);

			ChatClient client = _openAIClient.GetChatClient("saia:agent:e4e7a837-b8ad-4d25-b2db-431dda9af0af");

			ClientResult<ChatCompletion> response = await client.CompleteChatAsync(messages, customOptions);

			Console.Write(response.GetRawResponse().Content.ToString());
			return response.Value;

		}

	}
	internal class AIService : IAIService
	{
		//static readonly IGXLogger log = GXLoggerFactory.GetLogger<AIService>();
		private static volatile OpenAiService m_EmbeddingInstance;
		private static volatile AgentService m_AgentInstance;
		private static object m_SyncRoot = new Object();

		internal static OpenAiService EmbeddingHandlerInstance
		{
			get
			{
				if (m_EmbeddingInstance == null)
				{
					lock (m_SyncRoot)
					{
						if (m_EmbeddingInstance == null)
							m_EmbeddingInstance = new OpenAiService();
					}
				}
				return m_EmbeddingInstance;
			}
		}
		internal static AgentService AgentHandlerInstance
		{
			get
			{
				if (m_AgentInstance == null)
				{
					lock (m_SyncRoot)
					{
						if (m_AgentInstance == null)
							m_AgentInstance = new AgentService();
					}
				}
				return m_AgentInstance;
			}
		}
		public async Task<OpenAIEmbeddingCollection> GenerateEmbeddingAsync(string model, int dimensions, IEnumerable<string> input)
		{
			return await EmbeddingHandlerInstance.GenerateEmbeddingAsync(model, dimensions, input);
		}

		public Task<ReadOnlyMemory<float>> GenerateEmbeddingAsync(string model, int dimensions, string input)
		{
			return EmbeddingHandlerInstance.GenerateEmbeddingAsync(model, dimensions, input);
		}
	}
		public class Variable
	{
		public string Key { get; set; }
		public string Value { get; set; }
	}

	public class CustomChatCompletionOptions : ChatCompletionOptions
	{
		public List<Variable> Variables { get; set; }
	}
	public class NoAuthHeaderHandler : DelegatingHandler
	{
		protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			request.Headers.Authorization = null;
			return await base.SendAsync(request, cancellationToken);
		}
	}
}
