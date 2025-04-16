using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using GeneXus.AI.Chat;
using GeneXus.Application;
using GeneXus.Configuration;
using GeneXus.Http;
using GeneXus.Http.Client;
using GeneXus.Utils;
using Microsoft.AspNetCore.Http;
namespace GeneXus.AI
{
	internal class AgentService
	{
		private static readonly IGXLogger log = GXLoggerFactory.GetLogger<AgentService>();
		const string SAIA_AGENT = "saia:agent:";
		protected string API_KEY;
		protected const string AI_PROVIDER = "AI_PROVIDER";
		protected const string AI_PROVIDER_API_KEY = "AI_PROVIDER_API_KEY";

		protected string DEFAULT_API_KEY => "default_";
		protected string DEFAULT_PROVIDER => "https://api.qa.saia.ai";
		protected string _providerUri;
		internal AgentService()
		{
			string val;
			_providerUri = DEFAULT_PROVIDER;
			API_KEY = DEFAULT_API_KEY;
			if (Config.GetValueOf(AI_PROVIDER, out val))
			{
				_providerUri = val;
			}
			_providerUri = AddChatToUrl(_providerUri);
			if (Config.GetValueOf(AI_PROVIDER_API_KEY, out val))
			{
				API_KEY = val;
			}
		}
		static string AddChatToUrl(string url)
		{
			var uriBuilder = new UriBuilder(url);
			if (!uriBuilder.Path.EndsWith("/"))
			{
				uriBuilder.Path += "/";
			}
			uriBuilder.Path += "chat";
			return uriBuilder.Uri.ToString();
		}
		internal async Task<ChatCompletionResult> CallAgent(string assistant, List<Chat.ChatMessage> messages, GXProperties properties, IGxContext context)
		{
			try
			{
				using (GxHttpClient httpClient = AgentHttpClient(context, assistant, messages, properties, false))
				{ 
					await httpClient.ExecuteAsync(HttpMethod.Post.Method, string.Empty);

					if (httpClient.StatusCode != (short)StatusCodes.Status200OK)
					{
						throw new Exception($"Request failed with status code: {httpClient.StatusCode}");
					}

					string responseJson = await httpClient.ToStringAsync();
					GXLogging.Debug(log, "Agent response:", responseJson);
					ChatCompletionResult chatCompletion = JsonSerializer.Deserialize<ChatCompletionResult>(responseJson);
					return chatCompletion;
				}
			}
			catch (Exception ex)
			{
				GXLogging.Error(log, "Error calling Agent ", assistant, ex);
				throw;
			}

		}
		internal GxHttpClient ChatAgent(string assistant, List<Chat.ChatMessage> messages, GXProperties properties, IGxContext context)
		{
			try
			{
				GxHttpClient httpClient = AgentHttpClient(context, assistant, messages, properties, true);

				httpClient.Execute(HttpMethod.Post.Method, string.Empty);

				if (httpClient.StatusCode != (short)StatusCodes.Status200OK)
				{
					throw new Exception($"Request failed with status code: {httpClient.StatusCode}");
				}

				return httpClient;
			}
			catch (Exception ex)
			{
				GXLogging.Error(log, "Error calling Agent ", assistant, ex);
				throw;
			}

		}
		internal GxHttpClient AgentHttpClient(IGxContext context, string assistant, List<Chat.ChatMessage> messages, GXProperties properties, bool stream)
		{
			GxHttpClient httpClient = new GxHttpClient(context);
			httpClient.Secure = 1;
			httpClient.AddHeader(HttpHeader.CONTENT_TYPE, "application/json");
			httpClient.AddHeader(HttpHeader.AUTHORIZATION, "Bearer " + API_KEY);
			string requestJson = AgentPaylod(assistant, messages, properties, stream);
			GXLogging.Debug(log, "Agent payload:", requestJson);

			httpClient.AddString(requestJson);
			httpClient.Url = _providerUri;
			return httpClient;
		}

		private string AgentPaylod(string assistant, List<ChatMessage> messages, GXProperties properties, bool stream)
		{
			ChatRequestPayload requestBody = new ChatRequestPayload();
			requestBody.Model = $"{SAIA_AGENT}{assistant}";
			requestBody.Messages = messages;
			requestBody.Variables = properties.ToList();
			requestBody.Stream = stream;

			JsonSerializerOptions options = new JsonSerializerOptions
			{
				WriteIndented = true
			};
			return JsonSerializer.Serialize(requestBody, options);

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
}
