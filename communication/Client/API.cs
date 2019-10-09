using System;
using HPSocketCS;
using System.Net;
using System.Threading;
using System.IO;
using Communication.Proto;

namespace Communication.CAPI
{
    public class API : ICAPI
    {
        private TcpClient client;
        private string buffer;
        private bool closed;
        public bool Connected => client.IsStarted;

        public int MyPlayer { get; private set; }
        public int PlayerCount { get; private set; }

        public void Initialize()
        {
            client = new TcpPackClient();
            client.OnReceive += OnReceive;
            client.OnClose += TryReconnect;
            closed = false;
            buffer = "";
        }

        private HandleResult TryReconnect(IClient sender, SocketOperation enOperation, int errcode)
        {
            if (!closed)
            {
                while (true)
                {
                    client.Connect("127.0.0.1", 9999, false);
                    if (Connected) break;
                    Console.WriteLine("Connection failed.");
                    Thread.Sleep(500);
                }
                Console.WriteLine("Reconnected to server");
            }
            return HandleResult.Ok;
        }
        private  HandleResult OnReceive(IClient sender, byte[] bytes)
        {
            PacketType type = (PacketType) BitConverter.ToInt32(bytes, 0);
            Console.WriteLine("Packet received : " + type);
            switch (type)
            {
                case PacketType.GameOver:
                    client.Stop();
                    closed = true;
                    break;
                case PacketType.ProtoPacket:
                    MemoryStream ms = new MemoryStream(bytes, 4, bytes.Length - 4);
                    Message message = new Message();
                    message.MergeFrom(ms);
                    switch (message.Content.GetType().FullName)
                    {
                        case "Communication.Proto.ChatMessage":
                            lock (buffer)
                            {
                                buffer += (message.Content as ChatMessage).Message;
                            }
                            break;
                        default:
                            throw new Exception("unknown message type received");
                    }
                    break;
                case PacketType.RequestInfo:
                    break;
            }
            return HandleResult.Ok;
        }
        public void ConnectServer(IPEndPoint endPoint)
        {
            buffer = "";
            client.Connect(endPoint.Address.ToString(), (ushort)endPoint.Port);
            if (!Connected)
                TryReconnect(client, SocketOperation.Close, 0);
            else
                Console.WriteLine("Connected to server.");
        }

        public void SendChatMessage(string Message)
        {
            ChatMessage chat = new ChatMessage();
            Message message = new Message();
            chat.Message = Message;
            message.Content = chat;
            MemoryStream ms = new MemoryStream();
            ms.Write(BitConverter.GetBytes((int)PacketType.ProtoPacket), 0, 4);
            message.WriteTo(ms);
            byte[] raw = ms.ToArray();
            client.Send(raw, raw.Length);
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