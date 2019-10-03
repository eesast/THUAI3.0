using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using Google.Protobuf;
using HPSocketCS;
using Logic.Server;

namespace Communication.Server
{
    class Program
    {
        private static TcpServer server = new TcpPackServer();
        private static int playerCount = 1;
        private static List<IntPtr> agentList = new List<IntPtr>();
        private static IGame game = new Game();
        private static HandleResult OnAccept(HPSocketCS.IServer sender, IntPtr connId, IntPtr pClient)
        {
            Console.WriteLine("agent accepted");
            agentList.Add(connId);
            return HandleResult.Ok;
        }

        private static HandleResult OnReceive(HPSocketCS.IServer sender, IntPtr connId, byte[] bytes)
        {
            Console.WriteLine("data received");
            int client;
            PacketType type;
            client = BitConverter.ToInt32(bytes, 0);
            type = (PacketType) BitConverter.ToInt32(bytes, 4);
            switch (type)
            {
                case PacketType.GameStart:
                    playerCount = 0;
                    break;
                case PacketType.ClientPacket:
                    ClientPacket packet = ClientPacket.Parser.ParseFrom(new MemoryStream(bytes, 8, bytes.Length - 8));
                    game.GetMessage(client, packet.Data);
                    break;
            }
            return HandleResult.Ok;
        }
        public static void Main(string[] args)
        {
            server.Port = 8885;
            server.OnAccept += OnAccept;
            server.OnReceive += OnReceive;
            game.Initialize(playerCount);
            game.SetSendMessageCallback(delegate (int client, string message)
            {
                Console.WriteLine("data sent");
                MemoryStream ms = new MemoryStream();
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    ServerPacket packet = new ServerPacket();
                    bw.Write(client);
                    bw.Write((int)PacketType.ServerPacket);
                    packet.Data = message;
                    packet.WriteTo(ms);
                    byte[] raw = ms.ToArray();
                    server.Send(agentList[0], raw, raw.Length);
                }
            });
            server.Start();
            Console.WriteLine("server started");
            while (playerCount != 0) ;
            Console.WriteLine("game started");
            game.ServerLoop();
        }
    }
}
