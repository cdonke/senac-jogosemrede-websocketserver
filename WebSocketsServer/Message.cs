using System;
namespace WebSocketsServer
{
    public class Message
    {
        public string sender { get; set; }
        public string operation { get; set; }
        public object message { get; set; }
    }
}
