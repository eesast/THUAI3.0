using Communication.Proto;
using Logic.Constant;
using System;
using System.Collections.Generic;
using static Logic.Constant.MapInfo;
using CommandLine;

namespace Logic.Server
{
    static class Program
    {
        private static Random _random;
        public static Random Random
        {
            get
            {
                _random = _random ?? new Random(DateTime.Now.Millisecond);
                return _random;
            }
        }

        private static MessageToClient _messageToClient;
        public static MessageToClient MessageToClient
        {
            get
            {
                _messageToClient = _messageToClient ?? new MessageToClient();
                return _messageToClient;
            }
        }
        public static readonly object MessageToClientLock = new object();


        public static void InitializeMap()
        {
            for (uint x = 0; x < WorldMap.Width; x++)
            {
                for (uint y = 0; y < WorldMap.Height; y++)
                {
                    switch (map[x, y])
                    {
                        case 0: break;
                        case 1: new TaskPoint(x + 0.5, y + 0.5).Parent = WorldMap; break;
                        case 2: new FoodPoint(x + 0.5, y + 0.5).Parent = WorldMap; break;
                        case 3: new Cooker(x + 0.5, y + 0.5).Parent = WorldMap; break;
                        case 4: new RubbishBin(x + 0.5, y + 0.5).Parent = WorldMap; break;
                        case 5: new Wall(x + 0.5, y + 0.5).Parent = WorldMap; break;
                        case 6: new Table(x + 0.5, y + 0.5).Parent = WorldMap; break;
                    }
                }
            }
        }

        public static Dictionary<Tuple<int, int>, Player> PlayerList = new Dictionary<Tuple<int, int>, Player>();
        private static Server server;
        public static void Main(string[] args)
        {
            Parser.Default.ParseArguments<AugmentOptions>(args)
                  .WithParsed<AugmentOptions>(o =>
                  {
                      if (!Convert.ToBoolean(o.debugLevel & 1))
                          Server.ServerDebug = new Action<string>(s => { });
                      if(!Convert.ToBoolean(o.debugLevel & 2))
                          Communication.Proto.Constants.Debug = new Constants.DebugFunc((str) => { });
                      if (!Convert.ToBoolean(o.debugLevel & 4))
                      {
                          THUnity2D.GameObject.Debug = new Action<THUnity2D.GameObject, string>((gameObject, str) => { });
                          THUnity2D.GameObject.DebugWithoutEndline = new Action<THUnity2D.GameObject, string>((gameObject, str) => { });
                          THUnity2D.GameObject.DebugWithoutID = new Action<THUnity2D.GameObject, string>((gameObject, str) => { });
                          THUnity2D.GameObject.DebugWithoutIDEndline = new Action<THUnity2D.GameObject, string>((gameObject, str) => { });
                      }
                      InitializeMap();
                      server = new Server(o.serverPort, o.playerCount, o.agentCount, o.totalGameTimeSeconds);
                  });
        }
    }
}
