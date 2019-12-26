using System;
using GeneXus.Http.WebSocket;
using log4net;

namespace GeneXus.Notifications.WebSocket
{
	public class GXWebSocketFactory
    {
        private static readonly ILog log = log4net.LogManager.GetLogger(typeof(GXWebSocketFactory));

        public static IGXWebSocketAsync GetWebSocketProvider()
        {
            IGXWebSocketAsync ws = null;
            try
            {
#if NETCORE
				ws = new WSHandler();
#else
                Type t = Type.GetType("GeneXus.Http.WebSocket.WSHandler,gxwebsocket");
				ws = (IGXWebSocketAsync)Activator.CreateInstance(t);
#endif
                GXLogging.Debug(log, "ASP.NET Websocket instance created ok");
            }
            catch (Exception e)
            {                
                GXLogging.Error(log, "Could not initialize ASP.NET WebSocket ", e);                
            }
            return ws;
        }
    }
}
