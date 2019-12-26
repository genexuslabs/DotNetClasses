using System;
using GeneXus.Application;

using GeneXus.Utils;
using GeneXus.Http.WebSocket;
using GeneXus.Notifications.WebSocket;

namespace GeneXus.Notifications
{
	public class GXWebNotification
    {
        private static IGXWebSocketAsync _ws;
        private IGxContext _ctx;
        private short _errCode;
        private string _errDescription;        

        public short ErrCode
        {
            get { return _errCode; }
        }

        public string ErrDescription
        {
            get { return _errDescription; }
        }

        public GXWebNotification(IGxContext gxContext)
        {
            _ctx = gxContext;
            Init();
        }

        private void SetError(short errCode)
        {
            this._errCode = errCode;
            switch (errCode)
            {
                case 0:
                    _errDescription = "OK";
                    break;
                case 1:
                    _errDescription = "Could not start WebSocket Server";
                    break;
                case 2:
                    _errDescription = "WebSocket Session not found";
                    break;
                case 3:
                    _errDescription = "WebSocket Session is closed or invalid";
                    break;
                case 4:
                    _errDescription = "Message could not be delivered to client";
                    break;
                default:
                    break;
            }
        }

        private static Object syncObj = new Object();

        public bool Init()
        {
            bool started = false;
            if (_ws == null)
            {
                lock (syncObj)
                {
                    if (_ws == null)
                    {
                        _ws = GXWebSocketFactory.GetWebSocketProvider();
						if (_ws != null)
						{
							started = _ws.Start();
						}
						else
						{
							started = false;
						}
                    }
                }                
            }
            else
            {
                started = true;
            }
            if (!started)
                SetError(1);
            else
                SetError(0);
            return started;
        }

        public static bool Start()
        {            
            return true;
        }

        public short NotifyClient(string clientId, GxUserType message)
        {
            return NotifyImpl(clientId, message.ToJSonString());
        }

		public short NotifyClient(string clientId, string message)
		{
			return NotifyImpl(clientId, message);
		}

		private short NotifyImpl(string clientId, string msg)
        {
            if (Init())
            {
                SendResponseType result = _ws.Send(clientId.Trim(), msg);
                switch (result)
                {
                    case SendResponseType.OK:
                        SetError(0);
                        break;
                    case SendResponseType.SessionNotFound:
                        SetError(2);
                        break;
                    case SendResponseType.SessionInvalid:
                        SetError(3);
                        break;
                    case SendResponseType.SendFailed:
                        SetError(4);
                        break;
                    default:
                        break;
                }
            }
            return _errCode;
        }

        public short Notify(string message)
        {
            return NotifyImpl(_ctx.ClientID, message);
        }

        public short Notify(GxUserType message)
        {
            return NotifyClient(_ctx.ClientID, message);
        }

        public void Broadcast(GxUserType message)
        {
			if (_ws != null)
			{
				_ws.Broadcast(message.ToJSonString());
			}
        }

        public String ClientId
        {
            get
            {
                return _ctx.ClientID;
            }
        }
    }

    public class GXWebNotificationInfo : GxUserType
    {
        public String Id { get; set; }
        public String GroupName { get; set; }
        public String Object { get; set; }
        public GxUserType Message { get; set; }

        public override void ToJSON()
        {
            AddObjectProperty("Id", Id);
            AddObjectProperty("GroupName", GroupName);
            AddObjectProperty("Object", Object);
            AddObjectProperty("Message", Message.ToJSonString());
        }
    }
}
