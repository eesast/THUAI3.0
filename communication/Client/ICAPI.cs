using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;


namespace Communication.CAPI
{
    public delegate void ReceiveMessageCallback(IMessage message);
    public interface ICAPI
    {
        /* Connect Control */
        float Ping { get; }
        bool Connected { get;}
        int PlayerId { get; }
        int AgentId { get; }
        int PlayerCount { get; }
        int AgentCount { get; }
        void Initialize();
        void ConnectServer(IPEndPoint endPoint);
        void Refresh();
        /* Game Control */

        void SendChatMessage(string Message);
        void SendQuitMessage();

        /* Data Control */
        string BufferedMessage();

        void SendMessage(IMessage message);
        
        event ReceiveMessageCallback ReceiveMessage;
        
    }
}
