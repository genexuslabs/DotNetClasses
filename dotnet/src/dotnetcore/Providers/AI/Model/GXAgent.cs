using System;
using GeneXus.Procedure;
using GeneXus.Utils;
using OpenAI.Chat;

namespace GeneXus.AI
{
	public class GXAgent : GXProcedure
	{
		protected string CallAssistant(string assistant, GXProperties properties, object result)
		{
			ChatCompletion chatCompletion=null;
			CallResult callResult = result as CallResult;
			try
			{
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
				return chatCompletion.Content[0].Text;
			}
			else
			{
				return string.Empty;
			}
		}
	}
}
