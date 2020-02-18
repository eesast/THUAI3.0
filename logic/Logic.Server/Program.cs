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
            new Block(1.5, 1.5, BlockType.Cooker).Parent = WorldMap;
            new Block(1.5, 2.5, BlockType.TaskPoint).Parent = WorldMap;
            new Dish(1.5, 1.5, DishType.Apple).Parent = WorldMap;
            new Dish(1.5, 1.5, DishType.Potato).Parent = WorldMap;

        }

        private static Server server;
        public static void Main(string[] args)
        {
            Communication.Proto.Constants.Debug = new Constants.DebugFunc((str) => { });
            THUnity2D.GameObject.Debug = new Action<THUnity2D.GameObject, string>((gameObject, str) => { });
            THUnity2D.GameObject.DebugWithoutEndline = new Action<THUnity2D.GameObject, string>((gameObject, str) => { });
            THUnity2D.GameObject.DebugWithoutID = new Action<THUnity2D.GameObject, string>((gameObject, str) => { });
            THUnity2D.GameObject.DebugWithoutIDEndline = new Action<THUnity2D.GameObject, string>((gameObject, str) => { });
            InitializeMap();
            server = new Server();
        }
    }
}
