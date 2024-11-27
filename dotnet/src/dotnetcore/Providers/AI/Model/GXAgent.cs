using System;
using System.Collections;
using System.Collections.Generic;
using GeneXus.Procedure;
using GeneXus.Utils;
using OpenAI.Chat;

namespace GeneXus.AI
{
	public class GXAgent : GXProcedure
	{
		static readonly IGXLogger log = GXLoggerFactory.GetLogger<GXAgent>();

		protected string CallAssistant(string assistant, GXProperties properties, object result)
		{
			return CallAgent(assistant, properties, null, result);
		}
		protected string CallAgent(string assistant, GXProperties gxproperties, IList chatMessages, object result)
		{
			ChatCompletion chatCompletion = null;
			CallResult callResult = result as CallResult;
			try
			{
				GXLogging.Debug(log, "Calling Agent: ", assistant);
				List<ChatMessage> messages = ChatMessagesToOpenAiChatMessages(chatMessages);
				chatCompletion = AgentService.AgentHandlerInstance.Assistant(assistant, messages, gxproperties).GetAwaiter().GetResult();
				if (chatCompletion != null && chatCompletion.Content != null && chatCompletion.Content.Count > 0)
				{
					GXLogging.Debug(log, "Agent response:", chatCompletion.Content[0].Text);
					return chatCompletion.Content[0].Text;
				}
				else
				{
					GXLogging.Debug(log, "Agent response is empty");
					return string.Empty;
				}
			}
			catch (Exception ex)
			{
				callResult.AddMessage($"Error calling Agent {assistant}:" + ex.Message);
				callResult.IsFail = true;
				return string.Empty;
			}

		}

		private List<ChatMessage> ChatMessagesToOpenAiChatMessages(IList chatMessages)
		{
			List<ChatMessage> messages = new List<ChatMessage>();

			if (chatMessages != null && chatMessages.Count > 0)
			{

				foreach (Chat.ChatMessage chatMessage in chatMessages)
				{
					if (!string.IsNullOrEmpty(chatMessage.Role))
					{
						if (chatMessage.Role.Equals(ChatMessageRole.User.ToString(), StringComparison.OrdinalIgnoreCase))
						{
							UserChatMessage userChatMessage = new UserChatMessage(chatMessage.Content);
							messages.Add(userChatMessage);
						}
						else if (chatMessage.Role.Equals(ChatMessageRole.Assistant.ToString(), StringComparison.OrdinalIgnoreCase))
						{
							AssistantChatMessage assistantChatMessage = new AssistantChatMessage(chatMessage.Content);
							messages.Add(assistantChatMessage);
						}
					}
				}
			}
			if (messages.Count == 0)
			{
				messages.Add(new UserChatMessage(string.Empty));
			}
			return messages;
		}
	}
}
