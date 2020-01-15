using Communication.CAPI;
using Communication.Proto;
using System;
using System.Net;
using System.Threading;

namespace Communication.ClientChatTest
{
    public class Program
    {
        private static ICAPI API;
        public static void Main(string[] args)
        {
            API = new API();
            API.Initialize();
            Constants.Debug = delegate (string DebugMessage)
            {

            };

            Console.Write("Agent IP&Port: ");
            string[] agent = Console.ReadLine().Split(':');
            API.ConnectServer(new IPEndPoint(IPAddress.Parse(agent[0]), Int32.Parse(agent[1])));

            new Thread(new ThreadStart(delegate ()
            {
                while (API.Connected)
                {
                    string buffer = API.BufferedMessage();
                    if (buffer != "") Console.Write(buffer);
                    else
                    {
                        API.Refresh();
                        Console.WriteLine($"Ping : {API.Ping}ms");
                        Thread.Sleep(200);
                    }
                }
            })).Start(); //buffer监视线程，没错这个cpu占用很高，不过毕竟是demo
            string message = "Connected!";
            while (API.Connected) //Console输入发送给Server
            {
                API.SendChatMessage(message + $" (agent: {API.AgentId}/{API.AgentCount}, player: {API.PlayerId}/{API.PlayerCount})");
                message = Console.ReadLine();
            }

            Console.WriteLine("Disconnected from server.");
            Console.ReadLine();
        }
    }
}
