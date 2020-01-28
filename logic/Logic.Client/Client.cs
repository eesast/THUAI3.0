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
using static Logic.Constant.Map;
using THUnity2D;
using static THUnity2D.Tools;
using GameForm;
namespace Client
{
    class Player : Character
    {
        protected Int64 id = -1;
        private static int port = 30000;
        static Thread operationThread;
        public static DateTime lastSendTime = new DateTime();
        public Communication.CAPI.API ClientCommunication = new Communication.CAPI.API();
        public void moveFormLabel(Int64 id_t, XYPosition xy_t, Direction direction_t)
        {
            if (!Program.form.playerLabels.ContainsKey(id_t))
            {
                Program.form.playerLabels.Add(id_t, new System.Windows.Forms.Label());
                Program.form.playerLabels[id_t].BackColor = System.Drawing.Color.Red;
                Program.form.playerLabels[id_t].Name = "playerLabel";
                Program.form.playerLabels[id_t].TabIndex = 1;
                if (Program.form.InvokeRequired)
                {
                    Action<object> actionDelegate1 = (o) =>
                    {
                        Program.form.Controls.Add(Program.form.playerLabels[id_t]);
                    };
                    Program.form.Invoke(actionDelegate1, new object());
                }
                else
                {
                    Program.form.Controls.Add(Program.form.playerLabels[id_t]);
                }

                if (Program.form.playerLabels[id_t].InvokeRequired)
                {
                    Action<object> actionDelegate1 = (o) =>
                    {
                        Program.form.playerLabels[id_t].Size = new System.Drawing.Size(Form1.LABEL_WIDTH, Form1.LABEL_WIDTH);
                    };
                    Program.form.playerLabels[id_t].Invoke(actionDelegate1, new object());
                }
                else
                {
                    Program.form.playerLabels[id_t].Size = new System.Drawing.Size(Form1.LABEL_WIDTH, Form1.LABEL_WIDTH);
                }
            }
            if (Program.form.playerLabels[id_t].InvokeRequired)
            {
                // 当一个控件的InvokeRequired属性值为真时，说明有一个创建它以外的线程想访问它
                Action<XYPosition, Direction> actionDelegate = (xy_t2, direction_t2) =>
                {
                    Program.form.playerLabels[id_t].Location = new System.Drawing.Point(Convert.ToInt32((xy_t2.x - 0.5) * Convert.ToDouble(GameForm.Form1.LABEL_WIDTH)), Convert.ToInt32((Convert.ToDouble(WorldMap.Height) - xy_t2.y - 0.5) * Convert.ToDouble(GameForm.Form1.LABEL_WIDTH)));
                    switch (direction_t2)
                    {
                        case Direction.Right:
                            Program.form.playerLabels[id_t].Text = "→";
                            break;
                        case Direction.RightUp:
                            Program.form.playerLabels[id_t].Text = "↗";
                            break;
                        case Direction.Up:
                            Program.form.playerLabels[id_t].Text = "↑";
                            break;
                        case Direction.LeftUp:
                            Program.form.playerLabels[id_t].Text = "↖";
                            break;
                        case Direction.Left:
                            Program.form.playerLabels[id_t].Text = "←";
                            break;
                        case Direction.LeftDown:
                            Program.form.playerLabels[id_t].Text = "↙";
                            break;
                        case Direction.Down:
                            Program.form.playerLabels[id_t].Text = "↓";
                            break;
                        case Direction.RightDown:
                            Program.form.playerLabels[id_t].Text = "↘";
                            break;
                        default:
                            break;
                    }
                };
                Program.form.playerLabels[id_t].Invoke(actionDelegate, xy_t, direction_t);
            }
            else
            {
                Program.form.playerLabels[id_t].Location = new System.Drawing.Point(Convert.ToInt32((xy_t.x - 0.5) * Convert.ToDouble(GameForm.Form1.LABEL_WIDTH)), Convert.ToInt32((Convert.ToDouble(WorldMap.Width) - xy_t.y - 0.5) * Convert.ToDouble(GameForm.Form1.LABEL_WIDTH)));
                switch (direction_t)
                {
                    case Direction.Right:
                        Program.form.playerLabels[id_t].Text = "→";
                        break;
                    case Direction.RightUp:
                        Program.form.playerLabels[id_t].Text = "↗";
                        break;
                    case Direction.Up:
                        Program.form.playerLabels[id_t].Text = "↑";
                        break;
                    case Direction.LeftUp:
                        Program.form.playerLabels[id_t].Text = "↖";
                        break;
                    case Direction.Left:
                        Program.form.playerLabels[id_t].Text = "←";
                        break;
                    case Direction.LeftDown:
                        Program.form.playerLabels[id_t].Text = "↙";
                        break;
                    case Direction.Down:
                        Program.form.playerLabels[id_t].Text = "↓";
                        break;
                    case Direction.RightDown:
                        Program.form.playerLabels[id_t].Text = "↘";
                        break;
                    default:
                        break;
                }
            }
        }
        public Player(double x, double y) :
            base(x, y)
        {
            ClientCommunication.Initialize();
            ClientCommunication.ReceiveMessage += OnReceive;
            ClientCommunication.ConnectServer(new IPEndPoint(IPAddress.Loopback, port));

            operationThread = new Thread(Operation);
            operationThread.Start();
        }
        private void Operation()
        {
            TimeSpan deltaTime = DateTime.Now - lastSendTime;
            if (deltaTime.TotalSeconds <= TimeInterval)
                return;

            char key;
            while (true)
            {
                key = Console.ReadKey().KeyChar;
                if (key == 'd') Move(Direction.Right);
                else if (key == 'e') Move(Direction.RightUp);
                else if (key == 'w') Move(Direction.Up);
                else if (key == 'q') Move(Direction.LeftUp);
                else if (key == 'a') Move(Direction.Left);
                else if (key == 'z') Move(Direction.LeftDown);
                else if (key == 'x') Move(Direction.Down);
                else if (key == 'c') Move(Direction.RightDown);
                lastSendTime = DateTime.Now;
            }
        }
        public new void Move(Direction direction)
        {
            ClientCommunication.SendMessage(
                new MessageToServer
                {
                    ID = this.id,
                    CommandType = (int)COMMAND_TYPE.MOVE,
                    Parameter1 = (int)direction,
                    Parameter2 = 0
                }
            );
        }

        public void OnReceive(IMessage message)
        {
            if (!(message is MessageToClient)) throw new Exception("Recieve Error !");
            MessageToClient msg = message as MessageToClient;

            if (this.id < 0)
            {
                foreach (var gameObject in msg.GameObjectMessageList)
                {
                    this.id = gameObject.Key;
                    this.Position = new XYPosition(gameObject.Value.Position.X, gameObject.Value.Position.Y);
                    this.facingDirection = (Tools.Direction)(int)gameObject.Value.Direction;
                    Console.WriteLine("\nThis Player :\n" + "\t" + id.ToString() + "\n\tposition: " + Position.ToString());
                    moveFormLabel(this.id, this.Position, this.facingDirection);
                    return;
                }
            }
            foreach (var gameObject in msg.GameObjectMessageList)
            {
                this.Position = new XYPosition(gameObject.Value.Position.X, gameObject.Value.Position.Y);
                this.facingDirection = (Tools.Direction)(int)gameObject.Value.Direction;
                Console.WriteLine("\nPlayer " + gameObject.Key.ToString() + "  position: " + Position.ToString());
                moveFormLabel(gameObject.Key, Position, facingDirection);
                break;
            }
        }
    }

}