using Communication.Proto;
using Communication.Server;
using Logic.Constant;
using System;
using System.Threading;
using Timer;
using THUnity2D;
using static Logic.Constant.Constant;
using static Logic.Constant.MapInfo;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Net;
using System.Text;

namespace Logic.Server
{
    class Server
    {
        protected ICommunication ServerCommunication;
        protected uint MaxRunTimeInSecond;
        System.Threading.Timer? SendMessageTimer;
        System.Threading.Timer? ToolRefreshTimer;
        //System.Threading.Timer? ServerStopTimer;
        System.Threading.Timer? WatchInputTimer;
        Thread ServerRunThread;
        PlayBack.Writer writer = new PlayBack.Writer("server.playback");

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
            XYPosition[] bornPoints = { new XYPosition(2.5, 1.5), new XYPosition(48.5, 2.5), new XYPosition(48.5, 48.5), new XYPosition(2.5, 48.5) };
            for (int a = 0; a < Constants.AgentCount; a++)
            {
                Program.MessageToClient.Scores.Add(a, 0);
                Program.ScoreLocks.TryAdd(a, new object());
                for (int c = 0; c < Constants.PlayerCount; c++)
                {
                    Tuple<int, int> playerIDTuple = new Tuple<int, int>(a, c);
                    Program.PlayerList.TryAdd(playerIDTuple, new Player(bornPoints[a].x, bornPoints[a].y));//new Random().Next(2, WORLD_MAP_WIDTH - 2), new Random().Next(2, WORLD_MAP_HEIGHT - 2)));
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
            TaskSystem.RefreshTimer.Change(1000, (int)Configs("TaskRefreshTime"));
            ToolRefreshTimer = new System.Threading.Timer(ToolRefresh, null,
                0, (int)Configs("ToolRefreshTime"));

            new System.Threading.Thread(
                () =>
                {
                    while (true)
                    {
                        int begin = Environment.TickCount;
                        SendMessageToAllClient();
                        int end = Environment.TickCount;
                        int delta = (int)TimeIntervalInMillisecond - (end - begin);
                        if (delta > 0)
                            Thread.Sleep(delta);
                    }
                }).Start();//发送消息

            WatchInputTimer = new System.Threading.Timer(WatchInput, null, 0, 0);

            Thread.Sleep((int)MaxRunTimeInSecond * 1000);
            PrintScore();
            SaveScore();
            SendHttpRequest($"https://api.eesast.com/v1/teams/scores", ServerCommunication.Token, "PUT", new JObject
            {
                ["scores"] = new JArray(Program.MessageToClient.Scores.Values)
            });
            ServerCommunication.GameOver();
            Server.ServerDebug("Server stop running");
        }

        void ToolRefresh(object? o)
        {
            THUnity2D.XYPosition tempPosition = new THUnity2D.XYPosition(Program.Random.Next(1, map.GetLength(0) - 1), Program.Random.Next(1, map.GetLength(1) - 1)); ;
            for (int i = 0; i < 10 && !WorldMap.Grid[(int)tempPosition.x, (int)tempPosition.y].IsEmpty(); i++)//加入次数限制，防止后期地图过满疯狂Random
            {
                tempPosition = new THUnity2D.XYPosition(Program.Random.Next(1, map.GetLength(0) - 1), Program.Random.Next(1, map.GetLength(1) - 1));
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

        protected void SaveScore()
        {
            JToken scores = new JObject();
            foreach (var item in Program.MessageToClient.Scores)
            {
                scores[item.Key.ToString()] = item.Value;
            }
            System.IO.File.WriteAllText("scores.json", Newtonsoft.Json.JsonConvert.SerializeObject(scores));
        }

        protected void WatchInput(object? o)
        {
            try
            {
                while (true)
                {
                    char key = Console.ReadKey().KeyChar;
                    switch (key)
                    {
                        case '`': GodMode(); break;
                    }

                }
            }
            catch (InvalidOperationException)
            { }
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
                    case "Move":
                        switch (words[1])
                        {
                            case "Dish":
                                var dish = WorldMap.Grid[int.Parse(words[2]), int.Parse(words[3])].GetFirstObject(typeof(Dish));
                                if (dish != null)
                                    dish.Position = new XYPosition(double.Parse(words[4]), double.Parse(words[5]));
                                break;
                            case "Tool":
                                var tool = WorldMap.Grid[int.Parse(words[2]), int.Parse(words[3])].GetFirstObject(typeof(Tool));
                                if (tool != null)
                                    tool.Position = new XYPosition(double.Parse(words[4]), double.Parse(words[5]));
                                break;
                            case "Trigger":
                                var trigger = WorldMap.Grid[int.Parse(words[2]), int.Parse(words[3])].GetFirstObject(typeof(Trigger));
                                if (trigger != null)
                                    trigger.Position = new XYPosition(double.Parse(words[4]), double.Parse(words[5]));
                                break;
                            case "Player":
                                var player = WorldMap.Grid[int.Parse(words[2]), int.Parse(words[3])].GetFirstObject(typeof(Player));
                                if (player != null)
                                    player.Position = new XYPosition(double.Parse(words[4]), double.Parse(words[5]));
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
            //CommunicationImpl communicationImpl = communication as CommunicationImpl;
            MessageToServer msg2svr = (MessageToServer)((MessageEventArgs)e).message.Message;
            Tuple<int, int> playerCommunitionID = new Tuple<int, int>(((MessageEventArgs)e).message.Agent, ((MessageEventArgs)e).message.Client);
            if (msg2svr.IsSetTalent)
            {
                while (!Program.PlayerList.ContainsKey(playerCommunitionID))
                {
                    Thread.Sleep(20);
                }
                if (msg2svr.Talent < Talent.None || msg2svr.Talent >= Talent.Size)
                {
                    msg2svr.Talent = Talent.None;
                }
                Program.PlayerList[playerCommunitionID].Talent = msg2svr.Talent;
                Server.ServerDebug("Player " + playerCommunitionID.Item1 + "." + playerCommunitionID.Item2 + " has chose talent " + Program.PlayerList[playerCommunitionID].Talent);
                return;
            }

            //Server.ServerDebug("GameTime : " + Time.GameTime().TotalSeconds.ToString("F3") + "s");
            Program.PlayerList[playerCommunitionID].ExecuteMessage(msg2svr);
        }

        //向所有Client发送消息，按照帧率定时发送，严禁在其他地方调用此函数
        protected void SendMessageToAllClient()
        {
            lock (Program.MessageToClientLock)
            {
                //Console.Write("S;");
                Program.ServerMessage.Message = Program.MessageToClient;
                ServerCommunication.SendMessage(Program.ServerMessage);
                writer.Write(Program.MessageToClient);
            }
        }
        public static Action<string> ServerDebug = (str) => { Console.WriteLine(str); };

        protected void SendHttpRequest(string url, string token, string method, JObject data)
        {
            if (string.IsNullOrEmpty(token)) return;
            try
            {
                var request = WebRequest.CreateHttp(url);
                request.Method = method;
                request.Headers.Add("Authorization", $"bearer {token}");
                if (data != null)
                {
                    request.ContentType = "application/json";
                    var raw = Encoding.UTF8.GetBytes(data.ToString());
                    request.GetRequestStream().Write(raw, 0, raw.Length);
                    request.GetResponse();
                }
            }
            catch (Exception e)
            {
                Server.ServerDebug(e.ToString());
            }
        }
    }
}