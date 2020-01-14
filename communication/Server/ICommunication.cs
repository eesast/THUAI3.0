using System.Collections.Concurrent;
using System;

namespace Communication.Server
{
    public delegate void MessageHandler(Object comm, MessageEventArgs e);
    public class MessageEventArgs : EventArgs
    {
        public readonly ServerMessage message;
        public MessageEventArgs(ServerMessage mes)
        {
            this.message = mes;
        }
    }
    public interface ICommunication
    {
        ushort Port { get; set; }
        void Initialize();
        void GameStart(); /* This function should return when all the players are connected to server*/
        void GameOver();
        void SendMessage(ServerMessage message);

        event MessageHandler MsgProcess;
        void OnNewMessage(MessageEventArgs e);
    }
}
