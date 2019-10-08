
namespace Communication.Server
{
    public delegate void SendMessageCallback(int client, string message);
    interface IGame
    {
        void Initialize(int playerCount);
        void GetMessage(int client, string message);
        void SetSendMessageCallback(SendMessageCallback callback);
        void ServerLoop();

    }
}
