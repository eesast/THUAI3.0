using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Google.Protobuf;
using Logic.Constant;
using static Logic.Constant.CONSTANT;
using WindowsFormsApp2;
namespace Client
{
    class Player : Character
    {
        private static int port = 8889;
        static Thread operationThread;
        public static DateTime lastSendTime = new DateTime();
        public Communication.CAPI.API ClientCommunication = new Communication.CAPI.API();
        public delegate void MoveFormLabel(XY_Position xy);
        public MoveFormLabel moveFormLabel;
        public Player(double x, double y) :
            base(new Tuple<int, int>(0, 0), x, y)
        {
            //moveFormLabel = new MoveFormLabel(Program.form.moveLabel);

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
            //this.id.Item1 = msg.PlayerIDAgent;
            //this.id.Item2 = msg.PlayerIDClient;
            this.xyPosition.x = BitConverter.Int64BitsToDouble(msg.PlayerPositionX);
            this.xyPosition.y = BitConverter.Int64BitsToDouble(msg.PlayerPositionY);

            Console.WriteLine("Player " + msg.PlayerIDAgent.ToString() + " , " + msg.PlayerIDClient.ToString() + "  position: " + xyPosition.ToString());
            moveFormLabel(xyPosition);
        }

    }

}