using System;
using System.Collections.Generic;
using System.Text;
using Communication.Proto;
using Logic.Constant;
using static THUnity2D._Map;

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


        // Start is called before the first frame update
        public static void InitializeMap()
        {
            for (uint x = 0; x < WorldMap.Width; x++)
            {
                for (uint y = 0; y < WorldMap.Height; y++)
                {
                    if (map[x, y] == 6)
                    {
                        new Block(x + 0.5, y + 0.5, BlockType.FoodPoint).Parent = WorldMap;
                    }
                    else if (map[x, y] == 5)
                    {
                        new Block(x + 0.5, y + 0.5, BlockType.Wall).Parent = WorldMap;
                    }
                    else if (map[x, y] == 0)
                    {
                        //new Trigger(x + 0.5, y + 0.5, TriggerType.Trap, -1).Parent = WorldMap;
                    }
                    else
                    {
                        new Block(x + 0.5, y + 0.5, BlockType.Wall).Parent = WorldMap;
                    }
                }
            }
            new Tool(1.5, 1.5, ToolType.SpeedBuff).Parent = WorldMap;

        }

        private static Server server;
        public static void Main(string[] args)
        {
            InitializeMap();
            server = new Server();
        }
    }
}
