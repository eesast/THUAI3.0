using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Logic.Constant;
using static Logic.Constant.CONSTANT;
using System.Collections.Generic;
using static Map;
using Communication.Server;
using Communication.Proto;

namespace Logic.Server
{
    class Server
    {
        private const int serverPort = 8888;
        Dictionary<Tuple<int, int>, Player> playerList = new Dictionary<Tuple<int, int>, Player>();
        public ICommunication ServerCommunication = new CommunicationImpl();
        private static DateTime initTime = new DateTime();
        public static void InitializeTime() { initTime = DateTime.Now; }
        private static TimeSpan gameTime = new TimeSpan();
        public static void RefreshGameTime() { gameTime = DateTime.Now - initTime; }

        public Server()
        {
            ServerCommunication.Initialize();
            ServerCommunication.Port = serverPort;
            ServerCommunication.GameStart();

            for (int a = 0; a < Constants.AgentCount; a++)
            {
                for (int c = 0; c < Constants.PlayerCount; c++)
                {
                    playerList.Add(new Tuple<int, int>(a, c), new Player(new Tuple<int, int>(a, c), new Random().Next(2, WORLD_MAP_WIDTH - 2), new Random().Next(2, WORLD_MAP_HEIGHT - 2)));
                    ServerCommunication.SendMessage(new ServerMessage
                    {
                        Agent = a,
                        Client = c,
                        Message = new MessageToClient
                        {
                            PlayerIDAgent = a,
                            PlayerIDClient = c,
                            PlayerPositionX = BitConverter.DoubleToInt64Bits(playerList[new Tuple<int,int>(a,c)].xyPosition.x),
                            PlayerPositionY = BitConverter.DoubleToInt64Bits(playerList[new Tuple<int, int>(a, c)].xyPosition.y),
                            FacingDirection = (int)playerList[new Tuple<int, int>(a, c)].facingDirection,
                            IsAdd = false,
                            ObjType = 0,
                            ObjType2 = 0
                        }
                    }
                    );
                }
            }


            new Thread(Run).Start();

            Console.WriteLine("Server constructed");
        }

        public void Run()
        {
            InitializeTime();
            Console.WriteLine("Server begin to run");

            new Thread(ExecuteMessageQueue).Start();
            /*
            这里应该放定时、刷新物品等代码。
            */

            Console.WriteLine("Server stop running");
        }

        //ExecuteMessageQueue()函数控制消息队列
        public void ExecuteMessageQueue()
        {
            Console.WriteLine("Begin to execute message queue");
            while (true)
            {
                RefreshGameTime();
                Console.WriteLine("Time : " + gameTime.TotalSeconds.ToString() + "s");

                ServerMessage msg = ServerCommunication.MessageQueue.Take();
                if (!(msg.Message is MessageToServer)) throw new Exception("Recieve Error !");
                MessageToServer msgToSvr = msg.Message as MessageToServer;

                if (msgToSvr.CommandType < 0 || msgToSvr.CommandType >= (int)COMMAND_TYPE.SIZE)
                    continue;

                if (msgToSvr.CommandType == (int)COMMAND_TYPE.MOVE && msgToSvr.Parameter1 >= 0 && msgToSvr.Parameter1 < (int)Direction.Size)
                {
                    playerList[new Tuple<int, int>(msg.Agent, msg.Client)].Move((Direction)msgToSvr.Parameter1);
                }

            }

        }

    }

}