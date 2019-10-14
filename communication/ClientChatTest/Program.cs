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

            Console.Write("Agent IP: ");
            string[] agent = Console.ReadLine().Split(':');
            API.ConnectServer(new IPEndPoint(IPAddress.Parse(agent[0]), Int32.Parse(agent[1])));

            new Thread(new ThreadStart(delegate ()
            {
                while (API.Connected)
                {
                    string buffer = API.BufferedMessage();
                    if (buffer != "") Console.Write(buffer);
                }
            })).Start();
            string message = "Connected!";
            while (API.Connected)
            {
                API.SendChatMessage(message + $" (agent: {API.AgentId}/{API.AgentCount}, player: {API.PlayerId}/{API.PlayerCount})");
                message = Console.ReadLine();
            }

            Console.WriteLine("Disconnected from server.");
            Console.ReadLine();
        }
    }
}
