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
    public class Clients
    {
        private readonly ConcurrentDictionary<string, WebSocket> _clients;
        private readonly ILogger<Clients> _logger;

        public Clients(ILogger<Clients> logger)
        {
            _clients = new ConcurrentDictionary<string, WebSocket>();
            _logger = logger;
        }

        public WebSocket this[string id]{
            get {
                return _clients[id];
            }
        }
        public string Add(WebSocket socket){
            var id = CreateConnectionId();
            _clients.TryAdd(id, socket);
            return id;
        }

        
        public async Task All(Message message, string sender){
            var all = _clients.Values.ToArray();
            await SendAsync(message, all);
        }
        public async Task Others(Message message, string sender) {
            var others = _clients
                .Where(q => !q.Key.Equals(sender))
                .Select(q => q.Value)
                .ToArray();
            await SendAsync(message, others);
        }
        public async Task Caller(Message message, string sender) {
            var caller = _clients[sender];
            await SendAsync(message, caller);
        }

        private async Task SendAsync(Message msg, params WebSocket[] targets){
            var jsonMessage = JsonConvert.SerializeObject(msg);
            var buffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(jsonMessage));

            foreach (var target in targets)
                await SendAsync(target,  buffer, WebSocketMessageType.Text);
        }
        private async Task SendAsync(WebSocket target, ArraySegment<byte> buffer, WebSocketMessageType type){
            await target.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);    
        }

        private string CreateConnectionId()
        {
            return Guid.NewGuid().ToString();
        }

        public async Task Disconnect(string sender)
        {
            WebSocket client;
            if (_clients.TryRemove(sender, out client))
                await SendAsync(client, null, WebSocketMessageType.Close);
        }
    }
}
