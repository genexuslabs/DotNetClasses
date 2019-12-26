using System.Web;
using GeneXus.Http.WebSocket;
using Microsoft.Web.WebSockets;


namespace GeneXus.Http
{
	public class WSHttpHandler : IHttpHandler
	{
		/// <summary>
		/// You will need to configure this handler in the Web.config file of your 
		/// web and register it with IIS before being able to use it. For more information
		/// see the following link: http://go.microsoft.com/?linkid=8101007
		/// </summary>
		#region IHttpHandler Members

		public bool IsReusable
		{
			
			get { return false; }
		}

		public void ProcessRequest(HttpContext context)
		{
			
			if (context != null && context.IsWebSocketRequest)
			{
				context.AcceptWebSocketRequest(new WSHandler());
			}
			else
			{
				throw new HttpException("This isn't a web socket request!");
			}
		}

		#endregion

	}
}
