
using System;
using System.Net;

namespace Communication.Proto
{
    public static class Constants
    {
        public delegate void DebugFunc(string DebugMessage);
        public static readonly int PlayerCount = 1;
        public static readonly int AgentCount = 1;
        public static readonly ushort ServerPort = 8888;
        public static readonly ushort AgentPort = 8887;
        public static readonly IPEndPoint Server = new IPEndPoint(IPAddress.Parse("127.0.0.1"), ServerPort);
        public static readonly IPEndPoint Agent = new IPEndPoint(IPAddress.Parse("127.0.0.1"), AgentPort);
        public static DebugFunc Debug = delegate (string DebugMessage)
        {
            Console.WriteLine(DebugMessage);
        };
    }
}
