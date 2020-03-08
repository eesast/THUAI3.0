using HPSocketCS;
using System;
using System.IO;
using System.Net;
using System.Threading;

namespace Communication.Proto
{
    internal delegate void OnDisconnectCallback();
    internal sealed class IDClient : IDisposable
    {
        private IPEndPoint endPoint;
        private readonly TcpClient client;
        
        public int Address { get; private set; }
        public bool Closed { get; private set; }
        public IDClient()
        {
            client = new TcpPackClient();
            Address = -1;
            Closed = false;

            client.OnReceive += delegate (IClient sender, byte[] bytes)
            {
                MemoryStream istream = new MemoryStream(bytes);
                BinaryReader br = new BinaryReader(istream);
                PacketType type = (PacketType) br.ReadInt32();

                switch(type)
                {
                    case PacketType.IdAllocate: //被Server分配ID
                        Address = br.ReadInt32();
                        Constants.Debug($"ClientSide: Allocated ID {Address}");
                        break;
                    case PacketType.ProtoPacket: //接收到Message
                        Message message = new Message();
                        message.MergeFrom(istream);
                        OnReceive?.Invoke(message);
                        Constants.Debug($"ClientSide: Data received {message.Content.GetType().FullName}");
                        break;
                    case PacketType.Disconnected: //主动Disconnect包
                        Constants.Debug($"ClientSide: Disconnect Message Received.");
                        Disconnect();
                        break;
                    default: //未知包
                        throw new Exception($"unknown Packet Type : {type}");
                }
                return HandleResult.Ok;
            };
            client.OnConnect += delegate (IClient sender)
            {
                MemoryStream ostream = new MemoryStream();
                BinaryWriter bw = new BinaryWriter(ostream);
                if (Address == -1) //未得到分配ID时请求ID
                {
                    bw.Write((int)PacketType.IdRequest);
                    Constants.Debug("ClientSide: Request ID");
                }
                else //断线重连，要求服务端分配到ID
                {
                    bw.Write((int)PacketType.IdAllocate);
                    bw.Write(Address);
                    Constants.Debug($"ClientSide: Using Pre-Allocated ID #{Address}");
                }
                byte[] raw = ostream.ToArray();
                client.Send(raw, raw.Length);
                return HandleResult.Ok;
            };
            client.OnClose += delegate (IClient sender, SocketOperation enOperation, int errorCode)
            {
                if (!Closed) //断线重连
                    while (!client.IsStarted)
                    {
                        Thread.Sleep(1000);
                        Constants.Debug($"ClientSide: Connecting to server {endPoint}");
                        client.Connect(endPoint.Address.ToString(), (ushort)endPoint.Port, false);
                    }
                return HandleResult.Ok;
            };
        }
        public void Connect(IPEndPoint endPoint)
        {
            this.endPoint = endPoint;
            while (!client.IsStarted)
            {
                Constants.Debug($"ClientSide: Connecting to server {endPoint}");
                client.Connect(endPoint.Address.ToString(), (ushort)endPoint.Port, false);
                Thread.Sleep(1000);
            }
        }

        public void Disconnect() //主动断开连接
        {
            Constants.Debug("ClientSide: Stopping");
            Closed = true;
            client.Stop();
            OnDisconnect?.Invoke();
        }

        public void Send(Message message) //发送Message
        {
            MemoryStream ostream = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ostream);
            bw.Write((int)PacketType.ProtoPacket);
            message.WriteTo(ostream);
            byte[] raw = ostream.ToArray();
            client.Send(raw, raw.Length);
            Constants.Debug($"ClientSide: Data sent {message.Content.GetType().FullName}");
        }

        public void Quit()
        {
            MemoryStream ostream = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ostream);
            bw.Write((int)PacketType.Quit);
            byte[] raw = ostream.ToArray();
            client.Send(raw, raw.Length);
            Disconnect();
        }

        public void Dispose()
        {
            client.Destroy();
            GC.SuppressFinalize(this);
        }

        public event OnReceiveCallback OnReceive;
        public event OnDisconnectCallback OnDisconnect; //被Server断开链接或客户端主动断开的时候触发
    }
}
