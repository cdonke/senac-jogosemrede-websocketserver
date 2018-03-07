using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace WebSocketsServer
{
    public class WebSocketConnectionManager
    {
        private readonly ConcurrentDictionary<string, WebSocket> _clients;
        private readonly ILogger<WebSocketConnectionManager> _logger;

        public WebSocketConnectionManager(ILogger<WebSocketConnectionManager> logger)
        {
            _clients = new ConcurrentDictionary<string, WebSocket>();
            _logger = logger;
        }

        public WebSocket this[string id]
        {
            get
            {
                return _clients[id];
            }
        }
        public string this[WebSocket socket]
        {
            get
            {
                return _clients.FirstOrDefault(q => q.Value == socket).Key;
            }
        }


        public string AddSocket(WebSocket socket)
        {
            var id = CreateConnectionId();


            if (_clients.TryAdd(id, socket))
                return id;

            _logger.LogInformation($"Connected: {id}");

            return string.Empty;
        }
        public async Task RemoveSocket(string sender)
        {
            WebSocket client;
            if (_clients.TryRemove(sender, out client))
                await client.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed by WebSocketManager", CancellationToken.None);
        }


        public IEnumerable<WebSocket> All()
        {
            return _clients.Select(q => q.Value);
        }
        public IEnumerable<WebSocket> Others(string sender)
        {
            return _clients
                .Where(q => !q.Key.Equals(sender))
                .Select(q => q.Value);
        }

        
        private string CreateConnectionId()
        {
            return Guid.NewGuid().ToString();
        }
    }
}
