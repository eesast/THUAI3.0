using System.Collections.Concurrent;
using System;
using System.Net;

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
    public interface ICommunication : IDisposable
    {
        int PlayerCount { get;}

        /// <summary>
        /// Token for the client api
        /// </summary>
        string Token { get; set; }
        ushort ServerPort { get; set; }
        void Initialize();
        void GameStart(); /* This function should return when all the players are connected to server*/
        void GameOver();
        void SendMessage(ServerMessage message);

        event MessageHandler MsgProcess;
        //maybe should be private?
        void OnNewMessage(MessageEventArgs e);
    }
}
