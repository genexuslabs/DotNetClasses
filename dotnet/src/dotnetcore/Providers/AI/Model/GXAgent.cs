using System;
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
			ChatCompletion chatCompletion=null;
			CallResult callResult = result as CallResult;
			try
			{
				GXLogging.Debug(log, "Calling Agent: ",assistant);
				chatCompletion = AgentService.AgentHandlerInstance.Assistant(assistant, string.Empty, properties).GetAwaiter().GetResult();
			}
			catch (Exception ex)
			{
				callResult.AddMessage($"Error calling Agent {assistant}:" + ex.Message);
				callResult.IsFail = true;
				return null;
			}
			if (chatCompletion != null && chatCompletion.Content.Count > 0)
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
	}
}
