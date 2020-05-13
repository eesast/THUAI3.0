using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Google.Protobuf;
using Logic.Constant;
using Communication.Proto;
using static Logic.Constant.Constant;
using static Logic.Constant.MapInfo;
using THUnity2D;
using static THUnity2D.Tools;
using GameForm;
using System.Collections.Generic;
using Communication;
namespace Client
{
    class Player : Character
    {
        protected Int64 id = -1;//注意！！！在这个类里基类的ID已被弃用
        protected int team = 0;
        protected static DateTime lastSendTime = new DateTime();
        protected Communication.CAPI.API ClientCommunication = new Communication.CAPI.API();
        protected MessageToServer messageToServer = new MessageToServer();
        protected Talent playerTalent;
        protected bool isStartedGame = false;
        protected MessageToClient lastRecieveMessage = new MessageToClient();
        protected PlayBack.Reader Reader;
        protected System.Threading.Timer playBackTimer;
        protected bool isPlayBack;

        public void ChangeControlLabelText(string id, string str)
        {
            if (Program.form.ControlLabels[id].InvokeRequired)
            {
                Program.form.ControlLabels[id].Invoke(new Action(() => { Program.form.ControlLabels[id].Text = id + "  " + str; }));
            }
            else
            {
                Program.form.ControlLabels[id].Text = id + "  " + str;
            }
        }
        public void RefreshFormLabelMethod(Int64 id_t, Communication.Proto.GameObject gameObjectMessage)
        {
            switch (gameObjectMessage.ObjType)
            {
                case ObjType.People:
                    Program.form.playerLabels[id_t].Location =
                        new System.Drawing.Point(
                            (int)((gameObjectMessage.PositionX - 0.5) * GameForm.Form1.LABEL_WIDTH + Form1.HALF_LABEL_INTERVAL),
                            Convert.ToInt32((WorldMap.Height - gameObjectMessage.PositionY - 0.5) * GameForm.Form1.LABEL_WIDTH + Form1.HALF_LABEL_INTERVAL));
                    switch ((THUnity2D.Direction)gameObjectMessage.Direction)
                    {
                        case THUnity2D.Direction.Right: Program.form.playerLabels[id_t].Text = "→"; break;
                        case THUnity2D.Direction.RightUp: Program.form.playerLabels[id_t].Text = "↗"; break;
                        case THUnity2D.Direction.Up: Program.form.playerLabels[id_t].Text = "↑"; break;
                        case THUnity2D.Direction.LeftUp: Program.form.playerLabels[id_t].Text = "↖"; break;
                        case THUnity2D.Direction.Left: Program.form.playerLabels[id_t].Text = "←"; break;
                        case THUnity2D.Direction.LeftDown: Program.form.playerLabels[id_t].Text = "↙"; break;
                        case THUnity2D.Direction.Down: Program.form.playerLabels[id_t].Text = "↓"; break;
                        case THUnity2D.Direction.RightDown: Program.form.playerLabels[id_t].Text = "↘"; break;
                        default: break;
                    }
                    if (id_t == this.id)
                    {
                        if (gameObjectMessage.DishType != DishType.DishEmpty)
                            ChangeControlLabelText("Dish", gameObjectMessage.DishType.ToString());
                        else
                            ChangeControlLabelText("Dish", "");
                        if (gameObjectMessage.ToolType != ToolType.ToolEmpty
                            && gameObjectMessage.ToolType != ToolType.ToolSize)
                            ChangeControlLabelText("Tool", gameObjectMessage.ToolType.ToString());
                        else
                            ChangeControlLabelText("Tool", "");
                        ChangeControlLabelText("RecieveText", gameObjectMessage.RecieveText);
                    }
                    break;
                case ObjType.Block:
                    switch (gameObjectMessage.BlockType)
                    {
                        case BlockType.FoodPoint:
                            if (gameObjectMessage.DishType == DishType.DishEmpty)
                                Program.form.playerLabels[id_t].Text = "";
                            else
                                Program.form.playerLabels[id_t].Text = gameObjectMessage.DishType.ToString();
                            break;
                        case BlockType.Cooker:
                            if (gameObjectMessage.DishType == DishType.DishEmpty)
                                Program.form.playerLabels[id_t].Text = "";
                            else
                                Program.form.playerLabels[id_t].Text = gameObjectMessage.DishType.ToString();
                            break;
                    }
                    break;
                case ObjType.Dish:
                    Program.form.playerLabels[id_t].Location =
                        new System.Drawing.Point(
                            (int)((gameObjectMessage.PositionX - 0.5) * GameForm.Form1.LABEL_WIDTH + Form1.HALF_LABEL_INTERVAL),
                            Convert.ToInt32((WorldMap.Height - gameObjectMessage.PositionY - 0.5) * GameForm.Form1.LABEL_WIDTH + Form1.HALF_LABEL_INTERVAL));
                    Program.form.playerLabels[id_t].Text = gameObjectMessage.DishType.ToString();
                    break;
                case ObjType.Tool:
                    Program.form.playerLabels[id_t].Location =
                        new System.Drawing.Point(
                            (int)((gameObjectMessage.PositionX - 0.5) * GameForm.Form1.LABEL_WIDTH + Form1.HALF_LABEL_INTERVAL),
                            Convert.ToInt32((WorldMap.Height - gameObjectMessage.PositionY - 0.5) * GameForm.Form1.LABEL_WIDTH + Form1.HALF_LABEL_INTERVAL));
                    Program.form.playerLabels[id_t].Text = gameObjectMessage.ToolType.ToString();
                    break;
                case ObjType.Trigger:
                    Program.form.playerLabels[id_t].Location =
                        new System.Drawing.Point(
                            (int)((gameObjectMessage.PositionX - 0.5) * GameForm.Form1.LABEL_WIDTH + Form1.HALF_LABEL_INTERVAL),
                            Convert.ToInt32((WorldMap.Height - gameObjectMessage.PositionY - 0.5) * GameForm.Form1.LABEL_WIDTH + Form1.HALF_LABEL_INTERVAL));
                    break;
            }
        }
        public void initializeFormLabelMethod(Int64 id_t, Communication.Proto.GameObject gameObjectMessage)
        {
            switch (gameObjectMessage.ObjType)
            {
                case ObjType.People:
                    Program.form.playerLabels[id_t].BackColor = System.Drawing.Color.Red;
                    Program.form.playerLabels[id_t].TabIndex = 1;
                    break;
                case ObjType.Block:
                    switch (gameObjectMessage.BlockType)
                    {
                        case BlockType.FoodPoint:
                            Program.form.playerLabels[id_t].BackColor = System.Drawing.Color.Purple;
                            Program.form.playerLabels[id_t].Text = gameObjectMessage.DishType.ToString();
                            break;
                        case BlockType.Cooker:
                            Program.form.playerLabels[id_t].BackColor = System.Drawing.Color.SandyBrown;
                            break;
                        case BlockType.RubbishBin:
                            Program.form.playerLabels[id_t].BackColor = System.Drawing.Color.DarkGreen;
                            break;
                    }
                    break;
                case ObjType.Dish:
                    Program.form.playerLabels[id_t].BackColor = System.Drawing.Color.LightSalmon;
                    Program.form.playerLabels[id_t].Text = gameObjectMessage.DishType.ToString();
                    break;
                case ObjType.Tool:
                    Program.form.playerLabels[id_t].BackColor = System.Drawing.Color.LightCyan;
                    Program.form.playerLabels[id_t].Text = gameObjectMessage.ToolType.ToString();
                    break;
                case ObjType.Trigger:
                    Program.form.playerLabels[id_t].BackColor = System.Drawing.Color.DarkBlue;
                    Program.form.playerLabels[id_t].Text = gameObjectMessage.TriggerType.ToString();
                    break;
            }

            Program.form.playerLabels[id_t].Location =
                        new System.Drawing.Point(
                            (int)((gameObjectMessage.PositionX - 0.5) * GameForm.Form1.LABEL_WIDTH + Form1.HALF_LABEL_INTERVAL),
                            Convert.ToInt32((WorldMap.Height - gameObjectMessage.PositionY - 0.5) * GameForm.Form1.LABEL_WIDTH + Form1.HALF_LABEL_INTERVAL));
            Program.form.playerLabels[id_t].Size = new System.Drawing.Size(Form1.LABEL_WIDTH - Form1.LABEL_INTERVAL, Form1.LABEL_WIDTH - Form1.LABEL_INTERVAL);
            Program.form.playerLabels[id_t].BringToFront();
        }

