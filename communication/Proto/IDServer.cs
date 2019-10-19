using HPSocketCS;
using System;
using System.Collections.Generic;
using System.IO;

namespace Communication.Proto
{
    internal delegate void OnReceiveCallback(Message message);
    internal delegate void OnAcceptCallback();
    internal class IDServer
    {
        private List<IntPtr> clientList;
        private TcpServer server;

        public ushort Port
        {
            get => server.Port;
            set => server.Port = value;
        }

        public int Count => clientList.Count;
        public IDServer()
        {
            clientList = new List<IntPtr>();
            server = new TcpPackServer();
            server.OnReceive += delegate (IServer sender, IntPtr connId, byte[] bytes)
            {
                MemoryStream istream = new MemoryStream(bytes);
                BinaryReader br = new BinaryReader(istream);
                PacketType type = (PacketType) br.ReadInt32();

                switch (type)
                {
                    case PacketType.IdAllocate: //客户端请求分配到，此时slot应已有，不需要扩容clientList
                        clientList[br.ReadInt32()] = connId;
                        OnAccept?.Invoke();
                        Constants.Debug($"ServerSide: Using Pre-Allocated ID #{clientList.IndexOf(connId)}");
                        break;
                    case PacketType.IdRequest: //客户端请求ID
                        clientList.Add(connId);
                        MemoryStream ostream = new MemoryStream();
                        BinaryWriter bw = new BinaryWriter(ostream);
                        bw.Write((int)PacketType.IdAllocate);
                        bw.Write(clientList.Count - 1);
                        byte[] raw = ostream.ToArray();
                        server.Send(connId, raw, raw.Length);
                        OnAccept?.Invoke();
                        Constants.Debug($"ServerSide: Allocate ID #{clientList.Count - 1}");
                        break;
                    case PacketType.ProtoPacket: //接收到包
                        Message message = new Message();
                        message.MergeFrom(istream);
                        Constants.Debug($"ServerSide: Data received {message.Content.GetType().FullName}");
                        InternalSend(message, clientList.IndexOf(connId));
                        break;
                    default:
                        throw new Exception($"unknown Packet Type : {type}");
                }
                return HandleResult.Ok;
            };
        }
        public void Start()
        {
            server.Start();
        }

        public void Stop()
        {
            Constants.Debug("ServerSide: Stopping");
            MemoryStream ostream = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ostream);
            bw.Write((int)PacketType.Disconnected);
            byte[] raw = ostream.ToArray();
            foreach (IntPtr client in clientList)
                server.Send(client, raw, raw.Length);
            server.Stop();
        }

        private void InternalSend(Message message, int address) //address代表目标为自身时的发包者
        {
            if (message.Address == -1) //发给自己的
            {
                message.Address = address;
                OnReceive?.Invoke(message);
            }
            else
            {
                MemoryStream ostream = new MemoryStream();
                BinaryWriter bw = new BinaryWriter(ostream);
                bw.Write((int)PacketType.ProtoPacket);
                message.WriteTo(ostream);
                byte[] raw = ostream.ToArray();
                if (message.Address == -2) //广播包
                    foreach(IntPtr client in clientList)
                        server.Send(client, raw, raw.Length);
                else
                    server.Send(clientList[message.Address], raw, raw.Length);
            }
        }

        public void Send(Message message) //发包
        {
            InternalSend(message, -1);
            Constants.Debug($"ServerSide: Data sent {message.Content.GetType().FullName}");
        }

        public event OnReceiveCallback OnReceive;
        public event OnAcceptCallback OnAccept; //当客户端被分配到/已请求ID而非TCP的Accept时触发
    }
}
