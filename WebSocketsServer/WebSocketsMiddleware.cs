using System;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace WebSocketsServer
{
    public class WebSocketsMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<WebSocketsMiddleware> _logger;
        private readonly WebSocketHandler _webSocketHandler;

        public WebSocketsMiddleware(RequestDelegate next, ILogger<WebSocketsMiddleware> logger, WebSocketHandler webSocketHandler)
        {
            _webSocketHandler = webSocketHandler;
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            if (!httpContext.WebSockets.IsWebSocketRequest)
                return;

            var webSocket = await httpContext.WebSockets.AcceptWebSocketAsync();
            await _webSocketHandler.OnConnected(webSocket);

            await OnReceive(webSocket, async (result, buffer) =>
            {
                switch (result.MessageType)
                {
                    case WebSocketMessageType.Close:
                        await _webSocketHandler.OnDisconnect(webSocket);
                        break;

                    case WebSocketMessageType.Text:
                        await _webSocketHandler.ReceiveAsync(webSocket, result, buffer);
                        break;
                }
            });
        }

        public async Task OnReceive(WebSocket socket, Action<WebSocketReceiveResult, byte[]> handleMessage)
        {
            var buffer = new byte[4096];
            var token = CancellationToken.None;

            while (socket.State == WebSocketState.Open)
            {
                var result = await socket.ReceiveAsync(buffer, token);
                handleMessage(result, buffer);
            }
        }
    }
}
