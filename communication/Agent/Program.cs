using System;
using System.Threading;
using Communication.Proto;
using System.Net;
using Microsoft.Extensions.CommandLineUtils;

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
            var app = new CommandLineApplication();
            app.HelpOption("-h|--help");
            var server = app.Option("-s|--server", "game server endpoint", CommandOptionType.SingleValue);
            var port = app.Option("-p|--port", "agent port", CommandOptionType.SingleValue);
            var token = app.Option("-h|--token", "player token, leave empty to enable offline mode", CommandOptionType.SingleValue);
            app.OnExecute(() => MainInternal(server.Value(), ushort.Parse(port.Value()), token.Value()));
            app.Execute(args);
        }

        private static int MainInternal(string ep, ushort port, string token)
        {
            string[] t = ep.Split(':');
            Console.WriteLine("Server endpoint: " + t);
            Server = new IPEndPoint(IPAddress.Parse(t[0]), int.Parse(t[1]));
            server.Port = port;
            Console.WriteLine("Agent Listen Port: " + server.Port.ToString());
            Console.WriteLine("Client Token: " + (token ?? "<offline>"));

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

            client.OnDisconnect += delegate ()
             {
                 if (server.Count == 0)
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
                    if (token != "offline")
                    {
                        client.Send(new Message //发送token
                        {
                            Address = -1,
                            Content = new PlayerToken
                            {
                                Token = token
                            }
                        });
                    }
                }
            };

            server.Start();

            Constants.Debug = new Constants.DebugFunc((str) => { });//注释掉这一行恢复Debug输出

            Thread.Sleep(int.MaxValue);
            return 0;
        }

        private static void TimedUpdate(object source, System.Timers.ElapsedEventArgs e) //轮询预留
        {

        }

    }

}
