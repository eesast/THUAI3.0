
using System;
using HPSocketCS;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using Communication.Proto;

namespace Communication.Agent
{
    class Program
    {
        //TODO: use function of tcp agent
        static TcpClient client = new TcpPackClient();
        static TcpServer server = new TcpPackServer();

        static int nClient = 0;

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
            byte[] clientID = BitConverter.GetBytes(clientList.IndexOf(connID));
            for (int i = 0; i < 4; ++i) bytes[i + 4] = clientID[i];
            client.Send(bytes, bytes.Length);
            Console.WriteLine($"C2S Packet sent ({bytes.Length} bytes)");
            return HandleResult.Ok;
        }
        private static HandleResult ClientReceive(IClient sender, byte[] bytes)
        {
            int clientID = BitConverter.ToInt32(bytes, 4);
            if (clientID == -1)
                foreach (IntPtr client in clientList)
                    server.Send(client, bytes, bytes.Length);
            else
                server.Send(clientList[clientID], bytes, bytes.Length);
            Console.WriteLine($"S2C {(clientID == -1 ? "BroadCast" : "Packet")} sent ({bytes.Length} bytes)");
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
