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
using Timer;
namespace Logic.Server
{
    class Server
    {
        //private const int serverPort = 8888;
        protected Dictionary<Tuple<int, int>, Player> PlayerList = new Dictionary<Tuple<int, int>, Player>();
        public ICommunication ServerCommunication = new CommunicationImpl();

        public MessageToClient MessageToClient { get; } = new MessageToClient();

        public Server()
        {
            ServerCommunication.Initialize();
            ServerCommunication.MsgProcess += OnRecieve;
            //ServerCommunication.Port = serverPort;
            ServerCommunication.GameStart();

            //初始化playerList
            //向所有Client发送他们自己的ID
            for (int a = 0; a < Constants.AgentCount; a++)
            {
                for (int c = 0; c < Constants.PlayerCount; c++)
                {
                    Tuple<int, int> playerIDTuple = new Tuple<int, int>(a, c);
                    PlayerList.Add(playerIDTuple, new Player(2.5, 1.5));//new Random().Next(2, WORLD_MAP_WIDTH - 2), new Random().Next(2, WORLD_MAP_HEIGHT - 2)));
                    MessageToClient.GameObjectMessageList.Add(
                        PlayerList[playerIDTuple].ID,
                        new GameObjectMessage
                        {
                            Type = ObjectTypeMessage.People,
                            IsMoving = false,
                            Position = new XYPositionMessage { X = PlayerList[playerIDTuple].Position.x, Y = PlayerList[playerIDTuple].Position.y },
                            Direction = (DirectionMessage)(int)PlayerList[playerIDTuple].facingDirection
                        });

                    MessageToClient msg = new MessageToClient();
                    msg.GameObjectMessageList.Add(
                        PlayerList[playerIDTuple].ID,
                        new GameObjectMessage
                        {
                            Type = ObjectTypeMessage.People,
                            IsMoving = false,
                            Position = new XYPositionMessage { X = PlayerList[playerIDTuple].Position.x, Y = PlayerList[playerIDTuple].Position.y },
                            Direction = (DirectionMessage)(int)PlayerList[playerIDTuple].facingDirection
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

            SendMessageToAllClient();

            new Thread(Run).Start();
            Console.WriteLine("Server constructed");
        }

        public void Run()
        {
            Time.InitializeTime();
            Console.WriteLine("Server begin to run");

            while (true)
            {
                /*
                这里应该放定时、刷新物品等代码。
                */
                Console.ReadKey();
            }

            Console.WriteLine("Server stop running");
        }

        public void OnRecieve(Object communication, EventArgs e)
        {
            CommunicationImpl communicationImpl = communication as CommunicationImpl;
            MessageEventArgs messageEventArgs = e as MessageEventArgs;

            Console.WriteLine("Time : " + Time.GameTime().TotalSeconds.ToString("F3") + "s");
            PlayerList[new Tuple<int, int>(messageEventArgs.message.Agent, messageEventArgs.message.Client)].ExecuteMessage(communicationImpl, (MessageToServer)((ServerMessage)messageEventArgs.message).Message);
            SendMessageToAllClient();
        }

        protected void SendMessageToAllClient()
        {
            ServerCommunication.SendMessage(new ServerMessage
            {
                Agent = -2,
                Client = -2,
                Message = MessageToClient
            });
        }
    }
}