using Communication.Proto;
using Google.Protobuf;
using System.IO;

namespace Communication.Server
{
    public class ServerMessage //和logic通信时使用的Message
    {
        public int Agent, Client;
        public IMessage Message;
    }
}
