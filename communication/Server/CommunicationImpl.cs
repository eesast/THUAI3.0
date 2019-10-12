using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using Communication.Proto;
using HPSocketCS;

namespace Communication.Server
{
    public class CommunicationImpl : ICommunication
    {
        private TcpServer server;
        private IntPtr client;
        private bool Full;
        private object locker = new object();
        public ushort Port
        {
            get => server.Port;
            set => server.Port = value;
        }
        public BlockingCollection<Message> MessageQueue { get; private set; }
        public void GameOver()
        {
            MemoryStream ms = new MemoryStream();
            ms.Write(BitConverter.GetBytes((int)PacketType.GameOver), 0, 4);
            ms.Write(BitConverter.GetBytes(-1), 0, 4);
            byte[] raw = ms.ToArray();
            server.Send(client, raw, raw.Length);
            server.Disconnect(client);
            ms.Dispose();
        }

        public void GameStart()
        {
            server.OnAccept += delegate (IServer sender, IntPtr connId, IntPtr pClient)
            {
                if (client.ToInt32() != 0)
                {
                    return HandleResult.Ignore;
                }
                else
                {
                    client = connId;
                    Console.WriteLine($"Allocated agent handle {client}");
                    return HandleResult.Ok;
                }
            };
            server.OnReceive += delegate (IServer sender, IntPtr connId, byte[] bytes)
            {
                MemoryStream ms = new MemoryStream(bytes);
                byte[] buffer = new byte[4];
                ms.Read(buffer, 0, 4);
                PacketType type = (PacketType)BitConverter.ToInt32(buffer);
                Console.WriteLine("Packet received : " + type);
                switch (type)
                {
                    case PacketType.PlayerReady:
                        lock (locker) Full = true;
                        break;
                    case PacketType.ProtoPacket:
                        Message message = new Message();
                        message.MergeFrom(ms);
                        MessageQueue.Add(message);
                        break;
                    default:
                        throw new Exception("unknown packet type received.");

                }
                return HandleResult.Ok;
            };
            Full = false;
            server.Start();
            Console.WriteLine("server started");
            while (true)
            {
                lock (locker)
                    if (Full) break;
            }
        }

        public void Initialize()
        {
            MessageQueue = new BlockingCollection<Message>();
            server = new TcpPackServer();
            client = new IntPtr(0);
        }

        public void SendMessage(Message message)
        {
            MemoryStream ms = new MemoryStream();
            ms.Write(BitConverter.GetBytes((int)PacketType.ProtoPacket));
            message.WriteTo(ms);
            byte[] raw = ms.ToArray();
            server.Send(client, raw, raw.Length);
            ms.Dispose();
        }
    }
}
