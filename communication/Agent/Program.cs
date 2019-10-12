
using System;
using HPSocketCS;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using Communication.Proto;
using System.Text;

namespace Communication.Agent
{
    class Program
    {
        //TODO: use function of tcp agent
        static TcpClient client = new TcpPackClient();
        static TcpServer server = new TcpPackServer();

        static int nClient = 0;
        static int MyID = -1;
        private const string ServerIP = "127.0.0.1";
        private const ushort ServerPort = 8080;
        private const int Interval = 1000;  //timely update

        static System.Timers.Timer myTimer = new System.Timers.Timer();
        //TODO: add reconnect function
        private static List<IntPtr> clientList = new List<IntPtr>();

        private static HandleResult Start(IServer sender, IntPtr soListen)
        {
            Console.WriteLine($"server start {soListen}");
            return HandleResult.Ok;
        }

        private static HandleResult Accept(IServer s, IntPtr connID, IntPtr pClient)
        {
            Console.WriteLine($"Accept {connID}:{pClient}");
            clientList.Add(connID);
            if (++nClient == Constants.PlayerCount)
            {
                MemoryStream ms = new MemoryStream();
                ms.Write(BitConverter.GetBytes((int)PacketType.PlayerReady), 0, 4);
                byte[] raw = ms.ToArray();
                client.Send(raw, raw.Length);
            }
            return HandleResult.Ok;
        }

        private static HandleResult ServerReceive(IServer sender, IntPtr connID, byte[] bytes)
        {
            PacketType type = (PacketType)BitConverter.ToInt32(bytes, 0);
            switch (type)
            {
                case PacketType.ProtoPacket:
                    byte[] clientID = BitConverter.GetBytes(clientList.IndexOf(connID));
                    MemoryStream ms = new MemoryStream();
                    ms.Write(BitConverter.GetBytes((int)type), 0, 4);
                    ms.Write(clientID,0,4);
                    ms.Write(bytes, 4, bytes.Length-4);
                    byte[] mes = ms.ToArray();
                    client.Send(mes, mes.Length);
                    Console.WriteLine($"C2S Packet sent ({mes.Length} bytes)");
                    break;
                case PacketType.IdRequest:
                    clientID = BitConverter.GetBytes(clientList.IndexOf(connID));
                    ms = new MemoryStream();
                    ms.Write(BitConverter.GetBytes((int)PacketType.IdAllocate), 0, 4);
                    Console.WriteLine(clientID);
                    ms.Write(clientID, 0, clientID.Length);
                    mes = ms.ToArray();
                    server.Send(connID, mes, mes.Length);
                    Console.WriteLine($"Allocate ID {clientList.IndexOf(connID)} to {connID}");
                    break;
                case PacketType.IdAllocate:
                    nClient--;
                    int index = BitConverter.ToInt32(bytes, 4);
                    clientList.Remove(connID);
                    clientList[index] = connID;
                    break;
            }


            return HandleResult.Ok;
        }
        private static HandleResult ClientReceive(IClient sender, byte[] bytes)
        {
            PacketType type = (PacketType)BitConverter.ToInt32(bytes, 0);
            switch (type)
            {
                case PacketType.ProtoPacket:
                    int clientID = BitConverter.ToInt32(bytes, 4);
                    MemoryStream ms = new MemoryStream();
                    ms.Write(BitConverter.GetBytes((int)PacketType.ProtoPacket), 0, 4);
                    ms.Write(bytes, 8, bytes.Length - 8);
                    byte[] mes = ms.ToArray();
                    if (clientID == -1)
                        foreach (IntPtr client in clientList)
                            server.Send(client, mes, mes.Length);
                    else
                        server.Send(clientList[clientID], mes, mes.Length);
                    Console.WriteLine($"S2C {(clientID == -1 ? "BroadCast" : "Packet")} sent ({bytes.Length} bytes)");
                    break;
                
            }
                    return HandleResult.Ok;
        }
        static void Main(string[] args)
        {
            server.IpAddress = "127.0.0.1";
            server.Port = 8081;
            server.OnAccept += Accept;
            client.OnReceive += ClientReceive;
            server.OnReceive += ServerReceive;
            client.OnClose += TryReconnect;

            //init timer
            myTimer.Interval = Interval;
            myTimer.Elapsed += TimedUpdate;
            myTimer.Enabled = true;

            while (!client.Connect(ServerIP, ServerPort))
            {
                Console.WriteLine("connection failed");
                Thread.Sleep(1000);
            }
            if (MyID == -1)
            {
                MemoryStream ms = new MemoryStream();
                ms.Write(BitConverter.GetBytes((int)PacketType.IdRequest), 0, 4);
                byte[] mes = ms.ToArray();
                client.Send(mes, mes.Length);
            }
            Console.WriteLine("agent connected");
            server.Start();
            Console.ReadLine();
        }
        static HandleResult TryReconnect(IClient c, SocketOperation enOperation, int errcode)
        {
            Console.WriteLine("restarting");
            while (!client.Connect(ServerIP, ServerPort))
            {
                Console.WriteLine("connection failed");
                Thread.Sleep(1000);
            }
            Console.WriteLine("agent connected");
            MemoryStream ms = new MemoryStream();
            if (MyID == -1)
            {
                ms.Write(BitConverter.GetBytes((int)PacketType.IdRequest), 0, 4);
            }
            else
            {
                ms.Write(BitConverter.GetBytes((int)PacketType.IdAllocate), 0, 4);
                ms.Write(BitConverter.GetBytes(MyID), 0, 4);
            }
            byte[] mes = ms.ToArray();
            client.Send(mes, mes.Length);
            return HandleResult.Ok;
        }
        private static void TimedUpdate(object source, System.Timers.ElapsedEventArgs e)
        {
            MemoryStream ms = new MemoryStream();
            ms.Write(BitConverter.GetBytes((int)PacketType.RequestInfo), 0, 4);
            byte[] raw = ms.ToArray();
            foreach (IntPtr client in clientList)
                server.Send(client, raw, raw.Length);
        }
            
    }

}
