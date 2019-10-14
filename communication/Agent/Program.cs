
using System;
using HPSocketCS;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using Communication.Proto;
using System.Text;
using System.Net;

namespace Communication.Agent
{
    public class Program
    {
        private const int Interval = 100;
        private static IDClient client = new IDClient();
        private static IDServer server = new IDServer();
        private static System.Timers.Timer myTimer = new System.Timers.Timer();
        private static IPEndPoint Server;
        public static void Main(string[] args)
        {
            Console.Write("Server IP: ");
            string[] t = Console.ReadLine().Split(':');
            Server = new IPEndPoint(IPAddress.Parse(t[0]), Int32.Parse(t[1]));

            Console.Write("Agent Listen Port: ");
            server.Port = UInt16.Parse(Console.ReadLine());
            //init timer
            myTimer.Interval = Interval;
            myTimer.Elapsed += TimedUpdate;
            myTimer.Enabled = true;

            client.OnReceive += delegate (Message message)
            {
                server.Send(message.Content as Message);
            };

            server.OnReceive += delegate (Message message)
            {
                Message msg = new Message()
                {
                    Address = -1,
                    Content = message
                };
                client.Send(msg);
            };

            client.OnDisconnect += delegate ()
            {
                server.Stop();
                Environment.Exit(0);
            };

            server.OnAccept += delegate ()
            {
                Constants.Debug($"Player Connected: {server.Count}/{Constants.PlayerCount}");
                if (server.Count == Constants.PlayerCount)
                    client.Connect(Server);
            };

            server.Start();

            Thread.Sleep(Int32.MaxValue);
        }

        private static void TimedUpdate(object source, System.Timers.ElapsedEventArgs e)
        {

        }
            
    }

}
