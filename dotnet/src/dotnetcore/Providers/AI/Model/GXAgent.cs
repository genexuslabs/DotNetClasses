using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GeneXus.AI.Chat;
using GeneXus.Procedure;
using GeneXus.Utils;

namespace GeneXus.AI
{
	public class GXAgent : GXProcedure
	{
		static readonly IGXLogger log = GXLoggerFactory.GetLogger<GXAgent>();
		const string FINISH_REASON_STOP = "stop";
		const string FINISH_REASON_TOOL_CALLS = "toolcalls";

		protected string CallAssistant(string assistant, GXProperties properties, object result)
		{
			return CallAgent(assistant, properties, null, result);
		}
		protected string CallAgent(string assistant, GXProperties gxproperties, IList chatMessages, object result)
		{
			CallResult callResult = result as CallResult;
			try
			{
				GXLogging.Debug(log, "Calling Agent: ", assistant);

				List<ChatMessage> chatMessagesList = chatMessages!=null ? chatMessages.Cast<ChatMessage>().ToList() :null;
				ChatCompletionResult chatCompletion = AgentService.AgentHandlerInstance.Assistant(assistant, chatMessagesList, gxproperties).GetAwaiter().GetResult();

				if (chatCompletion != null && chatCompletion.Choices != null)
				{
					foreach (Choice choice in chatCompletion.Choices)
					{
						switch (choice.FinishReason.ToLower())
						{
							case FINISH_REASON_STOP:
								return choice.Message.Content;
							case FINISH_REASON_TOOL_CALLS:
								chatMessagesList.Add(choice.Message);
								foreach (ToolCall toolCall in choice.Message.ToolCalls)
									ProcessTollCall(toolCall, chatMessagesList);
								return CallAgent(assistant, gxproperties, chatMessagesList, result);
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
		private void ProcessTollCall(ToolCall toolCall, List<ChatMessage> messages)
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
			ChatMessage toolCallMessage = new ChatMessage();
			toolCallMessage.Role = "tool";
			toolCallMessage.Content = result;
			toolCallMessage.ToolCallId = toolCall.Id;
			messages.Add(toolCallMessage);
		}
		protected virtual string CallTool(string name, string arguments) 
		{
			return string.Empty;
		}


	}
}
