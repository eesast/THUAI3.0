using System;
using System.Collections.Generic;
using System.Text;
using Communication.Proto;
using Logic.Constant;
using static Logic.Constant.MapInfo;

namespace Logic.Server
{
    static class Program
    {
        private static Random _random;
        public static Random Random
        {
            get
            {
                _random = _random ?? new Random();
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
                        case 1: new Block(x + 0.5, y + 0.5, BlockType.TaskPoint).Parent = WorldMap; break;
                        case 2: new Block(x + 0.5, y + 0.5, BlockType.FoodPoint).Parent = WorldMap; break;
                        case 3: new Block(x + 0.5, y + 0.5, BlockType.Cooker).Parent = WorldMap; break;
                        case 4: new Block(x + 0.5, y + 0.5, BlockType.RubbishBin).Parent = WorldMap; break;
                        case 5: new Block(x + 0.5, y + 0.5, BlockType.Wall).Parent = WorldMap; break;
                        case 6: new Block(x + 0.5, y + 0.5, BlockType.Table).Parent = WorldMap; break;
                    }
                }
            }
            //new Tool(1.5, 1.5, ToolType.SpeedBuff).Parent = WorldMap;
        }

        public static Dictionary<Tuple<int, int>, Player> PlayerList = new Dictionary<Tuple<int, int>, Player>();
        private static Server server;
        public static void Main(string[] args)
        {
            Communication.Proto.Constants.Debug = new Constants.DebugFunc((str) => { });
            THUnity2D.GameObject.Debug = new Action<THUnity2D.GameObject, string>((gameObject, str) => { });
            THUnity2D.GameObject.DebugWithoutEndline = new Action<THUnity2D.GameObject, string>((gameObject, str) => { });
            THUnity2D.GameObject.DebugWithoutID = new Action<THUnity2D.GameObject, string>((gameObject, str) => { });
            THUnity2D.GameObject.DebugWithoutIDEndline = new Action<THUnity2D.GameObject, string>((gameObject, str) => { });
            InitializeMap();

            ushort serverPort = 0, playerCount = 0, agentCount = 0;
            uint maxGameTimeSecond = 1000;
            try
            {
                serverPort = ushort.Parse(args[0]);
                playerCount = ushort.Parse(args[1]);
                agentCount = ushort.Parse(args[2]);
                maxGameTimeSecond = uint.Parse(args[3]);
            }
            catch (FormatException)
            {
                Server.ServerDebug("Format Error");
            }
            catch (IndexOutOfRangeException)
            {
                Server.ServerDebug("Arguments Number Error");
            }
            server = new Server(serverPort, playerCount, agentCount, maxGameTimeSecond);
        }
    }
}
