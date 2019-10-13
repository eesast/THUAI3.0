using System;
using System.Collections.Concurrent;
using System.IO;
using Communication.Proto;
using HPSocketCS;
using System.Collections.Generic;

namespace Communication.Server
{
    public class CommunicationImpl : ICommunication
    {
        private TcpServer server;
        private List<IntPtr> agentList = new List<IntPtr>();
        private int nAgentReady = 0;
        private bool Full;
        private readonly object locker = new object();
        public ushort Port
        {
            get => server.Port;
            set => server.Port = value;
        }
        public BlockingCollection<ServerMessage> MessageQueue { get; private set; }
        public void GameOver()
        {
            MemoryStream ms = new MemoryStream();
            ms.Write(BitConverter.GetBytes((int)PacketType.GameOver), 0, 4);
            ms.Write(BitConverter.GetBytes(-1), 0, 4);
            byte[] raw = ms.ToArray();
            foreach(IntPtr agent in agentList)
            {
                server.Send(agent, raw, raw.Length);
                server.Disconnect(agent);
            }
            ms.Dispose();
        }

        public void GameStart()
        {
            server.OnAccept += delegate (IServer sender, IntPtr connId, IntPtr pAgent)
            {
                if (agentList.Count == Constants.AgentCount) return HandleResult.Ignore;

                Console.WriteLine($"Accept {connId}:{pAgent}");
                return HandleResult.Ok;
            };
            server.OnReceive += delegate (IServer sender, IntPtr connId, byte[] bytes)
            {
                PacketType type = (PacketType)BitConverter.ToInt32(bytes, 0);
                Console.WriteLine("Packet received : " + type);
                MemoryStream ms = new MemoryStream(bytes, 4, bytes.Length - 4);
                BinaryReader br = new BinaryReader(ms);

                switch (type)
                {
                    case PacketType.PlayerReady:
                        if(++nAgentReady==Constants.AgentCount)
                        {
                            lock (locker) Full = true;
                        }
                        break;
                    case PacketType.ProtoPacket:
                        ServerMessage message = new ServerMessage();
                        message.Agent = agentList.IndexOf(connId);
                        message.Client = br.ReadInt32();
                        message.Message.MergeFrom(ms);
                        MessageQueue.Add(message);
                        break;
                    case PacketType.IdRequest:
                        agentList.Add(connId);
                        byte[] clientID = BitConverter.GetBytes(agentList.Count - 1);
                        ms = new MemoryStream();
                        ms.Write(BitConverter.GetBytes((int)PacketType.IdAllocate), 0, 4);
                        ms.Write(clientID, 0, 4);
                        byte[] mes = ms.ToArray();
                        server.Send(connId, mes, mes.Length);
                        Console.WriteLine($"Allocate ID {agentList.IndexOf(connId)} to {connId}");
                        break;
                    case PacketType.IdAllocate:
                        int index = BitConverter.ToInt32(bytes, 4);
                        agentList[index] = connId;
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
            MessageQueue = new BlockingCollection<ServerMessage>();
            server = new TcpPackServer();
        }

        public void SendMessage(ServerMessage message)
        {
            MemoryStream ms = new MemoryStream();
            ms.Write(BitConverter.GetBytes((int)PacketType.ProtoPacket),0,4);
            ms.Write(BitConverter.GetBytes(message.Client));
            message.Message.WriteTo(ms);
            byte[] raw = ms.ToArray();

            if (message.Agent == -1)
                foreach (IntPtr target in agentList)
                    server.Send(target, raw, raw.Length);
            else server.Send(agentList[message.Agent], raw, raw.Length);

            ms.Dispose();
        }
    }
}