        public void CreatePlayerLabel(Int64 id_t)
        {
            Program.form.playerLabels.Add(id_t, new System.Windows.Forms.Label());
            if (Program.form.InvokeRequired)
            {
                Program.form.Invoke(new Action(() =>
                {
                    Program.form.Controls.Add(Program.form.playerLabels[id_t]);
                }));
            }
            else
            {
                Program.form.Controls.Add(Program.form.playerLabels[id_t]);
            }
        }

        public void moveFormLabel(Int64 id_t, Communication.Proto.GameObject gameObjectMessage, ref HashSet<Int64> IDsToDelete)
        {
            if (!Program.form.playerLabels.ContainsKey(id_t))
            {
                CreatePlayerLabel(id_t);
                if (Program.form.playerLabels[id_t].InvokeRequired)
                {
                    Program.form.playerLabels[id_t].Invoke(new Action<Int64, Communication.Proto.GameObject>(initializeFormLabelMethod), id_t, gameObjectMessage);
                }
                else
                {
                    initializeFormLabelMethod(id_t, gameObjectMessage);
                }
                //recordDic[true].Add(id_t);
                Console.WriteLine("New Form : " + id_t + "  (" + gameObjectMessage.PositionX + "," + gameObjectMessage.PositionY + ")  " + gameObjectMessage.ObjType);
            }
            else
            {
                //recordDic[true].Add(id_t);
                IDsToDelete.Remove(id_t);
                //Console.WriteLine("Change Form");
            }

            if (lastRecieveMessage.GameObjectList.ContainsKey(id_t) && gameObjectMessage.GetHashCode() == lastRecieveMessage.GameObjectList[id_t].GetHashCode())
            {
                //Console.WriteLine("Same " + id_t);
                return;
            }
            if (Program.form.playerLabels[id_t].InvokeRequired)
            {
                Program.form.playerLabels[id_t].Invoke(new Action<Int64, Communication.Proto.GameObject>(RefreshFormLabelMethod), id_t, gameObjectMessage);
            }
            else
            {
                RefreshFormLabelMethod(id_t, gameObjectMessage);
            }
        }
        public Player(double x, double y, ushort agentPort, Talent talent, bool isPlayBack) :
            base(x, y)
        {
            this.isPlayBack = isPlayBack;
            if (!isPlayBack)
            {
                ClientCommunication.Initialize();
                ClientCommunication.ReceiveMessage += OnReceive;
                ClientCommunication.ConnectServer(new IPEndPoint(IPAddress.Loopback, agentPort));
            }
            playerTalent = talent;

            if (isPlayBack)
            {
                Reader = new PlayBack.Reader("server.playback");
                PlayBack();
            }
            else
                new Thread(Operation).Start();
        }
        private void Operation()
        {
            lastSendTime = DateTime.Now;
            char key;
            while (true)
            {
                try
                {

                    key = Console.ReadKey().KeyChar;
                    if (!isStartedGame)
                        continue;
                    if ((DateTime.Now - lastSendTime).TotalSeconds <= TimeInterval)
                        continue;
                    switch (key)
                    {
                        case 'd':
                            Move(THUnity2D.Direction.Right, 1000);
                            break;
                        case 'e':
                            Move(THUnity2D.Direction.RightUp, 1000);
                            break;
                        case 'w':
                            Move(THUnity2D.Direction.Up, 1000);
                            break;
                        case 'q':
                            Move(THUnity2D.Direction.LeftUp, 1000);
                            break;
                        case 'a':
                            Move(THUnity2D.Direction.Left, 1000);
                            break;
                        case 'z':
                            Move(THUnity2D.Direction.LeftDown, 1000);
                            break;
                        case 'x':
                            Move(THUnity2D.Direction.Down, 1000);
                            break;
                        case 'c':
                            Move(THUnity2D.Direction.RightDown, 1000);
                            break;
                        case 'f':
                            Console.WriteLine("\nPlease Input 3 parameters : isSelfPosition, pickType, dishOrToolType");
                            Pick(Convert.ToBoolean(int.Parse(Console.ReadLine())), (ObjType)int.Parse(Console.ReadLine()), int.Parse(Console.ReadLine()));
                            break;
                        case 'u':
                            Console.WriteLine("\nPlease Input 2 parameters : ");
                            Use(1, double.Parse(Console.ReadLine()), double.Parse(Console.ReadLine()));
                            break;
                        case 'i': Use(0, 0); break;
                        case 'r':
                            Console.WriteLine("\nPlease Input 2 parameters : ");
                            Put(double.Parse(Console.ReadLine()), double.Parse(Console.ReadLine()), true);
                            break;
                        case 't':
                            Console.WriteLine("\nPlease Input 2 parameters : ");
                            Put(double.Parse(Console.ReadLine()), double.Parse(Console.ReadLine()), false);
                            break;
                        case ':':
                            Console.WriteLine("\nPlease Input your text to speak : ");
                            SpeakToFriend(Console.ReadLine());
                            break;
                    }
                    lastSendTime = DateTime.Now;
                }
                catch (ArgumentNullException)
                {
                    ClientDebug("\nArgument Null !!!");
                }
                catch (FormatException)
                {
                    ClientDebug("\nFormat Incorrect !!!");
                }
            }
        }

