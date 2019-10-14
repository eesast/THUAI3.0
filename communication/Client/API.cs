using System;
using System.Net;
using Communication.Proto;

namespace Communication.CAPI
{
    public class API : ICAPI
    {
        private IDClient client;
        private string buffer;
        public bool Connected => !client.Closed;

        public int PlayerId => client.Address;
        public int AgentId { get; private set; }
        public int PlayerCount => Constants.PlayerCount;
        public int AgentCount => Constants.AgentCount;

        public void Initialize()
        {
            client = new IDClient();
            client.OnReceive += delegate (Message message)
            {
                switch (message.Content.GetType().FullName)
                {
                    case "Communication.Proto.ChatMessage":
                        buffer += (message.Content as ChatMessage).Message;
                        break;
                    case "Communication.Proto.AgentId":
                        AgentId = (message.Content as AgentId).Agent;
                        break;
                    default:
                        throw new Exception($"unknown protobuf packet type {message.Content.GetType().FullName}");
                }
            };
            buffer = "";
        }

        public void ConnectServer(IPEndPoint endPoint)
        {
            client.Connect(endPoint);
        }

        public void SendChatMessage(string Message)
        {
            client.Send(new Message()
            {
                Address = -1,
                Content = new ChatMessage()
                {
                    Message = Message
                }
            });
        }

        public string BufferedMessage()
        {
            string t;
            lock (buffer)
            {
                t = buffer;
                buffer = "";
            }
            return t;
        }
    }
}