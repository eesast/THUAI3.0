using System.Collections.Concurrent;

namespace Communication.Server
{
    public interface ICommunication
    {
        ushort Port { get; set; }
        BlockingCollection<ServerMessage> MessageQueue { get; }
        void Initialize();
        void GameStart(); /* This function should return when all the players are connected to server*/
        void GameOver();
        void SendMessage(ServerMessage message);

    }
}
