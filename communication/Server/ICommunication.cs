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
        /// be aware it means the http port but not the server listening port
        /// Please change Constants.ServerPort
        /// </summary>
        IPEndPoint EndPoint { get; set; }

        /// <summary>
        /// my id in docker for futher usaae
        /// </summary>
        string ID { get; set; }
        void Initialize();
        void GameStart(); /* This function should return when all the players are connected to server*/
        void GameOver();
        void SendMessage(ServerMessage message);

        event MessageHandler MsgProcess;
        void OnNewMessage(MessageEventArgs e);
    }
}
