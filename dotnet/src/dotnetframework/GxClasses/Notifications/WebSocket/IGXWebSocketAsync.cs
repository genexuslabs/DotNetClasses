namespace GeneXus.Http.WebSocket
{
    public interface IGXWebSocketAsync
    {        
        bool Start();
        void Stop();
        SendResponseType Send(string clientId, string message);
        void Broadcast(string message);

        event WebSocketEventHandler<string, string> OnNewMessage;
        event WebSocketEventHandler<string> OnSessionConnected;
        event WebSocketEventHandler<string ,string> OnSessionClosed;        
    }

    public delegate void WebSocketEventHandler<WebSocketSessionId, TEventArgs>(string s , TEventArgs e);
    public delegate void WebSocketEventHandler<TEventArgs>(TEventArgs e);

    public enum SendResponseType { SessionNotFound, SessionInvalid, OK, SendFailed}
}
