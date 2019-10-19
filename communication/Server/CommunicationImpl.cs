using System;
using System.Collections.Concurrent;
using Communication.Proto;

namespace Communication.Server
{
    public class CommunicationImpl : ICommunication
    {
        private IDServer server;
        private bool full;
        public ushort Port
        {
            get => server.Port;
            set => server.Port = value;
        }
        public BlockingCollection<ServerMessage> MessageQueue { get; private set; }
        public void GameOver()
        {
            server.Stop();
        }

        public void GameStart()
        {
            server.OnReceive += delegate (Message message) //收到信息入消息队列
            {
                ServerMessage msg = new ServerMessage();
                msg.Agent = message.Address;
                message = message.Content as Message;
                msg.Client = message.Address;
                msg.Message = message.Content;
                MessageQueue.Add(msg);
            };

            server.OnAccept += delegate () //判断是否满人
            {
                Constants.Debug($"Agent Connected: {server.Count}/{Constants.AgentCount}");
                if (server.Count == Constants.AgentCount)
                {
                    full = true;
                    server.Pause();
                }
            };

            full = false;
            server.Start();
            Constants.Debug("Waiting for clients");
            while (!full) ;
            //此时应广播通知Client，不过应该由logic广播？
        }

        public void Initialize()
        {
            MessageQueue = new BlockingCollection<ServerMessage>();
            server = new IDServer();
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
    }
}
