using GeneXus.Procedure;
using GeneXus.Utils;
using OpenAI.Chat;

namespace GeneXus.AI
{
	internal class GXAgent : GXProcedure
	{
		protected string CallAssistant( string caller, GXProperties properties, CallResult result)
		{
			ChatCompletion chatCompletion = AgentService.AgentHandlerInstance.Assistant(caller, properties).GetAwaiter().GetResult();
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
