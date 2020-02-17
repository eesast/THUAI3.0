using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Logic.Constant;
using static Logic.Constant.Constant;
using System.Collections.Generic;
using Communication.Server;
using Communication.Proto;
using Timer;
namespace Logic.Server
{
    class Server
    {
        protected Dictionary<Tuple<int, int>, Player> PlayerList = new Dictionary<Tuple<int, int>, Player>();
        public ICommunication ServerCommunication = new CommunicationImpl();

        public Server()
        {
            ServerCommunication.Initialize();
            ServerCommunication.MsgProcess += OnRecieve;
            ServerCommunication.GameStart();

            //初始化playerList
            //向所有Client发送他们自己的ID
            for (int a = 0; a < Constants.AgentCount; a++)
            {
                for (int c = 0; c < Constants.PlayerCount; c++)
                {
                    Tuple<int, int> playerIDTuple = new Tuple<int, int>(a, c);
                    PlayerList.Add(playerIDTuple, new Player(2.5, 1.5));//new Random().Next(2, WORLD_MAP_WIDTH - 2), new Random().Next(2, WORLD_MAP_HEIGHT - 2)));
                    PlayerList[playerIDTuple].team = a;
                    MessageToClient msg = new MessageToClient();
                    msg.GameObjectMessageList.Add(
                        PlayerList[playerIDTuple].ID,
                        new GameObjectMessage
                        {
                            ObjType = ObjTypeMessage.People,
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

            System.Threading.Timer timer = new System.Threading.Timer(
                (o) =>
                {
                    SendMessageToAllClient();
                },
                new object(),
                TimeSpan.FromSeconds(TimeInterval),
                TimeSpan.FromSeconds(TimeInterval));

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

            Console.WriteLine("GameTime : " + Time.GameTime().TotalSeconds.ToString("F3") + "s");
            PlayerList[new Tuple<int, int>(messageEventArgs.message.Agent, messageEventArgs.message.Client)].ExecuteMessage(communicationImpl, (MessageToServer)((ServerMessage)messageEventArgs.message).Message);
            //SendMessageToAllClient();
        }

        //向所有Client发送消息，按照帧率定时发送，严禁在其他地方调用此函数
        protected void SendMessageToAllClient()
        {
            lock (Program.MessageToClientLock)
            {
                ServerCommunication.SendMessage(new ServerMessage
                {
                    Agent = -2,
                    Client = -2,
                    Message = Program.MessageToClient
                });
            }
        }
    }
}