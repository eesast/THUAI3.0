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
namespace Client
{
    class Player : Character
    {
        protected Int64 id = -1;//注意！！！在这个类里基类的ID已被弃用
        protected static DateTime lastSendTime = new DateTime();
        protected Communication.CAPI.API ClientCommunication = new Communication.CAPI.API();
        protected MessageToServer messageToServer = new MessageToServer();
        public void ChangeControlLabelText(string id, string str)
        {
            if (Program.form.ControlLabels[id].InvokeRequired)
            {
                Program.form.ControlLabels[id].Invoke(new Action<object>((o) => { Program.form.ControlLabels[id].Text = id + "  " + str; }));
            }
            else
            {
                Program.form.ControlLabels[id].Text = id + "  " + str;
            }
        }
        public void RefreshFormLabelMethod(Int64 id_t, GameObjectMessage gameObjectMessage)
        {
            switch (gameObjectMessage.ObjType)
            {
                case ObjTypeMessage.People:
                    Program.form.playerLabels[id_t].Location =
                        new System.Drawing.Point(
                            (int)((gameObjectMessage.Position.X - 0.5) * GameForm.Form1.LABEL_WIDTH + Form1.HALF_LABEL_INTERVAL),
                            Convert.ToInt32((WorldMap.Height - gameObjectMessage.Position.Y - 0.5) * GameForm.Form1.LABEL_WIDTH + Form1.HALF_LABEL_INTERVAL));
                    switch ((Direction)gameObjectMessage.Direction)
                    {
                        case Direction.Right: Program.form.playerLabels[id_t].Text = "→"; break;
                        case Direction.RightUp: Program.form.playerLabels[id_t].Text = "↗"; break;
                        case Direction.Up: Program.form.playerLabels[id_t].Text = "↑"; break;
                        case Direction.LeftUp: Program.form.playerLabels[id_t].Text = "↖"; break;
                        case Direction.Left: Program.form.playerLabels[id_t].Text = "←"; break;
                        case Direction.LeftDown: Program.form.playerLabels[id_t].Text = "↙"; break;
                        case Direction.Down: Program.form.playerLabels[id_t].Text = "↓"; break;
                        case Direction.RightDown: Program.form.playerLabels[id_t].Text = "↘"; break;
                        default: break;
                    }
                    if (id_t == this.id)
                    {
                        if (gameObjectMessage.DishType != DishTypeMessage.DishEmpty)
                            ChangeControlLabelText("Dish", gameObjectMessage.DishType.ToString());
                        else
                            ChangeControlLabelText("Dish", "");
                        if (gameObjectMessage.ToolType != ToolTypeMessage.ToolEmpty
                            && gameObjectMessage.ToolType != ToolTypeMessage.ToolSize)
                            ChangeControlLabelText("Tool", gameObjectMessage.ToolType.ToString());
                        else
                            ChangeControlLabelText("Tool", "");
                        ChangeControlLabelText("Score", gameObjectMessage.Score.ToString());
                    }
                    break;
                case ObjTypeMessage.Block:
                    switch (gameObjectMessage.BlockType)
                    {
                        case BlockTypeMessage.FoodPoint:
                            if (gameObjectMessage.DishType == DishTypeMessage.DishEmpty)
                                Program.form.playerLabels[id_t].Text = "";
                            else
                                Program.form.playerLabels[id_t].Text = gameObjectMessage.DishType.ToString();
                            break;
                        case BlockTypeMessage.Cooker:
                            if (gameObjectMessage.DishType == DishTypeMessage.DishEmpty)
                                Program.form.playerLabels[id_t].Text = "";
                            else
                                Program.form.playerLabels[id_t].Text = gameObjectMessage.DishType.ToString();
                            break;
                    }
                    break;
                case ObjTypeMessage.Dish:
                    Program.form.playerLabels[id_t].Location =
                        new System.Drawing.Point(
                            (int)((gameObjectMessage.Position.X - 0.5) * GameForm.Form1.LABEL_WIDTH + Form1.HALF_LABEL_INTERVAL),
                            Convert.ToInt32((WorldMap.Height - gameObjectMessage.Position.Y - 0.5) * GameForm.Form1.LABEL_WIDTH + Form1.HALF_LABEL_INTERVAL));
                    Program.form.playerLabels[id_t].Text = gameObjectMessage.DishType.ToString();
                    break;
                case ObjTypeMessage.Tool:
                    Program.form.playerLabels[id_t].Location =
                        new System.Drawing.Point(
                            (int)((gameObjectMessage.Position.X - 0.5) * GameForm.Form1.LABEL_WIDTH + Form1.HALF_LABEL_INTERVAL),
                            Convert.ToInt32((WorldMap.Height - gameObjectMessage.Position.Y - 0.5) * GameForm.Form1.LABEL_WIDTH + Form1.HALF_LABEL_INTERVAL));
                    Program.form.playerLabels[id_t].Text = gameObjectMessage.ToolType.ToString();
                    break;
                case ObjTypeMessage.Trigger:
                    break;
            }
        }
        public void initializeFormLabelMethod(Int64 id_t, GameObjectMessage gameObjectMessage)
        {
            switch (gameObjectMessage.ObjType)
            {
                case ObjTypeMessage.People:
                    Program.form.playerLabels[id_t].BackColor = System.Drawing.Color.Red;
                    Program.form.playerLabels[id_t].TabIndex = 1;
                    break;
                case ObjTypeMessage.Block:
                    switch (gameObjectMessage.BlockType)
                    {
                        case BlockTypeMessage.FoodPoint:
                            Program.form.playerLabels[id_t].BackColor = System.Drawing.Color.Purple;
                            Program.form.playerLabels[id_t].Text = gameObjectMessage.DishType.ToString();
                            break;
                        case BlockTypeMessage.Cooker:
                            Program.form.playerLabels[id_t].BackColor = System.Drawing.Color.SandyBrown;
                            break;
                        case BlockTypeMessage.RubbishBin:
                            Program.form.playerLabels[id_t].BackColor = System.Drawing.Color.DarkGreen;
                            break;
                    }
                    break;
                case ObjTypeMessage.Dish:
                    Program.form.playerLabels[id_t].BackColor = System.Drawing.Color.LightSalmon;
                    Program.form.playerLabels[id_t].Text = gameObjectMessage.DishType.ToString();
                    break;
                case ObjTypeMessage.Tool:
                    Program.form.playerLabels[id_t].BackColor = System.Drawing.Color.LightCyan;
                    Program.form.playerLabels[id_t].Text = gameObjectMessage.ToolType.ToString();
                    break;
                case ObjTypeMessage.Trigger:
                    Program.form.playerLabels[id_t].BackColor = System.Drawing.Color.DarkBlue;
                    Program.form.playerLabels[id_t].Text = gameObjectMessage.TriggerType.ToString();
                    break;
            }

            Program.form.playerLabels[id_t].Location =
                        new System.Drawing.Point(
                            (int)((gameObjectMessage.Position.X - 0.5) * GameForm.Form1.LABEL_WIDTH + Form1.HALF_LABEL_INTERVAL),
                            Convert.ToInt32((WorldMap.Height - gameObjectMessage.Position.Y - 0.5) * GameForm.Form1.LABEL_WIDTH + Form1.HALF_LABEL_INTERVAL));
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

        public void moveFormLabel(Int64 id_t, GameObjectMessage gameObjectMessage, ref HashSet<Int64> IDsToDelete)
        {
            if (!Program.form.playerLabels.ContainsKey(id_t))
            {
                CreatePlayerLabel(id_t);
                if (Program.form.playerLabels[id_t].InvokeRequired)
                {
                    Program.form.playerLabels[id_t].Invoke(new Action<Int64, GameObjectMessage>(initializeFormLabelMethod), id_t, gameObjectMessage);
                }
                else
                {
                    initializeFormLabelMethod(id_t, gameObjectMessage);
                }
                //recordDic[true].Add(id_t);
                Console.WriteLine("New Form : " + id_t + "  (" + gameObjectMessage.Position.X + "," + gameObjectMessage.Position.Y + ")  " + gameObjectMessage.ObjType);
            }
            else
            {
                //recordDic[true].Add(id_t);
                IDsToDelete.Remove(id_t);
                //Console.WriteLine("Change Form");
            }

            if (Program.form.playerLabels[id_t].InvokeRequired)
            {
                Program.form.playerLabels[id_t].Invoke(new Action<Int64, GameObjectMessage>(RefreshFormLabelMethod), id_t, gameObjectMessage);
            }
            else
            {
                RefreshFormLabelMethod(id_t, gameObjectMessage);
            }
        }
        public Player(double x, double y, ushort agentPort) :
            base(x, y)
        {
            ClientCommunication.Initialize();
            ClientCommunication.ReceiveMessage += OnReceive;
            ClientCommunication.ConnectServer(new IPEndPoint(IPAddress.Loopback, agentPort));

            new Thread(Operation).Start();
        }
        private void Operation()
        {
            lastSendTime = DateTime.Now;
            char key;
            while (true)
            {
                key = Console.ReadKey().KeyChar;

                if ((DateTime.Now - lastSendTime).TotalSeconds <= TimeInterval)
                    continue;
                switch (key)
                {
                    case 'd': Move(Direction.Right); break;
                    case 'e': Move(Direction.RightUp); break;
                    case 'w': Move(Direction.Up); break;
                    case 'q': Move(Direction.LeftUp); break;
                    case 'a': Move(Direction.Left); break;
                    case 'z': Move(Direction.LeftDown); break;
                    case 'x': Move(Direction.Down); break;
                    case 'c': Move(Direction.RightDown); break;
                    case 'f': Pick(); break;
                    case 'u':
                        {
                            if (tool == ToolType.SpaceGate)
                            {
                                Use(1, int.Parse(Console.ReadLine()), int.Parse(Console.ReadLine()));
                                break;
                            }
                            Use(1, 0, 0);
                        }
                        break;

                    case 'i': Use(0, 0); break;
                    case 'r':
                        char temp = Console.ReadKey().KeyChar;
                        if (temp >= '0' && temp <= '9')
                        {
                            Put(temp - '0', (double)facingDirection * Math.PI / 4, true);
                        }
                        break;
                    case 't':
                        char tmp = Console.ReadKey().KeyChar;
                        if (tmp >= '0' && tmp <= '9')
                        {
                            Put(tmp - '0', (double)facingDirection * Math.PI / 4, false);
                        }
                        break;
                    case ':':
                        SpeakToFriend(Console.ReadLine());
                        break;
                }
                lastSendTime = DateTime.Now;
            }
        }
        public override void Move(Direction direction_t, int duration = 1000)
        {
            messageToServer.CommandType = CommandTypeMessage.Move;

            //值检查放在Server
            messageToServer.MoveDirection = (DirectionMessage)direction_t;
            messageToServer.MoveDuration = duration;

            ClientCommunication.SendMessage(messageToServer);
        }
        public override void Put(double distance, double angle, bool isThrowDish)
        {
            //值检查放在Server
            messageToServer.ThrowDistance = distance;
            messageToServer.ThrowAngle = angle;
            messageToServer.IsThrowDish = isThrowDish;

            messageToServer.CommandType = CommandTypeMessage.Put;
            ClientCommunication.SendMessage(messageToServer);
        }
        public override void Use(int type, int parameter_1 = 0, int parameter_2 = 0)
        {
            //在Server端控制变量范围
            messageToServer.CommandType = CommandTypeMessage.Use;
            messageToServer.UseType = type;
            messageToServer.Parameter1 = parameter_1;
            messageToServer.Parameter2 = parameter_2;
            ClientCommunication.SendMessage(messageToServer);
        }
        public override void Pick()
        {
            messageToServer.CommandType = CommandTypeMessage.Pick;
            ClientCommunication.SendMessage(messageToServer);
        }
        public void SpeakToFriend(string speakText)
        {
            messageToServer.CommandType = CommandTypeMessage.Speak;
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

            //自己的id小于0时为未初始化状态，此时初始化自己的id
            if (this.id < 0)
            {
                foreach (var gameObject in msg.GameObjectMessageList)
                {
                    this.id = gameObject.Key;
                    this.Position = new XYPosition(gameObject.Value.Position.X, gameObject.Value.Position.Y);
                    this.facingDirection = (Tools.Direction)(int)gameObject.Value.Direction;
                    Console.WriteLine("\nThis Player :\n" + "\t" + id.ToString() + "\n\tposition: " + Position.ToString());
                    HashSet<long> tempDic = new HashSet<long>(Program.form.playerLabels.Keys);
                    moveFormLabel(this.id, gameObject.Value, ref tempDic);
                    return;
                }
                messageToServer.ID = this.id;
            }

            this.Position = new XYPosition(msg.GameObjectMessageList[this.id].Position.X, msg.GameObjectMessageList[this.id].Position.Y);
            this.facingDirection = (Tools.Direction)msg.GameObjectMessageList[this.id].Direction;

            ChangeAllLabels(msg);
        }

        HashSet<Int64> IDsToDelete = new HashSet<long>();
        public void ChangeAllLabels(MessageToClient msg)
        {
            IDsToDelete = new HashSet<long>(Program.form.playerLabels.Keys);
            foreach (var gameObject in msg.GameObjectMessageList)
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

            if (Program.form.ControlLabels["Task"].InvokeRequired)
            {
                Program.form.ControlLabels["Task"].Invoke(new Action<MessageToClient>(ChangeTaskLabel), msg);
            }
            else
            {
                ChangeTaskLabel(msg);
            }
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