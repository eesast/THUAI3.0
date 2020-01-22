using System.Collections.Concurrent;
using System.Threading;
using Communication.Proto;
using System;
using System.Reflection;
using System.Net;
using Google.Protobuf;
using System.Diagnostics;
using System.IO;

namespace Communication.Server
{
    public sealed class CommunicationImpl : ICommunication
    {
        private IDServer server;
        private ManualResetEvent full;
        private IDClient client;
        private DockerGameStatus status;

        public IPEndPoint EndPoint { get; set; }
        public string ID { get; set; }

        private static Process StartAgent()
        {
            return Process.Start("Communication.Agent.exe", $"127.0.0.1:{Constants.ServerPort} {Constants.AgentPort}");
        }

        public int PlayerCount => server.Count;

        public event MessageHandler MsgProcess;

        public void OnNewMessage(MessageEventArgs e)
        {
            MsgProcess?.Invoke(this, e);
        }
        public void GameOver()
        {
            status = DockerGameStatus.PendingTerminated;
            server.Stop();
        }

        public void GameStart()
        {
            server.OnReceive += delegate (Message message) //收到信息后将消息传给逻辑处理
            {
                ServerMessage msg = new ServerMessage();
                msg.Agent = message.Address;
                message = message.Content as Message;
                msg.Client = message.Address;
                msg.Message = message.Content;
                MessageEventArgs e = new MessageEventArgs(msg);
                OnNewMessage(e);
            };

            server.OnAccept += delegate () //判断是否满人
            {
                Constants.Debug($"Agent Connected: {server.Count}/{Constants.AgentCount}");
                if (server.Count == Constants.AgentCount)
                {
                    full.Set();
                    server.Pause();
                }
            };
            server.InternalQuit += delegate ()
            {
                server.Resume();
            };
            full = new ManualResetEvent(false);
            server.Start();
            StartAgent();
            status = DockerGameStatus.Listening;
            Constants.Debug("Waiting for clients");
            full.WaitOne();
            //此时应广播通知Client，不过应该由logic广播？
            status = DockerGameStatus.Heartbeat;
        }
        public void Initialize()
        {
            server = new IDServer();
            client = new IDClient();
            status = DockerGameStatus.Idle;
            //connect to the rest server

            client.OnReceive += (Message message) =>
            {
                IMessage content = message.Content;
                switch (content.GetType().ToString())
                {
                    case "Communication.Proto.ClientFile":
                        ClientFile file = content as ClientFile;
                        string filename = file.ClientID + ".exe";
                        using (var fs = new FileStream(filename, FileMode.Create))
                            fs.Write(file.Data.ToByteArray(), 0, file.Data.Length);
                        Process.Start(filename);
                        break;
                    default:
                        throw new Exception($"Unexcepted packet type received : {content.GetType()}");
                }
            };

            client.Connect(EndPoint);
            client.Send(new Message
            {
                Address = -1,
                Content = new LauncherID
                {
                    Id = ID
                }
            });

            new Thread(new ThreadStart(() =>
            {
                DockerGameStatus t;
                do
                {
                    t = status;
                    client.Send(new Message
                    {
                        Address = -1,
                        Content = new GameStatus
                        {
                            ClientCount = server.Count,
                            Status = (int)t
                        }
                    });
                    Thread.Sleep(Constants.HeartbeatInternal);
                } while (t != DockerGameStatus.PendingTerminated);
                client.Dispose();
            })).Start();
        }

        public void SendMessage(ServerMessage message)
        {
            Message msg = new Message()
            {
                Address = message.Agent, //封包Agent
                Content = new Message()
                {
                    Address = message.Client, //封包Cient
                    Content = message.Message
                }
            };
            server.Send(msg);
        }

        public void Dispose()
        {
            server.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
