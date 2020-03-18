using System;
using System.Net;
using Communication.Proto;
using Google.Protobuf;
using System.IO;

namespace Communication.CAPI
{
    public sealed class API : ICAPI, IDisposable
    {
        private IDClient client;
        private string buffer;

        public event ReceiveMessageCallback ReceiveMessage;

        public bool Connected => !client.Closed;

        public int PlayerId => client.Address;
        public int AgentId { get; private set; }
        public int PlayerCount => Constants.PlayerCount; //采用共享static库，可以省略玩家数量的通知（因为一般不会改动）
        public int AgentCount => Constants.AgentCount;
        public float Ping { get; internal set; }

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
                    case "Communication.Proto.PingPacket":
                        Ping = (DateTime.Now.Ticks - (message.Content as PingPacket).Ticks) * 0.0001f; //PingPacket计算Ping
                        break;
                    default:
                        ReceiveMessage(message.Content);
                        break;
                        //throw new Exception($"unknown protobuf packet type {message.Content.GetType().FullName}");
                }
            };
            buffer = "";
            Ping = -1;
        }

        public void ConnectServer(IPEndPoint endPoint)
        {
            client.Connect(endPoint);
            while (AgentId == -1 || PlayerId == -1) ;
        }

        public void Refresh()
        {
            if (AgentId == -1 || PlayerId == -1) throw new InvalidOperationException("Can not refresh when not ready.");
            client.Send(new Message()
            {
                Address = -1,
                Content = new Message()
                {
                    Address = AgentId,
                    Content = new Message()
                    {
                        Address = PlayerId,
                        Content = new PingPacket()
                        {
                            Ticks = DateTime.Now.Ticks
                        }
                    }
                }
            });
        }
        public void SendChatMessage(string Message)
        {
            client.Send(new Message()
            {
                Address = -1, //发送给Server端（Agent而非其他Player)
                Content = new Message()
                {
                    Address = -1, //GameServer而非其他Agent
                    Content = new Message()
                    {
                        Address = PlayerId,
                        Content = new ChatMessage()
                        {
                            Message = Message
                        }
                    }
                }
            });
        }
        public void SendQuitMessage()
        {
            client.Quit();
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

        public void SendMessage(IMessage message)
        {
            client.Send(new Message()
            {
                Address = -1, //发送给Server端（Agent而非其他Player)
                Content = new Message()
                {
                    Address = -1, //GameServer而非其他Agent
                    Content = new Message()
                    {
                        Address = PlayerId,
                        Content = message
                    }
                }
            });
        }

        public void Dispose()
        {
            client.Quit();
            client.Dispose();
        }
    }
}