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
using System.Configuration;
using Timer;
namespace Logic.Server
{
    class Server
    {

        protected ICommunication ServerCommunication = new CommunicationImpl();

        public Server(ushort serverPort, ushort playerCount, ushort agentCount, uint MaxGameTimeSeconds)
        {
            Communication.Proto.Constants.ServerPort = serverPort;
            Communication.Proto.Constants.PlayerCount = playerCount;
            Communication.Proto.Constants.AgentCount = agentCount;
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
                    Program.PlayerList.Add(playerIDTuple, new Player(2.5, 1.5));//new Random().Next(2, WORLD_MAP_WIDTH - 2), new Random().Next(2, WORLD_MAP_HEIGHT - 2)));
                    Program.PlayerList[playerIDTuple].CommunicationID = playerIDTuple;
                    MessageToClient msg = new MessageToClient();
                    msg.GameObjectMessageList.Add(
                        Program.PlayerList[playerIDTuple].ID,
                        new GameObjectMessage
                        {
                            ObjType = ObjTypeMessage.People,
                            IsMoving = false,
                            Position = new XYPositionMessage { X = Program.PlayerList[playerIDTuple].Position.x, Y = Program.PlayerList[playerIDTuple].Position.y },
                            Direction = (DirectionMessage)(int)Program.PlayerList[playerIDTuple].facingDirection
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
            TaskSystem.RefreshTimer.Change(1000, (int)(Configs["TaskRefreshTime"]));

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
                char key = Console.ReadKey().KeyChar;
                switch (key)
                {
                    case '`': GodMode(); break;
                }

            }

            Console.WriteLine("Server stop running");
        }

        protected void GodMode()
        {
            Console.WriteLine("\n======= Welcome to God Mode ========\n");
            string[] words = Console.ReadLine().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (words.Length < 5)
                return;
            try
            {
                switch (words[0])
                {
                    case "Add":
                        switch (words[1])
                        {
                            case "Dish":
                                DishType dishtype = (DishType)int.Parse(words[2]);
                                if (dishtype >= DishType.Size2 || dishtype == DishType.Size1 || dishtype == DishType.Empty)
                                    return;
                                new Dish(double.Parse(words[3]), double.Parse(words[4]), dishtype).Parent = MapInfo.WorldMap;
                                break;
                            case "Tool":
                                ToolType tooltype = (ToolType)int.Parse(words[2]);
                                if (tooltype >= ToolType.Size || tooltype == ToolType.Empty)
                                    return;
                                new Tool(double.Parse(words[3]), double.Parse(words[4]), tooltype).Parent = MapInfo.WorldMap;
                                break;
                            case "Trigger":
                                TriggerType triggertype = (TriggerType)int.Parse(words[2]);
                                if (triggertype >= TriggerType.Size)
                                    return;
                                new Trigger(double.Parse(words[3]), double.Parse(words[4]), triggertype, -1).Parent = MapInfo.WorldMap;
                                break;
                        }
                        break;
                    case "Remove":
                        break;
                }
            }
            catch (FormatException)
            {
                ServerDebug("Format Error");
            }
        }

        protected void OnRecieve(Object communication, EventArgs e)
        {
            CommunicationImpl communicationImpl = communication as CommunicationImpl;
            MessageEventArgs messageEventArgs = e as MessageEventArgs;

            Console.WriteLine("GameTime : " + Time.GameTime().TotalSeconds.ToString("F3") + "s");
            Program.PlayerList[new Tuple<int, int>(messageEventArgs.message.Agent, messageEventArgs.message.Client)].ExecuteMessage(communicationImpl, (MessageToServer)((ServerMessage)messageEventArgs.message).Message);
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

        public static Action<string> ServerDebug = (str) => { Console.WriteLine(str); };
    }
}