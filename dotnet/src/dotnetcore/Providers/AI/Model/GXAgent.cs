using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using GeneXus.AI.Chat;
using GeneXus.Http.Client;
using GeneXus.Procedure;
using GeneXus.Utils;

namespace GeneXus.AI
{
	public class GXAgent : GXProcedure
	{
		static readonly IGXLogger log = GXLoggerFactory.GetLogger<GXAgent>();

		protected ChatResult ChatAgent(String agent, GXProperties properties, IList chatMessages, object result)
		{
			CallResult callResult = result as CallResult;
			List<ChatMessage> chatMessagesList = chatMessages != null ? chatMessages.Cast<ChatMessage>().ToList() : null;
			try
			{
				GXLogging.Debug(log, "Chatting Agent: ", agent);

				GxHttpClient httpClient = AgentService.AgentHandlerInstance.ChatAgent(agent, chatMessagesList, properties, context);

				return new ChatResult(this, agent, properties, chatMessagesList, callResult, httpClient);
			}
			catch (Exception ex)
			{
				callResult.AddMessage($"Error chatting Agent {agent}:" + ex.Message);
				callResult.IsFail = true;
				return new ChatResult(this, agent, properties, chatMessagesList, callResult, null); 
			}
		}
		protected string CallAgent(string assistant, GXProperties gxproperties, IList chatMessages, object result)
		{
			return CallAgent(assistant, gxproperties, chatMessages, result, false);
		}
		protected string CallAgent(string assistant, GXProperties gxproperties, IList chatMessages, object result, bool stream)
		{
			CallResult callResult = result as CallResult;
			try
			{
				GXLogging.Debug(log, "Calling Agent: ", assistant);

				List<ChatMessage> chatMessagesList = chatMessages!=null ? chatMessages.Cast<ChatMessage>().ToList() :null;
				ChatCompletionResult chatCompletion = AgentService.AgentHandlerInstance.CallAgent(assistant, chatMessagesList, gxproperties, context).GetAwaiter().GetResult();

				if (chatCompletion != null && chatCompletion.Choices != null)
				{
					foreach (Choice choice in chatCompletion.Choices)
					{
						switch (choice.FinishReason.ToLower())
						{
							case ChatCompletionResult.FINISH_REASON_STOP:
								return choice.Message.Content;
							case ChatCompletionResult.FINISH_REASON_TOOL_CALLS:
								chatMessagesList.Add(choice.Message);
								return ProcessChatResponse(choice, stream, assistant, gxproperties, chatMessagesList, result);	
						}
					}
				}
				return string.Empty;
			}
			catch (Exception ex)
			{
				callResult.AddMessage($"Error calling Agent {assistant}:" + ex.Message);
				callResult.IsFail = true;
				return string.Empty;
			}

		}
		internal override string ProcessChatResponse(Choice choice, bool stream, string assistant, GXProperties gxproperties, List<ChatMessage> chatMessagesList, object result)
		{
			foreach (ToolCall toolCall in choice.Message.ToolCalls)
				ProcessToolCall(toolCall, chatMessagesList);
			return CallAgent(assistant, gxproperties, chatMessagesList, result, stream);
		}
		private void ProcessToolCall(ToolCall toolCall, List<ChatMessage> messages)
		{
			string result = string.Empty;
			string functionName = toolCall.Function.Name;
			try
			{
				result = CallTool(functionName, toolCall.Function.Arguments);
			}
			catch (Exception ex)
			{
				GXLogging.Error(log, "Error calling tool ", functionName, ex);
				result = $"Error calling tool {functionName}";
			}
			ChatMessage toolCallMessage = new ChatMessage
			{
				Role = "tool",
				Content = result,
				ToolCallId = toolCall.Id
			};
			messages.Add(toolCallMessage);
		}
		protected virtual string CallTool(string name, string arguments) 
		{
			return string.Empty;
		}


	}
}
