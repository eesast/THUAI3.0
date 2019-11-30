using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Google.Protobuf;
using Logic.Constant;
using Communication.Proto;
using static Logic.Constant.CONSTANT;
using static Map;
using GameForm;
namespace Client
{
    class Player : Character
    {
        private static int port = 30000;
        static Thread operationThread;
        public static DateTime lastSendTime = new DateTime();
        public Communication.CAPI.API ClientCommunication = new Communication.CAPI.API();
        public void moveFormLabel(Tuple<int, int> id_t, XY_Position xy_t, Direction direction_t)
        {
            if (Program.form.playerLabels[id_t].InvokeRequired)
            {
                // 当一个控件的InvokeRequired属性值为真时，说明有一个创建它以外的线程想访问它
                Action<XY_Position, Direction> actionDelegate = (xy_t2, direction_t2) =>
                {
                    Program.form.playerLabels[id_t].Location = new System.Drawing.Point(Convert.ToInt32((xy_t2.x - 0.5) * Convert.ToDouble(GameForm.Form1.LABEL_WIDTH)), Convert.ToInt32((Convert.ToDouble(WORLD_MAP_HEIGHT) - xy_t2.y - 0.5) * Convert.ToDouble(GameForm.Form1.LABEL_WIDTH)));
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
                Program.form.playerLabels[id_t].Location = new System.Drawing.Point(Convert.ToInt32((xy_t.x - 0.5) * Convert.ToDouble(GameForm.Form1.LABEL_WIDTH)), Convert.ToInt32((Convert.ToDouble(WORLD_MAP_HEIGHT) - xy_t.y - 0.5) * Convert.ToDouble(GameForm.Form1.LABEL_WIDTH)));
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
            if (deltaTime.TotalSeconds <= TIME_INTERVAL)
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
        public override void Move(Direction direction)
        {
            ClientCommunication.SendMessage(
                new MessageToServer
                {
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

            if (this.id.Item1 < 0 || this.id.Item2 < 0)
            {
                this.id = new Tuple<int, int>(msg.PlayerIDAgent, msg.PlayerIDClient);
                this.xyPosition.x = BitConverter.Int64BitsToDouble(msg.PlayerPositionX);
                this.xyPosition.y = BitConverter.Int64BitsToDouble(msg.PlayerPositionY);
                this.facingDirection = (Direction)msg.FacingDirection;
                Console.WriteLine("\nThis Player :\n" + "\t" + id.ToString() + "\n\tposition: " + xyPosition.ToString());
                return;
            }

            this.xyPosition.x = BitConverter.Int64BitsToDouble(msg.PlayerPositionX);
            this.xyPosition.y = BitConverter.Int64BitsToDouble(msg.PlayerPositionY);
            this.facingDirection = (Direction)msg.FacingDirection;

            Console.WriteLine("\nPlayer " + msg.PlayerIDAgent.ToString() + " , " + msg.PlayerIDClient.ToString() + "  position: " + xyPosition.ToString());
            moveFormLabel(new Tuple<int, int>(msg.PlayerIDAgent, msg.PlayerIDClient), xyPosition, facingDirection);
        }
    }

}