using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;

using GeneXus.Configuration;
using GeneXus.Metadata;
using GeneXus.Procedure;
using GeneXus.Services;
using GeneXus.Utils;
using Jayrock.Json;
using log4net;
using Microsoft.Web.WebSockets;

namespace GeneXus.Http.WebSocket
{
	public class WSHandler : WebSocketHandler, IGXWebSocketAsync
	{
		private static readonly ILog log = log4net.LogManager.GetLogger(typeof(WSHandler));

		private const string GX_NOTIFICATIONINFO_NAME = "GeneXus.Core.genexus.server.SdtNotificationInfo";

		private static WebSocketCollection clientsWS = new WebSocketCollection(); 

		public WSHandler()
		{
			GXLogging.Debug(log, "ASP.NET Websocket connection started ok!");
		}

		private static void AddClient(WSHandler client)
		{
			string key = GetClientKey(client);
			if (!string.IsNullOrEmpty(key))
			{
				LogDebug($"New Websocket connection {key}");
				clientsWS.Remove(client);
				clientsWS.Add(client);
			}
		}

		private static string GetClientKey(WSHandler client)
		{
			return client.WebSocketContext.QueryString.ToString();
		}

		#region IGXWebSocketAsync Members

		public void Broadcast(string message)
		{
			clientsWS.Broadcast(message);
		}

		public event WebSocketEventHandler<string, string> OnNewMessage;

		public event WebSocketEventHandler<string, string> OnSessionClosed;

		public event WebSocketEventHandler<string> OnSessionConnected;

		public SendResponseType Send(string clientId, string message)
		{
			SendResponseType result = SendResponseType.SessionNotFound;
			var Sockets = clientsWS.Where(ws => GetClientKey((WSHandler)ws) == clientId);
			foreach (WSHandler socket in Sockets)
			{
				if (socket.WebSocketContext.IsClientConnected)
				{
					socket.Send(message);
					LogDebug($"Send - Message sent to client '{clientId}'");
					result = SendResponseType.OK;
				}
				else
				{
					LogError($"Send - Web Socket connection has been closed. Could not send message '{clientId}'");
					result = SendResponseType.SessionInvalid;
				}
			}

			if (result == SendResponseType.SessionNotFound)
			{
				LogError(String.Format("Send - WebSocket Session Id: '{0}' was not found", clientId));
			}
			return result;
		}

		public bool Start()
		{
			return true;
		}

		public void Stop()
		{
			clientsWS.Clear();
		}

		#endregion

		public override void OnMessage(byte[] message)
		{
			OnMessage(Encoding.UTF8.GetString(message));
		}

		public override void OnMessage(string message)
		{
			string key = GetClientKey(this);
			Type objType = GeneXus.Metadata.ClassLoader.FindType("GeneXus", GX_NOTIFICATIONINFO_NAME, null);
			GxUserType nInfo = (GxUserType)Activator.CreateInstance(objType);
			JObject jObj = new JObject();
			jObj.Put("Message", message);
			nInfo.FromJSONObject(jObj);
			ExecuteHandler(HandlerType.ReceivedMessage, new Object[] { key, nInfo });
			OnNewMessage?.Invoke(key, message);
		}

		public override void OnClose()
		{
			clientsWS.Remove(this);
			string key = GetClientKey(this);
			LogDebug($"Client websocket disconnected '{key}'");
			ExecuteHandler(HandlerType.OnClose, new Object[] { key });
			OnSessionClosed?.Invoke(key, string.Empty);
			base.OnClose();
		}

		public override void OnError()
		{
			base.OnError();
			string key = GetClientKey(this);
			if (!ExecuteHandler(HandlerType.OnError, new Object[] { key }))
			{
				log.Debug($"An unknown error ocurred on WebSocket client ('{key}')");
			}
			OnSessionClosed?.Invoke(key, "WebSocket Error");
		}
		public override void OnOpen()
		{
			base.OnOpen();
			AddClient(this);
			string key = GetClientKey(this);
			ExecuteHandler(HandlerType.OnOpen, new Object[] { key });
			OnSessionConnected?.Invoke(key);
		}
		private static void LogDebug(string msg)
		{
			GXLogging.Debug(log, msg);
		}

		private static void LogError(string msg, Exception e = null)
		{
			GXLogging.Error(log, e, msg);
		}
		public enum HandlerType
		{
			ReceivedMessage, OnOpen, OnClose, OnError
		}
		private static ConcurrentDictionary<HandlerType, String> handlerCache = new ConcurrentDictionary<HandlerType, String>();

		private bool ExecuteHandler(HandlerType type, Object[] parameters)
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
						LogError("GXWebSocket - Failed to intialize Procedure Handler Class: " + handler, e);
					}
					if (obj == null)
					{
						LogError($"GXWebSocket - Could not create Procedure Handler Class. Class Type '{nSpace}.{handler}' not found in Class Loader.");
					}
					else
					{
						ClassLoader.Execute(obj, "execute", parameters);
						return true;
					}
				}
				catch (Exception e)
				{
					LogError($"GXWebSocket - Procedure Handler Class Found '{handler}', but failed executing action 'execute.", e);
				}
			}
			return false;
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
}
