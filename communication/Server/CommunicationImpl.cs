using System.Collections.Concurrent;
using System.Threading;
using Communication.Proto;
using System;
using System.Reflection;
using System.Net;
using Google.Protobuf;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Communication.Server
{
    public sealed class CommunicationImpl : ICommunication
    {
        private readonly IDServer server = new IDServer();
        private readonly ManualResetEvent full = new ManualResetEvent(false);
        public string Token { get; set; }

        private void PostAsync(string url, string data) //不清楚post格式所以暂不使用json
        {
            Task.Run(() =>
            {
                try
                {
                    using (var client = new HttpClient())
                        client.PostAsync(url, new StringContent(data));
                }
                catch (Exception e)
                {
                    Constants.Debug(e.ToString());
                }
            });
        }

        private void NoticeServer(string token, DockerGameStatus status)
        {
            if (token == null)
                PostAsync("http://localhost/", $"token={Token}&status={status}");
            else
                PostAsync("http://localhost/", $"token={Token}&client={token}");
        }

        private DockerGameStatus Status
        {
            set
            {
                NoticeServer(null, value);
            }
        }

        public string ID { get; set; }

        public int PlayerCount => server.Count;

        public ushort ServerPort
        {
            get => server.Port;
            set => server.Port = value;
        }

        public event MessageHandler MsgProcess;

        public void OnNewMessage(MessageEventArgs e)
        {
            MsgProcess?.Invoke(this, e);
        }
        public void GameOver()
        {
            Status = DockerGameStatus.PendingTerminated;
            server.Stop();
        }

        public void GameStart()
        {
            server.OnReceive += delegate (Message message) //收到信息后将消息传给逻辑处理
            {
                if (message.Content is PlayerToken token)
                {
                    NoticeServer(token.Token, default);
                    return;
                }
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
            full.Reset();
            server.Start();
            Status = DockerGameStatus.Listening;
            Constants.Debug("Waiting for clients");
            full.WaitOne();
            //此时应广播通知Client，不过应该由logic广播？
            Status = DockerGameStatus.Heartbeat;
        }

        //TODO: should be moved to the constructor
        public void Initialize()
        {
            Status = DockerGameStatus.Idle;
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
            full.Dispose();
        }
    }
}
