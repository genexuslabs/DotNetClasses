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
				return AgentService.AgentHandlerInstance.Assistant(assistant, chatMessagesList, gxproperties).GetAwaiter().GetResult();
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
