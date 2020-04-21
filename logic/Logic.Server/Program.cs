using Communication.Proto;
using Communication.Server;
using Logic.Constant;
using System;
using System.Collections.Generic;
using static Logic.Constant.MapInfo;
using CommandLine;
using System.Collections.Concurrent;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Google.Protobuf;

namespace Logic.Server
{
    static class Program
    {
        private static Random? _random;
        public static Random Random
        {
            get
            {
                _random = _random ?? new Random(DateTime.Now.Millisecond);
                return _random;
            }
        }

        private static MessageToClient? _messageToClient;
        public static MessageToClient MessageToClient
        {
            get
            {
                if (_messageToClient == null)
                    _messageToClient = new MessageToClient();
                return _messageToClient;
            }
        }
        public static readonly object MessageToClientLock = new object();

        private static ServerMessage? _serverMessage;
        public static ServerMessage ServerMessage
        {
            get
            {
                if (_serverMessage == null)
                    _serverMessage = new ServerMessage { Agent = -2, Client = -2 };
                return _serverMessage;
            }
        }

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

        public static ConcurrentDictionary<Tuple<int, int>, Player> PlayerList = new ConcurrentDictionary<Tuple<int, int>, Player>();
        public static ConcurrentDictionary<int, object> ScoreLocks = new ConcurrentDictionary<int, object>();
        private static Server server;
        public static void Main(string[] args)
        {

            //PlayBack.Reader reader = new PlayBack.Reader("test.bin");
            //IMessage<MessageToClient> message = new MessageToClient();
            //Console.WriteLine(reader.Read(ref message));
            //Console.WriteLine("=============");
            //foreach (var item in ((MessageToClient)message).Scores)
            //    Console.WriteLine(item.Key + " , " + item.Value);
            //foreach (var item in ((MessageToClient)message).GameObjectList)
            //    Console.WriteLine(item.Key + " , " + item.Value.ObjType);
            //message = new MessageToClient();
            //Console.WriteLine(reader.Read(ref message));
            //Console.WriteLine("=============");
            //foreach (var item in ((MessageToClient)message).Scores)
            //    Console.WriteLine(item.Key + " , " + item.Value);
            //foreach (var item in ((MessageToClient)message).GameObjectList)
            //    Console.WriteLine(item.Key + " , " + item.Value.ObjType);
            //Console.WriteLine(reader.Read(ref message));
            //Console.WriteLine("=============");
            //foreach (var item in ((MessageToClient)message).Scores)
            //    Console.WriteLine(item.Key + " , " + item.Value);
            //foreach (var item in ((MessageToClient)message).GameObjectList)
            //    Console.WriteLine(item.Key + " , " + item.Value.ObjType);

            Parser.Default.ParseArguments<AugmentOptions>(args)
                  .WithParsed<AugmentOptions>(o =>
                  {
                      if (!Convert.ToBoolean(o.debugLevel & 1))
                          Server.ServerDebug = new Action<string>(s => { });
                      if (!Convert.ToBoolean(o.debugLevel & 2))
                          Communication.Proto.Constants.Debug = new Constants.DebugFunc((str) => { });
                      if (!Convert.ToBoolean(o.debugLevel & 4))
                      {
                          THUnity2D.GameObject.Debug = new Action<THUnity2D.GameObject, string>((gameObject, str) => { });
                          THUnity2D.GameObject.DebugWithoutEndline = new Action<THUnity2D.GameObject, string>((gameObject, str) => { });
                          THUnity2D.GameObject.DebugWithoutID = new Action<THUnity2D.GameObject, string>((gameObject, str) => { });
                          THUnity2D.GameObject.DebugWithoutIDEndline = new Action<THUnity2D.GameObject, string>((gameObject, str) => { });
                      }
                      if (o.playerCount < 1)
                          o.playerCount = 1;
                      else if (o.playerCount > 2)
                          o.playerCount = 2;
                      if (o.agentCount < 1)
                          o.agentCount = 1;
                      else if (o.agentCount > 4)
                          o.agentCount = 4;
                      if (o.totalGameTimeSeconds < 10)
                          o.totalGameTimeSeconds = 10;
                      else if (o.totalGameTimeSeconds > 3600)
                          o.totalGameTimeSeconds = 3600;
                      InitializeMap();
                      server = new Server(o.serverPort, o.playerCount, o.agentCount, o.totalGameTimeSeconds, o.token);
                  });
        }
    }
}
