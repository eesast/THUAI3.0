using System;
using System.Diagnostics;
using System.Net;

namespace Communication.Proto
{
    public static class Constants
    {
        public delegate void DebugFunc(string DebugMessage);
        public static ushort PlayerCount = 2;
        public static ushort AgentCount = 1;
        public static ushort ServerPort = 10086;
        public static readonly ushort AgentPort = 8887;
        public static int MaxMessage = 3;
        public static double TimeLimit = 1000;   // 在TimeLimt内agent只会转发MaxMessage条消息
        public static readonly IPEndPoint Server = new IPEndPoint(IPAddress.Loopback, ServerPort);
        public static readonly IPEndPoint Agent = new IPEndPoint(IPAddress.Loopback, AgentPort);
        public static readonly int HeartbeatInternal = 1000;
        public static readonly int TokenExpire = 3600;

        public static DebugFunc Debug = delegate (string DebugMessage)
        {
            var stack = new StackTrace();
            var method = stack.GetFrame(1).GetMethod();
            Console.WriteLine($"[{method.DeclaringType.Name}/{method.Name}] {DebugMessage}");
        };
    }
}