        private object PlayBackLock = new object();
        public void PlayBack()
        {
            Console.WriteLine("Press any key to start playback");
            Console.ReadKey();
            IMessage<MessageToClient> message;
            playBackTimer = new System.Threading.Timer(
                (o) =>
                {
                    lock (PlayBackLock)
                    {
                        message = new MessageToClient();
                        if (Reader.Read(ref message))
                            OnReceive(message);
                        else
                        {
                            Console.WriteLine("PlayBack End !");
                            playBackTimer.Dispose();
                        }
                    }
                }, null, 0, 50);
        }
        public override void Move(THUnity2D.Direction direction_t, int duration = 1000)
        {
            messageToServer.CommandType = CommandType.Move;

            //值检查放在Server
            messageToServer.MoveDirection = (Communication.Proto.Direction)direction_t;
            messageToServer.MoveDuration = duration;

            ClientCommunication.SendMessage(messageToServer);
        }
        public override void Put(double distance, double angle, bool isThrowDish)
        {
            //值检查放在Server
            messageToServer.ThrowDistance = distance;
            messageToServer.ThrowAngle = angle;
            messageToServer.IsThrowDish = isThrowDish;

            messageToServer.CommandType = CommandType.Put;
            ClientCommunication.SendMessage(messageToServer);
        }
        public override void Use(int type, double parameter_1 = 0, double parameter_2 = 0)
        {
            //在Server端控制变量范围
            messageToServer.CommandType = CommandType.Use;
            messageToServer.UseType = type;
            messageToServer.Parameter1 = parameter_1;
            messageToServer.Parameter2 = parameter_2;
            ClientCommunication.SendMessage(messageToServer);
        }
        public override void Pick(bool isSelfPosition, ObjType pickType, int dishOrToolType)
        {
            messageToServer.CommandType = CommandType.Pick;
            messageToServer.IsPickSelfPosition = isSelfPosition;
            messageToServer.PickType = pickType;
            messageToServer.PickDishOrToolType = dishOrToolType;
            ClientCommunication.SendMessage(messageToServer);
        }
        public void SpeakToFriend(string speakText)
        {
            messageToServer.CommandType = CommandType.Speak;
            if (speakText.Length > 16)//限制发送的字符串长度为16
                speakText = speakText.Substring(0, 15);
            messageToServer.SpeakText = speakText;
            ClientCommunication.SendMessage(messageToServer);
        }

