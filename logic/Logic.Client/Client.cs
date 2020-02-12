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
        public void moveFormLabelMethod(Int64 id_t, GameObjectMessage gameObjectMessage)
        {
            Program.form.playerLabels[id_t].Location = new System.Drawing.Point(
                (int)((gameObjectMessage.Position.X - 0.5) * GameForm.Form1.LABEL_WIDTH + Form1.HALF_LABEL_INTERVAL),
                Convert.ToInt32((WorldMap.Height - gameObjectMessage.Position.Y - 0.5) * GameForm.Form1.LABEL_WIDTH + Form1.HALF_LABEL_INTERVAL));

            switch (gameObjectMessage.ObjType)
            {
                case ObjTypeMessage.People:
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
                    break;
                case ObjTypeMessage.Block:
                    break;
            }
        }
        public void initializeFormLabelMethod(Int64 id_t, GameObjectMessage gameObjectMessage)
        {
            switch (gameObjectMessage.ObjType)
            {
                case ObjTypeMessage.People: Program.form.playerLabels[id_t].BackColor = System.Drawing.Color.Red; break;
                case ObjTypeMessage.Block:
                    switch (gameObjectMessage.BlockType)
                    {
                        case BlockTypeMessage.FoodPoint:
                            Program.form.playerLabels[id_t].BackColor = System.Drawing.Color.Purple;
                            Program.form.playerLabels[id_t].Text = gameObjectMessage.DishType.ToString();
                            Console.WriteLine("New FootPoint Label");
                            break;
                    }
                    break;
            }
            Program.form.playerLabels[id_t].TabIndex = 1;
            Program.form.playerLabels[id_t].Size = new System.Drawing.Size(Form1.LABEL_WIDTH - Form1.LABEL_INTERVAL, Form1.LABEL_WIDTH - Form1.LABEL_INTERVAL);
            Program.form.playerLabels[id_t].BringToFront();
        }

        public void moveFormLabel(Int64 id_t, GameObjectMessage gameObjectMessage)
        {
            if (!Program.form.playerLabels.ContainsKey(id_t))
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

                if (Program.form.playerLabels[id_t].InvokeRequired)
                {
                    Program.form.playerLabels[id_t].Invoke(new Action<Int64, GameObjectMessage>(initializeFormLabelMethod), id_t, gameObjectMessage);
                }
                else
                {
                    initializeFormLabelMethod(id_t, gameObjectMessage);
                }
            }
            if (Program.form.playerLabels[id_t].InvokeRequired)
            {
                Program.form.playerLabels[id_t].Invoke(new Action<Int64, GameObjectMessage>(moveFormLabelMethod), id_t, gameObjectMessage);
            }
            else
            {
                moveFormLabelMethod(id_t, gameObjectMessage);
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
            lastSendTime = DateTime.Now;
            char key;
            while (true)
            {
                key = Console.ReadKey().KeyChar;

                if ((DateTime.Now - lastSendTime).TotalSeconds <= TimeInterval)
                    continue;
                switch (key)
                {
                    case 'd':
                        Move(Direction.Right);
                        break;
                    case 'e':
                        Move(Direction.RightUp);
                        break;
                    case 'w':
                        Move(Direction.Up);
                        break;
                    case 'q':
                        Move(Direction.LeftUp);
                        break;
                    case 'a':
                        Move(Direction.Left);
                        break;
                    case 'z':
                        Move(Direction.LeftDown); ;
                        break;
                    case 'x':
                        Move(Direction.Down);
                        break;
                    case 'c':
                        Move(Direction.RightDown);
                        break;
                    case 'f':
                        Pick();
                        break;
                }
                lastSendTime = DateTime.Now;
            }
        }
        public void Move(Direction direction)
        {
            ClientCommunication.SendMessage(
                new MessageToServer
                {
                    ID = this.id,
                    CommandType = (CommandTypeMessage)CommandType.Move,
                    MoveDirection = (DirectionMessage)direction
                }
            );
        }
        public override void Pick()
        {
            ClientCommunication.SendMessage(
                new MessageToServer
                {
                    ID = this.id,
                    CommandType = (CommandTypeMessage)CommandType.Pick
                }
            );
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
                    moveFormLabel(this.id, gameObject.Value);
                    return;
                }
            }

            this.Position = new XYPosition(msg.GameObjectMessageList[this.id].Position.X, msg.GameObjectMessageList[this.id].Position.Y);
            this.facingDirection = (Tools.Direction)msg.GameObjectMessageList[this.id].Direction;

            foreach (var gameObject in msg.GameObjectMessageList)
            {
                //Console.WriteLine("\nPlayer " + gameObject.Key.ToString() + "  position: " + Position.ToString());
                moveFormLabel(gameObject.Key, gameObject.Value);
            }
        }
    }

}