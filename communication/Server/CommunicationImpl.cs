using System.Collections.Concurrent;
using System.Threading;
using Communication.Proto;
using System;
namespace Communication.Server
{
    public class CommunicationImpl : ICommunication
    {
        private IDServer server;
        private ManualResetEvent full;
        public ushort Port
        {
            get => server.Port;
            set => server.Port = value;
        }
        public event MessageHandler MsgProcess=null;
        public void OnNewMessage(MessageEventArgs e)
        {
            MsgProcess?.Invoke(this, e);
        }
        public void GameOver()
        {
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
            Constants.Debug("Waiting for clients");
            full.WaitOne();
            //此时应广播通知Client，不过应该由logic广播？
        }
        public void Initialize()
        {
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
