using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using GeneXus.Http.Client;
using GeneXus.Procedure;
using GeneXus.Utils;

namespace GeneXus.AI.Chat
{
	public class ChatResult: IDisposable
	{
#if NETCORE
		static readonly IGXLogger log = GXLoggerFactory.GetLogger<ChatResult>();
#endif

		private GxHttpClient Client { get; set; }
		private string Agent { get; set; }
		private GXProperties Properties { get; set; }
		private List<ChatMessage> Messages { get; set; }
		private CallResult Result { get; set; }
		private GXProcedure AgentProcedure { get; set; }
		private bool disposed = false;
		public ChatResult()
		{
		}

		public ChatResult(GXProcedure agentProcedure, string agent, GXProperties properties, List<ChatMessage> messages, CallResult result, GxHttpClient client)
		{
			AgentProcedure = agentProcedure;
			Agent = agent;
			Properties = properties;
			Messages = messages;
			Result = result;
			Client = client;
		}
		public bool HasMoreData()
		{
			return !Client.Eof;
		}
		public string GetMoreData()
		{
#if NETCORE
			string data = Client.ReadChunk();
			if (string.IsNullOrEmpty(data))
				return string.Empty;
			int index = data.IndexOf(ChatCompletionResult.DATA) + ChatCompletionResult.DATA.Length;
			string chunkJson = data.Substring(index).Trim();
			try
			{
				ChatCompletionResult chatCompletion = JsonSerializer.Deserialize<ChatCompletionResult>(chunkJson);
				if (chatCompletion?.Choices != null && chatCompletion.Choices.Count > 0)
				{
					Choice choice = chatCompletion.Choices[0];
					if (choice.FinishReason.ToLower() == ChatCompletionResult.FINISH_REASON_TOOL_CALLS && Agent != null)
					{
						Messages.Add(choice.Message);
						return AgentProcedure.ProcessChatResponse(choice, true, Agent, Properties, Messages, Result);
					}
					else
					{
						return choice.Message.Content ?? string.Empty;
					}
				}
			}
			catch (Exception ex)
			{
				GXLogging.Error(log, "Error processing chat response:", data, ex);
			}
#endif
			return string.Empty;
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposed)
				return;

			if (disposing)
			{
				Client?.Dispose();
			}

			disposed = true;
		}

		~ChatResult()
		{
			Dispose(false);
		}
	}
#if NETCORE
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
		internal const string FINISH_REASON_STOP = "stop";
		internal const string FINISH_REASON_TOOL_CALLS = "tool_calls";
		internal const string DATA = "data:";

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
#endif
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
