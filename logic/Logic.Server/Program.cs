using System;
using System.Collections.Generic;
using System.Text;
using static Map;

namespace Logic.Server
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
