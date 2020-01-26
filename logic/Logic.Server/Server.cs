using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Logic.Constant;
using static Logic.Constant.Constant;
using System.Collections.Generic;
using static Logic.Constant.Map;
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
        private static TimeSpan getGameTime()
        {
            return DateTime.Now - initTime;
        }

        public Server()
        {
            ServerCommunication.Initialize();
            ServerCommunication.MsgProcess += OnRecieve;
            //ServerCommunication.Port = serverPort;
            ServerCommunication.GameStart();

            for (int a = 0; a < Constants.AgentCount; a++)
            {
                for (int c = 0; c < Constants.PlayerCount; c++)
                {
                    playerList.Add(new Tuple<int, int>(a, c), new Player(2.5, 1.5));//new Random().Next(2, WORLD_MAP_WIDTH - 2), new Random().Next(2, WORLD_MAP_HEIGHT - 2)));
                    playerList[new Tuple<int, int>(a, c)].Parent = WorldMap;
                    MessageToClient msg = new MessageToClient();
                    msg.GameObjectMessageList.Add(playerList[new Tuple<int, int>(a, c)].ID, new GameObjectMessage
                    {
                        Type = ObjectTypeMessage.People,
                        Position = new XYPositionMessage { X = playerList[new Tuple<int, int>(a, c)].Position.x, Y = playerList[new Tuple<int, int>(a, c)].Position.y },
                        Direction = (DirectionMessage)(int)playerList[new Tuple<int, int>(a, c)].facingDirection
                    });
                    ServerCommunication.SendMessage(new ServerMessage
                    {
                        Agent = a,
                        Client = c,
                        Message = msg
                    }
                    );
                }
            }
            foreach (var item in playerList)
            {
                MessageToClient msg = new MessageToClient();
                msg.GameObjectMessageList.Add(item.Value.ID, new GameObjectMessage
                {
                    Type = ObjectTypeMessage.People,
                    Position = new XYPositionMessage { X = item.Value.Position.x, Y = item.Value.Position.y },
                    Direction = (DirectionMessage)(int)item.Value.facingDirection
                }); ;
                ServerCommunication.SendMessage(new ServerMessage
                {
                    Agent = -2,
                    Client = -2,
                    Message = msg
                }
                );
            }
            new Thread(Run).Start();
            Console.WriteLine("Server constructed");
        }

        public void Run()
        {
            InitializeTime();
            Console.WriteLine("Server begin to run");

            //new Thread(ExecuteMessageQueue).Start();
            /*
            这里应该放定时、刷新物品等代码。
            */
            while(true)
            {
                Console.ReadKey();
            }

            Console.WriteLine("Server stop running");
        }

        public void OnRecieve(Object communication, EventArgs e)
        {
            CommunicationImpl communicationImpl = communication as CommunicationImpl;
            MessageEventArgs messageEventArgs = e as MessageEventArgs;
            //ServerMessage message = ev.message;
            //ChatMessage chat = message.Message as ChatMessage;
            Console.WriteLine("Time : " + getGameTime().TotalSeconds.ToString("F3") + "s");
            playerList[new Tuple<int, int>(messageEventArgs.message.Agent, messageEventArgs.message.Client)].ExecuteMessage(communicationImpl, (MessageToServer)((ServerMessage)messageEventArgs.message).Message);
            SendAll();
        }

        public void SendAll()
        {
            ;
        }

        ////ExecuteMessageQueue()函数控制消息队列
        //public void ExecuteMessageQueue()
        //{
        //    Console.WriteLine("Begin to execute message queue");
        //    while (true)
        //    {
        //        Console.WriteLine("Time : " + getGameTime().TotalSeconds.ToString("F3") + "s");

        //        ServerMessage msg = ServerCommunication.MessageQueue.Take();
        //        if (!(msg.Message is MessageToServer)) throw new Exception("Recieve Error !");
        //        MessageToServer msgToSvr = msg.Message as MessageToServer;

        //        if (msgToSvr.CommandType < 0 || msgToSvr.CommandType >= (int)COMMAND_TYPE.SIZE)
        //            continue;
        //        if (msgToSvr.CommandType == (int)COMMAND_TYPE.MOVE && msgToSvr.Parameter1 >= 0 && msgToSvr.Parameter1 < (int)Direction.Size)
        //        {
        //            playerList[new Tuple<int, int>(msg.Agent, msg.Client)].Move((Direction)msgToSvr.Parameter1);
        //        }
        //    }
        //}
    }
}