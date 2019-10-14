using Communication.Proto;
using Google.Protobuf;
using System.IO;

namespace Communication.Server
{
    public class ServerMessage
    {
        public int Agent, Client;
        public IMessage Message;
    }
}
