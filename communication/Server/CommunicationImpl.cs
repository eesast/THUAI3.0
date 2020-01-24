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
            status = DockerGameStatus.Idle;
            //connect to the rest server

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
