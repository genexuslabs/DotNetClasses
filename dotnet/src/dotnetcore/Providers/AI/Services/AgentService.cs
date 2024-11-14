using System;
using System.ClientModel;
using System.ClientModel.Primitives;
using System.Collections.Generic;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using GeneXus.Configuration;
using GeneXus.Utils;
using OpenAI;
using OpenAI.Chat;
namespace GeneXus.AI
{
	internal class AgentService
	{
		const string SerializedAdditionalRawDataPty = "SerializedAdditionalRawData";
		const string VARIABLES = "variables";
		const string SAIA_AGENT = "saia:agent:";
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

		internal async Task<ChatCompletion> Assistant(string assistant, string userMessage, GXProperties properties)
		{

			List<ChatMessage> messages = new List<ChatMessage>
			{
				new UserChatMessage(userMessage)
			};

			ChatCompletionOptions customOptions = new CustomChatCompletionOptions();
			if (properties != null && properties.Count > 0)
			{
				PropertyInfo fieldInfo = customOptions.GetType().GetProperty(SerializedAdditionalRawDataPty, BindingFlags.Instance | BindingFlags.NonPublic);
				IDictionary<string, BinaryData> SerializedAdditionalRawData = (IDictionary<string, BinaryData>)fieldInfo.GetValue(customOptions);
				SerializedAdditionalRawData = new Dictionary<string, BinaryData>
				{
					{ VARIABLES, BinaryData.FromString(properties.ToJSonString()) }
				};
				fieldInfo.SetValue(customOptions, SerializedAdditionalRawData);
			}
			ChatClient client = _openAIClient.GetChatClient($"{SAIA_AGENT}{assistant}");

			ClientResult<ChatCompletion> response = await client.CompleteChatAsync(messages, customOptions);

			//Console.Write(response.GetRawResponse().Content.ToString());
			return response.Value;

		}
		private static volatile AgentService m_Instance;
		private static object m_SyncRoot = new Object();
		internal static AgentService AgentHandlerInstance
		{
			get
			{
				if (m_Instance == null)
				{
					lock (m_SyncRoot)
					{
						if (m_Instance == null)
							m_Instance = new AgentService();
					}
				}
				return m_Instance;
			}
		}


	}

	internal class Variable
	{
		public string Key { get; set; }
		public string Value { get; set; }
	}

	internal class CustomChatCompletionOptions : ChatCompletionOptions
	{
		public List<Variable> Variables { get; set; }
	}
	internal class NoAuthHeaderHandler : DelegatingHandler
	{
		protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			request.Headers.Authorization = null;
			return await base.SendAsync(request, cancellationToken);
		}
	}
}
