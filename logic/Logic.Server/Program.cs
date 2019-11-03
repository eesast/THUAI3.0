using System;
using System.Collections.Generic;
using System.Text;
using Server;
using static Map;

namespace Server
{
    static class Program
    {
        public static Server server;
        public static void Main(string[] args)
        {
            InitializeMap();
            server = new Server();
        }
    }
}
