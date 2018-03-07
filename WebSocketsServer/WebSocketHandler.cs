using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WebSocketsServer
{
    public abstract class WebSocketHandler
    {
        protected readonly ILogger<WebSocketHandler> _logger;
        protected WebSocketConnectionManager WebSocketConnectionManager { get; set; }

        public WebSocketHandler(WebSocketConnectionManager connectionManager, ILogger<WebSocketHandler> logger)
        {
            _logger = logger;
            WebSocketConnectionManager = connectionManager;
        }

        public virtual async Task OnConnected(WebSocket socket)
        {
            var id = WebSocketConnectionManager.AddSocket(socket);
            _logger.LogInformation($"Connected: {id}");
        }
        public virtual async Task<string> OnDisconnect(WebSocket socket)
        {
            var playerId = WebSocketConnectionManager[socket];
            await WebSocketConnectionManager.RemoveSocket(playerId);

            return playerId;
        }


        public async Task SendToAll(Message message)
        {
            await SendMessageAsync(message, WebSocketConnectionManager.All().ToArray());
        }
        public async Task SendToOthers(Message message)
        {
            var others = WebSocketConnectionManager.Others(message.sender).ToArray();
            await SendMessageAsync(message, others);
        }
        public async Task SendToCaller(Message message)
        {
            var caller = WebSocketConnectionManager[message.sender];
            await SendMessageAsync(message, caller);
        }


        private async Task SendMessageAsync(Message message, params WebSocket[] sockets)
        {
            var jsonMessage = JsonConvert.SerializeObject(message);
            var buffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(jsonMessage));

            foreach (var socket in sockets)
            {
                if (socket.State != WebSocketState.Open)
                    continue;

                await socket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }
        public abstract Task ReceiveAsync(WebSocket socket, WebSocketReceiveResult result, byte[] buffer);
    }
}