        public void OnReceive(IMessage message)
        {
            if (!(message is MessageToClient))
                throw new Exception("Recieve Error !");
            MessageToClient msg = message as MessageToClient;

            //Console.WriteLine("\nRecieve :");
            //foreach (var item in msg.GameObjectList)
            //{
            //    Console.Write("\t" + item.Key);
            //}
            //Console.WriteLine("");

            //自己的id小于0时为未初始化状态，此时初始化自己的id，然后发送message选择天赋
            if (this.id < 0)
            {
                isStartedGame = true;
                foreach (var gameObject in msg.GameObjectList)
                {
                    this.id = gameObject.Key;
                    this.team = gameObject.Value.Team;
                    if (!isPlayBack)
                    {
                        Console.WriteLine("\nThis Player :\n" + "\t" + id.ToString() + "\n\tposition: " + Position.ToString());
                        ClientCommunication.SendMessage(new MessageToServer
                        {
                            Talent = playerTalent,
                            IsSetTalent = true
                        });
                    }
                    break;
                }
                messageToServer.ID = this.id;
            }

            //this.Position = new XYPosition(msg.GameObjectList[this.id].PositionX, msg.GameObjectList[this.id].PositionY);
            //this._facingDirection = (THUnity2D.Direction)msg.GameObjectList[this.id].Direction;

            ChangeAllLabels(msg);
            lastRecieveMessage = msg;
        }

