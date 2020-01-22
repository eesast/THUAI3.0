using System;
using System.Threading;
using Communication.Proto;
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
        private static int MessageLimit = Constants.MaxMessage;
        private static void TimeCount(object source, System.Timers.ElapsedEventArgs e) //倒计时
        {
            MessageLimit++;
        }
        public static void Main(string[] args)
        {
            string[] t = { "127.0.0.1", "8888" };
            Console.WriteLine("Server IP&Port: " + t[0] + t[1]);
            //string[] t = Console.ReadLine().Split(':');
            Server = new IPEndPoint(IPAddress.Parse(t[0]), Int32.Parse(t[1]));

            server.Port = 30000;
            Console.WriteLine("Agent Listen Port: " + server.Port.ToString());
            //init timer
            myTimer.Interval = Interval;
            myTimer.Elapsed += TimedUpdate;
            myTimer.Enabled = true;
            client.OnReceive += delegate (Message message)
            {
                server.Send(message.Content as Message); //向客户端转发Content
            };
            server.InternalQuit += delegate ()
              {
                  client.Quit();
                  server.Resume();
              };
            server.OnReceive += delegate (Message message)
            {
                if (MessageLimit > 0)
                {
                    client.Send(message.Content as Message);
                    MessageLimit--;
                    System.Timers.Timer timer = new System.Timers.Timer(Constants.TimeLimit);
                    timer.AutoReset = false;
                    timer.Elapsed += TimeCount;
                    timer.Start();
                }
            };

            client.OnDisconnect+= delegate ()
            {
                if(server.Count==0)
                { 
                    server.Stop(); //主动Disconnect意味着游戏结束，关闭Agent。
                    Environment.Exit(0);               
                }

            };

            server.OnAccept += delegate ()
            {
                Constants.Debug($"Player Connected: {server.Count}/{Constants.PlayerCount}");
                if (server.Count == Constants.PlayerCount)
                {
                    server.Pause();
                    client.Connect(Server); //客户端满人后再向Server发送连接请求，可以省略GameStart包
                }
            };
            server.Start();

            Thread.Sleep(Int32.MaxValue);
        }

        private static void TimedUpdate(object source, System.Timers.ElapsedEventArgs e) //轮询预留
        {

        }
            
    }

}
