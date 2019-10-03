
using System;
using System.Threading;
using HPSocketCS;
using System.Text;
using Google.Protobuf;
using System.IO;
using System.Timers;
using System.Collections.Generic;

namespace Communication.client
{
    namespace client
    {
        class Program
        {
            //TODO: use function of tcp agent
            static TcpClient client = new TcpPackClient();
            static TcpServer server = new TcpPackServer();
            private static int playerCount = 1;
            static int nClient = 0;
            //static DateTime StartTime = new DateTime();
            private const string ServerIP = "127.0.0.1";
            private const ushort ServerPort = 8885;
            private const int Interval = 1000;  //timely update
            static System.Timers.Timer myTimer = new System.Timers.Timer();
            //TODO: add reconnect function
            private static List<IntPtr> clientList = new List<IntPtr>();
                
            static HandleResult Start(IServer sender, IntPtr soListen)
            {
                Console.WriteLine($"server start {soListen}");
                return HandleResult.Ok;
            }

            static HandleResult Accept(IServer s, IntPtr connID, IntPtr pClient)
            {
                Console.WriteLine($"Accept {connID}:{pClient}");
                clientList.Add(connID);
                if (++nClient == playerCount)
                {
                    MemoryStream ms = new MemoryStream();
                    BinaryWriter br = new BinaryWriter(ms);
                    br.Write(0);
                    br.Write((int)PacketType.GameStart);
                    byte[] raw = ms.ToArray();
                    client.Send(raw, raw.Length);
                }
                return HandleResult.Ok;
            }

            private static HandleResult ServerReceive(IServer sender, IntPtr connID, byte[] bytes)
            {
                Console.WriteLine($"client receive {client} {Encoding.Default.GetString(bytes)}");
                MemoryStream ms = new MemoryStream();
                BinaryWriter bw = new BinaryWriter(ms);
                bw.Write(clientList.IndexOf(connID));
                bw.Write(bytes);
                byte[] raw = ms.ToArray();
                client.Send(raw, raw.Length);
                Console.WriteLine("data sent to server");
                return HandleResult.Ok;
            }
            private static HandleResult ClientReceive(IClient sender, byte[] bytes)
            {
                Console.WriteLine($"server receive {client} {Encoding.Default.GetString(bytes)}");
                server.Send(clientList[BitConverter.ToInt32(bytes)], new MemoryStream(bytes, 4, bytes.Length - 4).ToArray(), bytes.Length - 4);
                Console.WriteLine("data sent to client");
                return HandleResult.Ok;
            }
            static void Main(string[] args)
            {
                server.IpAddress = "127.0.0.1";
                server.Port = 9999;
                server.OnAccept += Accept;
                client.OnReceive += ClientReceive;
                server.OnReceive += ServerReceive;
                client.OnClose += TryReconnect;

                //init timer
                myTimer.Interval = Interval;
                myTimer.Elapsed += new ElapsedEventHandler(TimedUpdate);
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
            static void TimedUpdate(object source, ElapsedEventArgs e)
            {
                //TODO:Request info.
                //string str = "hello";
                //var mes = Encoding.Default.GetBytes(str);
                //client.Send(clientConid, mes, mes.Length);
            }
            
        }
    }

}
