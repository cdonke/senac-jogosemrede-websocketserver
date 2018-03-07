using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace WebSocketsServer
{
    public class CardGameHandler : WebSocketHandler
    {
        public CardGameHandler(WebSocketConnectionManager connectionManager, ILogger<WebSocketHandler> logger) : base(connectionManager, logger)
        { }

        public override async Task OnConnected(WebSocket socket)
        {
            await base.OnConnected(socket);

            var id = WebSocketConnectionManager[socket];

            var msg = new Message
            {
                sender = id,
                operation = "CONNECTED"
            };
            await SendToCaller(msg);


            msg.operation = "NEWPLAYER";
            await SendToOthers(msg);
        }
        public override async Task<string> OnDisconnect(WebSocket socket)
        {
            var id = await base.OnDisconnect(socket);
            _logger.LogInformation($"Disconnected: {id}");

            var msg = new Message
            {
                sender = id,
                operation = "DISCONNECTED"
            };
            await SendToAll(msg);

            return id;
        }

        public override async Task ReceiveAsync(WebSocket socket, WebSocketReceiveResult result, byte[] buffer)
        {
            var session = WebSocketConnectionManager[socket];
            var token = CancellationToken.None;

            var message = Encoding.UTF8.GetString(buffer, 0, buffer.Length);
            var obj = JsonConvert.DeserializeObject<Message>(message);

            _logger.LogInformation($"{result.MessageType} message received from {obj.sender}: {obj.message}");

            //TODO: Implementar acoes
            await SendToOthers(obj);
        }
    }
}
