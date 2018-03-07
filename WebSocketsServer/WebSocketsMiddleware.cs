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
        private readonly Clients _clients;                                                 


        public WebSocketsMiddleware(RequestDelegate next, ILogger<WebSocketsMiddleware> logger, Clients clients)
        {
            _clients = clients;
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext httpContext){
            if (!httpContext.WebSockets.IsWebSocketRequest)
                return;

            var webSocket = await httpContext.WebSockets.AcceptWebSocketAsync();
            await OnConnected(webSocket);


            await OnReceive(webSocket);
        }

        public async Task OnReceive(WebSocket socket){
            while(socket.State == WebSocketState.Open){
                var token = CancellationToken.None;
                var buffer = new ArraySegment<byte>(new byte[4096]);

                var received = await socket.ReceiveAsync(buffer, token);
                var message = Encoding.UTF8.GetString(buffer.Array, 0, received.Count);

                await MessageHandler(received, message);
            }
        }

        public async Task MessageHandler(WebSocketReceiveResult received, string message){
            var obj = JsonConvert.DeserializeObject<Message>(message);
            var session = _clients[obj.sender];

            switch (received.MessageType)
            {
                case WebSocketMessageType.Close:
                    try
                    {
                        await _clients.Disconnect(obj.sender);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex.ToString());
                    }
                    break;

                case WebSocketMessageType.Text:
                    try
                    {
                        //TODO: Implementar acoes
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex.ToString());
                    }
                    break;

                default:
                    _logger.LogError("Unknown message");
                    break;
            }
        }

        public async Task OnConnected(WebSocket socket){
            // Manter lista com todos os jogadores conectados
            var playerId = _clients.Add(socket);


            //TODO: Implementar OnConnected




            await Task.FromResult<object>(null);
        }

    }
}
