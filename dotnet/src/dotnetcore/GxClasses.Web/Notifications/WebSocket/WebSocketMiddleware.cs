using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace GeneXus.Notifications.WebSocket
{
	public abstract class WebSocketHandler
	{
		public virtual void OnOpen(string connectionGuid, System.Net.WebSockets.WebSocket socket) { }
		public virtual void OnClose(string connectionGuid, System.Net.WebSockets.WebSocket socket) { }

		public abstract Task OnMessage(string connectionGuid, System.Net.WebSockets.WebSocket socket, WebSocketReceiveResult result, byte[] buffer);
		
	}

	public class WebSocketManagerMiddleware
	{
		private readonly RequestDelegate _next;
		private WebSocketHandler _webSocketHandler { get; set; }

		public WebSocketManagerMiddleware(RequestDelegate next)
		{
			_next = next;
			_webSocketHandler = GXWebSocketFactory.GetWebSocketProvider() as WebSocketHandler;
		}

		public async Task Invoke(HttpContext context)
		{
			if (!context.WebSockets.IsWebSocketRequest)
			{
				context.Response.StatusCode = StatusCodes.Status400BadRequest;
				return;
			}

			System.Net.WebSockets.WebSocket socket = await context.WebSockets.AcceptWebSocketAsync();
			string connectionId = context.Request.Query.Keys.FirstOrDefault();

			_webSocketHandler.OnOpen(connectionId, socket);

			await Receive(socket, async (result, buffer) =>
			{
				if (result.MessageType == WebSocketMessageType.Text)
				{
					await _webSocketHandler.OnMessage(connectionId, socket, result, buffer);
					return;
				}

				else if (result.MessageType == WebSocketMessageType.Close)
				{
					_webSocketHandler.OnClose(connectionId, socket);
					return;
				}

			});
		}

		private async Task Receive(System.Net.WebSockets.WebSocket socket, Action<WebSocketReceiveResult, byte[]> handleMessage)
		{
			byte[] chunkBuffer = new byte[1024];
			try
			{
				while (socket.State == WebSocketState.Open)
				{
					using (MemoryStream ms = new())
					{
						var result = await ReadDataFromSocket(socket, chunkBuffer, ms);
						while (!result.EndOfMessage)
						{
							result = await ReadDataFromSocket(socket, chunkBuffer, ms);
						}
						handleMessage(result, ms.ToArray());
					}
				}
			}
			catch (WebSocketException wsex) when (wsex.WebSocketErrorCode == WebSocketError.ConnectionClosedPrematurely)
			{
				Console.WriteLine("ConnectionClosedPrematurely");
			}
		}

		private static async Task<WebSocketReceiveResult> ReadDataFromSocket(System.Net.WebSockets.WebSocket socket, byte[] chunkBuffer, MemoryStream ms)
		{
			var result = await socket.ReceiveAsync(new ArraySegment<byte>(chunkBuffer), CancellationToken.None);
			ms.Write(chunkBuffer, 0, result.Count);
			return result;
		}
	}
}
