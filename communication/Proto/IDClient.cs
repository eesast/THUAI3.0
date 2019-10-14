﻿using HPSocketCS;
using System;
using System.IO;
using System.Net;

namespace Communication.Proto
{
    internal delegate void OnDisconnectCallback();
    internal class IDClient
    {
        private IPEndPoint endPoint;
        private TcpClient client;
        
        public int Address { get; private set; }
        public bool Closed { get; private set; }
        public IDClient()
        {
            client = new TcpClient();
            Address = -1;
            Closed = false;

            client.OnReceive += delegate (IClient sender, byte[] bytes)
            {
                MemoryStream istream = new MemoryStream(bytes);
                BinaryReader br = new BinaryReader(istream);
                PacketType type = (PacketType) br.ReadInt32();

                switch(type)
                {
                    case PacketType.IdAllocate:
                        Address = br.ReadInt32();
                        Constants.Debug($"ClientSide: Allocated ID {Address}");
                        break;
                    case PacketType.ProtoPacket:
                        Message message = new Message();
                        message.MergeFrom(istream);
                        OnReceive?.Invoke(message);
                        Constants.Debug($"ClientSide: Data received {message.Content.GetType().FullName}");
                        break;
                    case PacketType.Disconnected:
                        Constants.Debug($"ClientSide: Disconnect Message Received.");
                        Disconnect();
                        break;
                    default:
                        throw new Exception($"unknown Packet Type : {type}");
                }
                return HandleResult.Ok;
            };
            client.OnConnect += delegate (IClient sender)
            {
                MemoryStream ostream = new MemoryStream();
                BinaryWriter bw = new BinaryWriter(ostream);
                if (Address == -1)
                {
                    bw.Write((int)PacketType.IdRequest);
                    Constants.Debug("ClientSide: Request ID");
                }

                else
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
                if (!Closed) client.Connect(endPoint.Address.ToString(), (ushort) endPoint.Port, false);
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
            }
        }

        public void Disconnect()
        {
            Constants.Debug("ClientSide: Stopping");
            Closed = true;
            client.Stop();
            OnDisconnect?.Invoke();
        }

        public void Send(Message message)
        {
            MemoryStream ostream = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ostream);
            bw.Write((int)PacketType.ProtoPacket);
            message.WriteTo(ostream);
            byte[] raw = ostream.ToArray();
            client.Send(raw, raw.Length);
            Constants.Debug($"ClientSide: Data sent {message.Content.GetType().FullName}");
        }

        public event OnReceiveCallback OnReceive;
        public event OnDisconnectCallback OnDisconnect;
    }
}
