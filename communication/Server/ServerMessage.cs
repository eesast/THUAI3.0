using Communication.Proto;
using System.IO;

namespace Communication.Server
{
    public class ServerMessage
    {
        public int Agent, Client;
        public Message Message;

        public ServerMessage()
        {
            Message = new Message();
        }
    }
}
