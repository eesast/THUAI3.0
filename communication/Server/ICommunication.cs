using System.Collections.Concurrent;
using Communication.Proto;

namespace Communication.Server
{
    public interface ICommunication
    {
        ushort Port { get; set; }
        BlockingCollection<Message> MessageQueue { get; }
        void Initialize();
        void GameStart(); /* This function should return when all the players are connected to server*/
        void GameOver();
        void SendMessage(Message message);

    }
}
