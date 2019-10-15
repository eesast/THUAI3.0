using Communication.CAPI;
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

            API.ConnectServer(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8081));
            new Thread(new ThreadStart(delegate ()
            {
                while (API.Connected)
                {
                    string buffer = API.BufferedMessage();
                    if (buffer != "") Console.WriteLine(buffer);
                }
            })).Start();
            string message = "Connected!";
            while (API.Connected)
            {
                API.SendChatMessage(message);
                message = Console.ReadLine();
            }
        }
    }
}
