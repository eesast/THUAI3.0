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
        public int PlayerCount => Constants.PlayerCount; //采用共享static库，可以省略玩家数量的通知（因为一般不会改动）
        public int AgentCount => Constants.AgentCount;

        public void Initialize()
        {
            client = new IDClient();
            client.OnReceive += delegate (Message message)
            {
                switch (message.Content.GetType().FullName)
                {
                    case "Communication.Proto.ChatMessage":
                        buffer += (message.Content as ChatMessage).Message; //ChatMessage收到后加入buffer
                        break;
                    case "Communication.Proto.AgentId":
                        AgentId = (message.Content as AgentId).Agent; //AgentId包通知Player对应的Agent
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
                Address = -1, //发送给Server端（Agent而非其他Player)
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