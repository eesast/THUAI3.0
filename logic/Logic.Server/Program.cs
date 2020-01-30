using System;
using System.Collections.Generic;
using System.Text;
using Communication.Proto;
using static Logic.Constant.Map;

namespace Logic.Server
{
    static class Program
    {
        private static MessageToClient _messageToClient;
        public static MessageToClient MessageToClient
        {
            get
            {
                _messageToClient = _messageToClient ?? new MessageToClient();
                return _messageToClient;
            }
        }
        private static Server server;
        public static void Main(string[] args)
        {
            InitializeMap();
            server = new Server();
        }
    }
}
