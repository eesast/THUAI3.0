using HPSocketCS;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.IO;

namespace Communication.Proto
{
    internal delegate void OnReceiveCallback(Message message);
    internal delegate void OnAcceptCallback();
    internal delegate void OnClientQuit();
    internal sealed class IDServer : IDisposable
    {
        private ConcurrentDictionary<int, IntPtr> clientList;
        private readonly TcpServer server;
        private bool isListening;
        private object idLock = new object();

        public ushort Port
        {
            get => server.Port;
            set => server.Port = value;
        }

        public int Count => clientList.Count;
        public IDServer()
        {
            clientList = new ConcurrentDictionary<int, IntPtr>();
            server = new TcpPackServer();
            isListening = false;
            server.OnReceive += delegate (IServer sender, IntPtr connId, byte[] bytes)
            {
                MemoryStream istream = new MemoryStream(bytes);
                BinaryReader br = new BinaryReader(istream);
                PacketType type = (PacketType)br.ReadInt32();

                switch (type)
                {
                    case PacketType.IdAllocate: //客户端请求分配到，此时slot应已有，不需要扩容clientList
                        if (!isListening)
                        {
                            server.Disconnect(connId);
                            break;
                        }
                        clientList[br.ReadInt32()] = connId;
                        OnAccept?.Invoke();
                        int id = -1;
                        foreach (int key in clientList.Keys)
                        {
                            if (clientList[key] == connId)
                            {
                                id = key;
                                break;
                            }
                        }
                        Constants.Debug($"ServerSide: Using Pre-Allocated ID #{id}");
                        break;
                    case PacketType.IdRequest: //客户端请求ID
                        lock (idLock)
                        {
                            if (!isListening)
                            {
                                server.Disconnect(connId);
                                break;
                            }
                            id = -1;
                            for (int i = 0; ; i++)
                            {
                                if (!clientList.ContainsKey(i))
                                {
                                    id = i;
                                    break;
                                }
                            }
                            clientList.TryAdd(id, connId);
                            MemoryStream ostream = new MemoryStream();
                            BinaryWriter bw = new BinaryWriter(ostream);
                            bw.Write((int)PacketType.IdAllocate);
                            bw.Write(id);
                            byte[] raw = ostream.ToArray();
                            server.Send(connId, raw, raw.Length);
                            OnAccept?.Invoke();
                            Constants.Debug($"ServerSide: Allocate ID #{id}");
                        }
                        break;
                    case PacketType.ProtoPacket: //接收到包
                        Message message = new Message();
                        message.MergeFrom(istream);
                        //Constants.Debug($"ServerSide: Data received {message.Content.GetType().FullName}");
                        id = -1;
                        foreach (int key in clientList.Keys)
                        {
                            if (clientList[key] == connId)
                            {
                                id = key;
                                break;
                            }
                        }
                        InternalSend(message, id);
                        break;
                    case PacketType.Quit:  // 申请退出房间
                        id = -1;
                        foreach (int key in clientList.Keys)
                        {
                            if (clientList[key] == connId)
                            {
                                id = key;
                                break;
                            }
                        }
                        IntPtr tmp;
                        clientList.TryRemove(id, out tmp);
                        Constants.Debug($"ServerSide: ID #{id } has quited");
                        server.Disconnect(tmp);
                        InternalQuit?.Invoke();
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
            Console.WriteLine($"Server has started on 0.0.0.0:{server.Port}");
            isListening = true;
            Console.WriteLine("Server is listening");
        }

        public void Pause()
        {
            isListening = false;
            Console.WriteLine("Server has paused, stop listening");
        }

        public void Resume()
        {
            isListening = true;
            Console.WriteLine("Server resume to listen");
        }

        public void Stop()
        {
            Constants.Debug("ServerSide: Stopping");
            MemoryStream ostream = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ostream);
            bw.Write((int)PacketType.Disconnected);
            byte[] raw = ostream.ToArray();
            foreach (IntPtr client in clientList.Values)
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
                    foreach (IntPtr client in clientList.Values)
                        server.Send(client, raw, raw.Length);
                else
                {
                    if (clientList.ContainsKey(message.Address)) server.Send(clientList[message.Address], raw, raw.Length);
                }

            }
        }

        public void Send(Message message) //发包
        {
            if (clientList.Count > 0)
            {
                InternalSend(message, -1);
                //Constants.Debug($"ServerSide: Data sent {message.Content.GetType().FullName}");
            }

        }

        public void Dispose()
        {
            server.Destroy();
            GC.SuppressFinalize(this);
        }

        public event OnReceiveCallback OnReceive;
        public event OnAcceptCallback OnAccept; //当客户端被分配到/已请求ID而非TCP的Accept时触发
        public event OnClientQuit InternalQuit;  // client退出事件
    }
}
