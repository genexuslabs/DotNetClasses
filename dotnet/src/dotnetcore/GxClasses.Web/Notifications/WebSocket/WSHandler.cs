using GeneXus.Configuration;
using GeneXus.Metadata;
using GeneXus.Notifications.WebSocket;
using GeneXus.Procedure;
using GeneXus.Services;
using GeneXus.Utils;
using Jayrock.Json;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GeneXus.Http.WebSocket
{
	public class WSHandler: WebSocketHandler, IGXWebSocketAsync
	{
		private static readonly ILog log = log4net.LogManager.GetLogger(typeof(WSHandler));
		private const string GX_NOTIFICATIONINFO_NAME = "GeneXus.Core.genexus.server.SdtNotificationInfo";
		protected static WebSocketConnectionManager WebSocketConnectionManager = new WebSocketConnectionManager();

		public event WebSocketEventHandler<string, string> OnNewMessage;
		public event WebSocketEventHandler<string> OnSessionConnected;
		public event WebSocketEventHandler<string, string> OnSessionClosed;

		public WSHandler()
		{
		}

		public void SendMessage(System.Net.WebSockets.WebSocket socket, string message)
		{
			if (socket.State != WebSocketState.Open)
				return;
			byte[] sendBuffer = Encoding.UTF8.GetBytes(message);
			socket.SendAsync(new ArraySegment<byte>(sendBuffer), WebSocketMessageType.Text, endOfMessage: true, cancellationToken: CancellationToken.None);
		}

		public void Broadcast(string message)
		{
			foreach (var pair in WebSocketConnectionManager.GetAllList())
			{
				foreach (var socket in pair.Value)
				{
					if (socket.State == WebSocketState.Open)
						SendMessage(socket, message);
				}
			}
		}

		public bool Start()
		{
			return true;
		}

		public void Stop()
		{
			WebSocketConnectionManager.Clear();
		}

		public SendResponseType Send(string connectionGUID, string message)
		{
			var Sockets = WebSocketConnectionManager.GetSocketsById(connectionGUID);
			if (Sockets != null)
				foreach (var socket in Sockets)
				{
					if (socket.State == WebSocketState.Open)
						SendMessage(socket, message);
				}
			return SendResponseType.OK;
		}
		public override void OnOpen(string connectionGUID, System.Net.WebSockets.WebSocket socket)
		{
			WebSocketConnectionManager.AddSocket(connectionGUID, socket);
			ExecuteHandler(HandlerType.OnOpen, new Object[] { connectionGUID });
			OnSessionConnected?.Invoke(connectionGUID);
		}
		public override Task OnMessage(string connectionGUID, System.Net.WebSockets.WebSocket socket, WebSocketReceiveResult result, byte[] buffer)
		{
			string key = connectionGUID;
			string message = Encoding.UTF8.GetString(buffer);
			Type objType = GeneXus.Metadata.ClassLoader.FindType("GeneXus", GX_NOTIFICATIONINFO_NAME, null);
			GxUserType nInfo = (GxUserType)Activator.CreateInstance(objType);
			JObject jObj = new JObject();
			jObj.Put("Message", message);
			nInfo.FromJSONObject(jObj);
			ExecuteHandler(HandlerType.ReceivedMessage, new Object[] { key, nInfo });
			OnNewMessage?.Invoke(key, message);
			return Task.CompletedTask;
		}

		public override void OnClose(string connectionGUID, System.Net.WebSockets.WebSocket socket)
		{
			WebSocketConnectionManager.RemoveSocket(connectionGUID, socket);
			ExecuteHandler(HandlerType.OnClose, new Object[] { connectionGUID });
			OnSessionClosed?.Invoke(connectionGUID, string.Empty);
		}

		private static void LogDebug(string msg)
		{
			GXLogging.Debug(log, msg);
		}

		private static void LogError(string msg, Exception e)
		{
			GXLogging.Error(log, e, msg);
		}
		public enum HandlerType
		{
			ReceivedMessage, OnOpen, OnClose, OnError
		}
		private static Dictionary<HandlerType, String> handlerCache = new Dictionary<HandlerType, String>(4);

		private void ExecuteHandler(HandlerType type, Object[] parameters)
		{
			String handler = GetHandlerClassName(type);
			if (!string.IsNullOrWhiteSpace(handler))
			{
				try
				{
					string nSpace = string.Empty;
					Config.GetValueOf("AppMainNamespace", out nSpace);
					GXProcedure obj = null;
					try
					{
						obj = (GXProcedure)ClassLoader.FindInstance(Config.CommonAssemblyName, nSpace, handler, null, null);
					}
					catch (Exception e)
					{
						LogError("GXWebSocket - Could not create Procedure Instance: " + handler, e);
						return;
					}
					ClassLoader.Execute(obj, "execute", parameters);
				}
				catch (Exception e)
				{
					LogError("GXWebSocket - Handler Found, but failed executing action: " + handler, e);
				}
			}
		}

		private String GetHandlerClassName(HandlerType hType)
		{
			string handlerClassName = string.Empty;
			if (!handlerCache.TryGetValue(hType, out handlerClassName))
			{
				String type = GetPtyTypeName(hType);
				GXService service = GXServices.Instance.Get(GXServices.WEBNOTIFICATIONS_SERVICE);
				if (service != null && service.Properties != null)
				{
					String className = service.Properties.Get(type);
					if (!string.IsNullOrWhiteSpace(className))
					{
						handlerCache[hType] = className.ToLower();
						handlerClassName = handlerCache[hType];
					}
				}
			}
			return handlerClassName;
		}

		private String GetPtyTypeName(HandlerType type)
		{
			String typeName = string.Empty;
			switch (type)
			{
				case HandlerType.ReceivedMessage:
					typeName = "WEBNOTIFICATIONS_RECEIVED_HANDLER";
					break;
				case HandlerType.OnClose:
					typeName = "WEBNOTIFICATIONS_ONCLOSE_HANDLER";
					break;
				case HandlerType.OnError:
					typeName = "WEBNOTIFICATIONS_ONERROR_HANDLER";
					break;
				case HandlerType.OnOpen:
					typeName = "WEBNOTIFICATIONS_ONOPEN_HANDLER";
					break;
			}
			return typeName;
		}
	}

	public class WebSocketConnectionManager
	{

		private Dictionary<string, HashSet<System.Net.WebSockets.WebSocket>> _socketsList = new Dictionary<string, HashSet<System.Net.WebSockets.WebSocket>>();

		public HashSet<System.Net.WebSockets.WebSocket> GetSocketsById(string id)
		{
			return _socketsList.FirstOrDefault(p => p.Key == id).Value;
		}

		public Dictionary<string, HashSet<System.Net.WebSockets.WebSocket>> GetAllList()
		{
			return _socketsList;
		}

		public void AddSocket(string connectionGUID, System.Net.WebSockets.WebSocket socket)
		{
			lock (_socketsList)
			{
				HashSet<System.Net.WebSockets.WebSocket> sockets;
				if (!_socketsList.TryGetValue(connectionGUID, out sockets))
				{
					sockets = new HashSet<System.Net.WebSockets.WebSocket>();
					_socketsList.Add(connectionGUID, sockets);
				}

				lock (sockets)
				{
					sockets.Add(socket);
				}
			}
		}

		public void RemoveSocket(string connectionGUID, System.Net.WebSockets.WebSocket socket)
		{
			lock (_socketsList)
			{
				HashSet<System.Net.WebSockets.WebSocket> sockets;
				if (!_socketsList.TryGetValue(connectionGUID, out sockets))
				{
					return;
				}

				lock (sockets)
				{
					sockets.Remove(socket);

					if (sockets.Count == 0)
					{
						_socketsList.Remove(connectionGUID);
					}
				}
			}
			socket.CloseAsync(closeStatus: WebSocketCloseStatus.NormalClosure,
									statusDescription: "Closed by the WebSocketManager",
									cancellationToken: CancellationToken.None);
		}

		internal void Clear()
		{
			foreach (HashSet<System.Net.WebSockets.WebSocket> socketHash in _socketsList.Values)
			{
				foreach (System.Net.WebSockets.WebSocket socket in socketHash)
				{
					socket.CloseAsync(WebSocketCloseStatus.NormalClosure, statusDescription: "Closed by Clear", cancellationToken: CancellationToken.None);
				}
			}
		}
	}
}
