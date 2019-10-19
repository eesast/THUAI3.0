using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Communication.CAPI
{
    public interface ICAPI
    {
        /* Connect Control */
        bool Connected { get; }
        int PlayerId { get; }
        int AgentId { get; }
        int PlayerCount { get; }
        int AgentCount { get; }
        void Initialize();
        void ConnectServer(IPEndPoint endPoint);
        /* Game Control */

        void SendChatMessage(string Message);

        /* Data Control */
        string BufferedMessage();
    }
}
