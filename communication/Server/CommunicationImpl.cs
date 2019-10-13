using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using Communication.Proto;
using HPSocketCS;
using System.Collections.Generic;
using System.Text;


namespace Communication.Server
{
    public class CommunicationImpl : ICommunication
    {
        private TcpServer server;
        private List<IntPtr> agentList = new List<IntPtr>();
        private int nAgent =0;
        private int nAgentReady = 0;
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
                Console.WriteLine($"Accept {connId}:{pAgent}");
                agentList.Add(connId);
                nAgent++;
                return HandleResult.Ok;
            };
            server.OnReceive += delegate (IServer sender, IntPtr connId, byte[] bytes)
            {
                PacketType type = (PacketType)BitConverter.ToInt32(bytes, 0);
                Console.WriteLine("Packet received : " + type);
                MemoryStream ms = new MemoryStream();
                switch (type)
                {
                    case PacketType.PlayerReady:
                        if(++nAgentReady==Constants.AgentCount)
                        {
                            lock (locker) Full = true;
                        }
                        break;
                    case PacketType.ProtoPacket:
                        ms.Write(BitConverter.GetBytes(agentList.IndexOf(connId)), 0, 4);
                        ms.Write(bytes, 4, bytes.Length - 4);
                        Console.Write(ms.Length);
                        Message message = new Message();
                        ms.Position = 0;
                        message.MergeFrom(ms);
                        MessageQueue.Add(message);
                        break;
                    case PacketType.IdRequest:
                        byte[] clientID = BitConverter.GetBytes(agentList.IndexOf(connId));
                        ms = new MemoryStream();
                        ms.Write(BitConverter.GetBytes((int)PacketType.IdAllocate), 0, 4);
                        ms.Write(clientID, 0, 4);
                        byte[] mes = ms.ToArray();
                        server.Send(connId, mes, mes.Length);
                        Console.WriteLine($"Allocate ID {agentList.IndexOf(connId)} to {connId}");
                        break;
                    case PacketType.IdAllocate:
                        nAgent--;
                        int index = BitConverter.ToInt32(bytes, 4);
                        agentList.Remove(connId);
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
            MessageQueue = new BlockingCollection<Message>();
            server = new TcpPackServer();
        }

        public void SendMessage(Message message)
        {
            MemoryStream ms = new MemoryStream();
            ms.Write(BitConverter.GetBytes((int)PacketType.ProtoPacket),0,4);
            message.WriteTo(ms);
            byte[] raw = ms.ToArray();
            int agentId = message.Agent;
            if (agentId == -1)
            {
                foreach (IntPtr target in agentList)
                {
                    server.Send(target, raw, raw.Length);
                }
            }
            else server.Send(agentList[agentId], raw, raw.Length);
            ms.Dispose();
        }
    }
}
