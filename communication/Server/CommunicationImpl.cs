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
            server.OnReceive += delegate (Message message)
            {
                ServerMessage msg = new ServerMessage();
                msg.Agent = message.Address;
                message = message.Content as Message;
                msg.Client = message.Address;
                msg.Message = message.Content;
                MessageQueue.Add(msg);
            };

            server.OnAccept += delegate ()
            {
                Constants.Debug($"Agent Connected: {server.Count}/{Constants.AgentCount}");
                if (server.Count == Constants.AgentCount)
                    full = true;
            };

            full = false;
            server.Start();
            Constants.Debug("Waiting for clients");
            while (!full) ;
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
                Address = message.Agent,
                Content = new Message()
                {
                    Address = message.Client,
                    Content = message.Message
                }
            };
            server.Send(msg);
        }
    }
}
