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
			CallResult callResult = result as CallResult;
			try
			{
				GXLogging.Debug(log, "Calling Agent: ", assistant);
				return AgentService.AgentHandlerInstance.Assistant(assistant, (List<Chat.ChatMessage>)chatMessages, gxproperties).GetAwaiter().GetResult();
			}
			catch (Exception ex)
			{
				callResult.AddMessage($"Error calling Agent {assistant}:" + ex.Message);
				callResult.IsFail = true;
				return string.Empty;
			}

		}
	}
}