        HashSet<Int64> IDsToDelete = new HashSet<long>();
        public void ChangeAllLabels(MessageToClient msg)
        {
            if (lastRecieveMessage.GetHashCode() == msg.GetHashCode())
            {
                //Console.WriteLine("Same messageToClient");
                return;
            }
            IDsToDelete = new HashSet<long>(Program.form.playerLabels.Keys);
            foreach (var gameObject in msg.GameObjectList)
            {
                moveFormLabel(gameObject.Key, gameObject.Value, ref IDsToDelete);
            }
            foreach (var number in IDsToDelete)
            {
                Console.WriteLine("Delete Form : " + number);
                if (Program.form.InvokeRequired)
                {
                    Program.form.Invoke(new Action(() =>
                    {
                        Program.form.Controls.Remove(Program.form.playerLabels[number]);
                    }));
                }
                else
                {
                    Program.form.Controls.Remove(Program.form.playerLabels[number]);
                }
                Program.form.playerLabels.Remove(number);
            }

            if (lastRecieveMessage.Tasks.GetHashCode() == msg.Tasks.GetHashCode())
            {
                //Console.WriteLine("Same Tasks");
                return;
            }

            if (Program.form.ControlLabels["Task"].InvokeRequired)
            {
                Program.form.ControlLabels["Task"].Invoke(new Action<MessageToClient>(ChangeTaskLabel), msg);
            }
            else
            {
                ChangeTaskLabel(msg);
            }
            if (msg.Scores.ContainsKey(team))
                ChangeControlLabelText("Score", msg.Scores[team].ToString());
        }

        public void ChangeTaskLabel(MessageToClient msg)
        {
            Program.form.ControlLabels["Task"].Text = "Task : ";
            foreach (var task in msg.Tasks)
            {
                Program.form.ControlLabels["Task"].Text += "\n" + (DishType)task;
            }
        }

        public static Action<string> ClientDebug = (string str) =>
        {
            Console.WriteLine(str);
        };
    }

}