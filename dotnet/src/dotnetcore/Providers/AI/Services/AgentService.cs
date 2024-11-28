using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using GeneXus.AI.Chat;
using GeneXus.Configuration;
using GeneXus.Utils;
namespace GeneXus.AI
{
	internal class AgentService
	{
		private static readonly IGXLogger log = GXLoggerFactory.GetLogger<AgentService>();
		const string SAIA_AGENT = "saia:agent:";
		private HttpClient _httpClient;
		protected string API_KEY;
		protected const string AI_PROVIDER = "AI_PROVIDER";
		protected const string AI_PROVIDER_API_KEY = "AI_PROVIDER_API_KEY";
		const string FINISH_REASON_STOP = "stop";
		const string FINISH_REASON_TOOL_CALLS = "toolcalls";

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
		internal async Task<string> Assistant(string assistant, List<Chat.ChatMessage> messages, GXProperties properties)
		{
			try
			{
				ChatRequestPayload requestBody = new ChatRequestPayload();
				requestBody.Model = $"{SAIA_AGENT}{assistant}";
				requestBody.Messages = messages;
				requestBody.Variables = properties.ToList();
				requestBody.Stream = false;

				JsonSerializerOptions options = new JsonSerializerOptions
				{
					WriteIndented = true
				};
				string requestJson = JsonSerializer.Serialize(requestBody, options);

				GXLogging.Debug(log, "Agent payload:", requestJson);

				var content = new StringContent(requestJson, Encoding.UTF8, "application/json");

				HttpResponseMessage response = await _httpClient.PostAsync(_providerUri, content);

				if (!response.IsSuccessStatusCode)
				{
					throw new Exception($"Request failed with status code: {response.StatusCode}");
				}

				string responseJson = await response.Content.ReadAsStringAsync();
				GXLogging.Debug(log, "Agent response:", responseJson);
				ChatCompletionResult chatCompletion = JsonSerializer.Deserialize<ChatCompletionResult>(responseJson);

				if (chatCompletion != null)
				{
					foreach (Choice choice in chatCompletion.Choices)
					{
						switch (choice.FinishReason.ToLower())
						{
							case FINISH_REASON_STOP:
								return choice.Message.Content;
							case FINISH_REASON_TOOL_CALLS:
								messages.Add(choice.Message);
								foreach (ToolCall toolCall in choice.Message.ToolCalls)
									ProcessTollCall(toolCall, messages);
								return await Assistant(assistant, messages, properties);
						}
					}
				}
				return string.Empty;
			}
			catch (Exception ex)
			{
				GXLogging.Error(log, "Error calling Agent ", assistant, ex);
				throw;
			}

		}

		private void ProcessTollCall(ToolCall toolCall, List<ChatMessage> messages)
		{
			string result = string.Empty;
			string functionName = toolCall.Function.Name;
			/*try
			{
				result = CallTool(functionName, toolCall.Function.Arguments);
			}
			catch (Exception ex)
			{
				GXLogging.Error(log, "Error calling tool ", functionName, ex);
				result = $"Error calling tool {functionName}";
			}*/
			ChatMessage toolCallMessage = new ChatMessage();
			toolCallMessage.Role = "tool";
			toolCallMessage.Content = result;
			toolCallMessage.ToolCallId = toolCall.Id;
			messages.Add(toolCallMessage);
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

	internal class NoAuthHeaderHandler : DelegatingHandler
	{
		protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			request.Headers.Authorization = null;
			return await base.SendAsync(request, cancellationToken);
		}
	}

	internal class ChatRequestPayload
	{
		[JsonPropertyName("model")]
		public string Model { get; set; }

		[JsonPropertyName("messages")]
		[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
		public List<Chat.ChatMessage> Messages { get; set; }

		[JsonPropertyName("stream")]
		public bool? Stream { get; set; }

		[JsonPropertyName("variables")]
		public List<GxKeyValuePair> Variables { get; set; }
	}

	internal class ChatCompletionResult
	{
		[JsonPropertyName("id")]
		public string Id { get; set; }

		[JsonPropertyName("object")]
		public string Object { get; set; }

		[JsonPropertyName("created")]
		public long Created { get; set; }

		[JsonPropertyName("choices")]
		public List<Choice> Choices { get; set; }

		[JsonPropertyName("usage")]
		public Usage Usage { get; set; }

		[JsonPropertyName("tool_calls")]
		public List<ChatMessage> ToolCalls { get; set; }

		[JsonPropertyName("data")]
		public List<DataItem> Data { get; set; }
	}
	public class Choice
	{
		[JsonPropertyName("index")]
		public int Index { get; set; }

		[JsonPropertyName("message")]
		public ChatMessage Message { get; set; }

		[JsonPropertyName("finish_reason")]
		public string FinishReason { get; set; }
	}


	public class Usage
	{
		[JsonPropertyName("prompt_tokens")]
		public int PromptTokens { get; set; }

		[JsonPropertyName("completion_tokens")]
		public int CompletionTokens { get; set; }

		[JsonPropertyName("total_tokens")]
		public int TotalTokens { get; set; }
	}

	public class DataItem
	{
		[JsonPropertyName("id")]
		public string Id { get; set; }

		[JsonPropertyName("object")]
		public string Object { get; set; }

		[JsonPropertyName("embedding")]
		public List<double> Embedding { get; set; }
	}
}
