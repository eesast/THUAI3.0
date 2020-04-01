using Communication.Proto;
using Communication.Server;
using Logic.Constant;
using System;
using System.Threading;
using Timer;
using THUnity2D;
using static Logic.Constant.Constant;
using static Logic.Constant.MapInfo;
namespace Logic.Server
{
    class Server
    {
        protected ICommunication ServerCommunication;
        protected uint MaxRunTimeInSecond;
        System.Threading.Timer SendMessageTimer = null;
        System.Threading.Timer ToolRefreshTimer = null;
        System.Threading.Timer ServerStopTimer = null;
        System.Threading.Timer WatchInputTimer = null;
        Thread ServerRunThread = null;

        public Server(ushort serverPort, ushort playerCount, ushort agentCount, uint MaxGameTimeSeconds, string token)
        {
            Communication.Proto.Constants.PlayerCount = playerCount;
            Communication.Proto.Constants.AgentCount = agentCount;
            MaxRunTimeInSecond = MaxGameTimeSeconds;
            ServerCommunication = new CommunicationImpl
            {
                ServerPort = serverPort,
                Token = token
            };
            ServerCommunication.Initialize();
            ServerCommunication.MsgProcess += OnRecieve;
            ServerCommunication.GameStart();

            //初始化playerList
            //向所有Client发送他们自己的ID
            for (int a = 0; a < Constants.AgentCount; a++)
            {
                Program.MessageToClient.Scores.Add(a, 0);
                for (int c = 0; c < Constants.PlayerCount; c++)
                {
                    Tuple<int, int> playerIDTuple = new Tuple<int, int>(a, c);
                    Program.PlayerList.TryAdd(playerIDTuple, new Player(2.5, 1.5));//new Random().Next(2, WORLD_MAP_WIDTH - 2), new Random().Next(2, WORLD_MAP_HEIGHT - 2)));
                    Program.PlayerList[playerIDTuple].CommunicationID = playerIDTuple;
                    MessageToClient msg = new MessageToClient();
                    msg.GameObjectList.Add(
                        Program.PlayerList[playerIDTuple].ID,
                        new Communication.Proto.GameObject
                        {
                            ObjType = ObjType.People,
                            IsMoving = false,
                            Team = playerIDTuple.Item1,
                            PositionX = Program.PlayerList[playerIDTuple].Position.x,
                            PositionY = Program.PlayerList[playerIDTuple].Position.y,
                            Direction = (Communication.Proto.Direction)Program.PlayerList[playerIDTuple].FacingDirection
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

            ServerRunThread = new Thread(Run);
            ServerRunThread.Start();
            Server.ServerDebug("Server constructed");
        }

        public void Run()
        {
            Time.InitializeTime();
            Server.ServerDebug("Server begin to run");
            TaskSystem.RefreshTimer.Change(1000, (int)Configs["TaskRefreshTime"]);
            ToolRefreshTimer = new System.Threading.Timer(ToolRefresh, null,
                0, (int)Configs["ToolRefreshTime"]);

            SendMessageTimer = new System.Threading.Timer(
                (o) =>
                {
                    SendMessageToAllClient();
                }, null, TimeSpan.FromSeconds(TimeInterval), TimeSpan.FromSeconds(TimeInterval));

            WatchInputTimer = new System.Threading.Timer(WatchInput, null, 0, 0);

            Thread.Sleep((int)MaxRunTimeInSecond * 1000);
            PrintScore();
            Server.ServerDebug("Server stop running");
        }

        void ToolRefresh(object o)
        {
            THUnity2D.XYPosition tempPosition = null;
            for (int i = 0; i < 10; i++)//加入次数限制，防止后期地图过满疯狂Random
            {
                tempPosition = new THUnity2D.XYPosition(Program.Random.Next(1, map.GetLength(0) - 1), Program.Random.Next(1, map.GetLength(1) - 1));
                if (WorldMap.Grid[(int)tempPosition.x, (int)tempPosition.y].IsEmpty())
                    break;
            }
            new Tool(tempPosition.x + 0.5, tempPosition.y + 0.5, (ToolType)Program.Random.Next(1, (int)ToolType.ToolSize - 1)).Parent = WorldMap;
        }

        protected void PrintScore()
        {
            Console.WriteLine("============= Score ===========");
            for (int i = 0; i < Communication.Proto.Constants.AgentCount; i++)
            {
                Console.WriteLine("Team " + i + " : " + Program.MessageToClient.Scores[i]);
            }
            Console.WriteLine("===============================");
        }

        protected void WatchInput(object o)
        {
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
        }

        protected void GodMode()
        {
            Server.ServerDebug("\n======= Welcome to God Mode ========\n");
            string[] words = Console.ReadLine().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            try
            {
                switch (words[0])
                {
                    case "Add":
                        switch (words[1])
                        {
                            case "Dish":
                                DishType dishtype = (DishType)int.Parse(words[2]);
                                if (dishtype >= DishType.DishSize2 || dishtype == DishType.DishSize1 || dishtype == DishType.DishEmpty)
                                    return;
                                new Dish(double.Parse(words[3]), double.Parse(words[4]), dishtype).Parent = MapInfo.WorldMap;
                                break;
                            case "Tool":
                                ToolType tooltype = (ToolType)int.Parse(words[2]);
                                if (tooltype >= ToolType.ToolSize || tooltype == ToolType.ToolEmpty)
                                    return;
                                new Tool(double.Parse(words[3]), double.Parse(words[4]), tooltype).Parent = MapInfo.WorldMap;
                                break;
                            case "Trigger":
                                TriggerType triggertype = (TriggerType)int.Parse(words[2]);
                                if (triggertype >= TriggerType.TriggerSize)
                                    return;
                                new Trigger(double.Parse(words[3]), double.Parse(words[4]), triggertype, -1, Talent.None).Parent = MapInfo.WorldMap;
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
            catch (IndexOutOfRangeException)
            {
                ServerDebug("Augment number incorrect");
            }
        }

        protected void OnRecieve(Object communication, EventArgs e)
        {
            CommunicationImpl communicationImpl = communication as CommunicationImpl;
            MessageEventArgs messageEventArgs = e as MessageEventArgs;
            Tuple<int, int> playerCommunitionID = new Tuple<int, int>(messageEventArgs.message.Agent, messageEventArgs.message.Client);
            if (((MessageToServer)messageEventArgs.message.Message).IsSetTalent)
            {
                while (!Program.PlayerList.ContainsKey(playerCommunitionID))
                {
                    Thread.Sleep(100);
                }
                if (((MessageToServer)messageEventArgs.message.Message).Talent < Talent.None || ((MessageToServer)messageEventArgs.message.Message).Talent >= Talent.Size)
                {
                    ((MessageToServer)messageEventArgs.message.Message).Talent = Talent.None;
                }
                Program.PlayerList[playerCommunitionID].Talent = ((MessageToServer)messageEventArgs.message.Message).Talent;
                Server.ServerDebug("Player " + playerCommunitionID.Item1 + "." + playerCommunitionID.Item2 + " has chose talent " + Program.PlayerList[playerCommunitionID].Talent);
                return;
            }

            //Server.ServerDebug("GameTime : " + Time.GameTime().TotalSeconds.ToString("F3") + "s");
            Program.PlayerList[playerCommunitionID].ExecuteMessage(communicationImpl, (MessageToServer)messageEventArgs.message.Message);
        }

        //向所有Client发送消息，按照帧率定时发送，严禁在其他地方调用此函数
        protected void SendMessageToAllClient()
        {
            //for (int i = 0; i < 20; i++)
            //{
            //    Console.Write("Send");
            //    for (int k = 0; k < i; k++)
            //        Console.Write(" ");
            //    Console.WriteLine(i);
            //}
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