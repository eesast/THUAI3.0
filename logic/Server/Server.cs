using System;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.Text;
using Constant;
using System.Collections.Concurrent;
using static Constant.CONSTANT;
using System.Collections.Generic;


namespace Server
{
    static class Program
    {
        public static BlockingCollection<MessageToServer> messageQueue = new BlockingCollection<MessageToServer>();
        public static List<Obj>[,] WORLD_MAP = new List<Obj>[WORLD_MAP_WIDTH, WORLD_MAP_HEIGHT];
        private static DateTime initTime = new DateTime();
        public static void InitializeTime()
        {
            initTime = DateTime.Now;
        }
        public static DateTime InitTime
        {
            get
            {
                return initTime;
            }
        }
        private static TimeSpan gameTime = new TimeSpan();
        public static void RefreshGameTime()
        {
            gameTime = DateTime.Now - initTime;
        }
        public static TimeSpan GameTime
        {
            get
            {
                return gameTime;
            }
        }
        public static void Main(string[] args)
        {
            for (int i = 0; i < WORLD_MAP_WIDTH; i++)
                for (int j = 0; j < WORLD_MAP_HEIGHT; j++)
                {
                    if (
                        (i >= 25 && i <= 75 && j >= 25 && j <= 75) ||
                        i == 0 ||
                        i == WORLD_MAP_WIDTH - 1 ||
                        j == 0 ||
                        j == WORLD_MAP_HEIGHT - 1
                        )
                    {

                        WORLD_MAP[i, j] = new List<Obj>();
                        WORLD_MAP[i, j].Add(new Block(i + 0.5, j + 0.5));
                    }
                    else
                    {
                        WORLD_MAP[i, j] = new List<Obj>();
                    }
                }


            Server server = new Server();
        }
    }


    class Player : Character
    {
        private const int bufferSize = 1024;
        private Socket socket;
        private Thread recieveThread;
        private byte[] recieveBuffer = new Byte[bufferSize];
        private bool isConnected = false;
        public bool IsConnected
        {
            get { return isConnected; }
        }
        public Player(byte id_t, double x, double y, Socket serverSocket)
        :
        base(id_t, x, y)
        {
            socket = serverSocket.Accept();
            Console.WriteLine("{0} has connected to local.", socket.RemoteEndPoint);
            isConnected = true;

            CorrectPosition();
            Program.WORLD_MAP[(uint)xyPosition.x, (uint)xyPosition.y].Add(new People(xyPosition.x, xyPosition.y, id));
            recieveThread = new Thread(RecieveMessage);
            recieveThread.Start();

        }


        static double halfWidth = 0.5;
        static double halfHeight = 0.5;
        static XY_Position operationRightUp = new XY_Position(halfWidth, halfHeight);
        static XY_Position operationLeftUp = new XY_Position(-halfWidth, halfHeight);
        static XY_Position operationLeftDown = new XY_Position(-halfWidth, -halfHeight);
        static XY_Position operationRightDown = new XY_Position(halfWidth, -halfHeight);
        public bool CheckXYPosition(XY_Position xyPos)
        {
            XY_Position RightUp = xyPos + operationRightUp;
            XY_Position LeftUp = xyPos + operationLeftUp;
            XY_Position LeftDown = xyPos + operationLeftDown;
            XY_Position RightDown = xyPos + operationRightDown;

            if (Program.WORLD_MAP[(uint)RightUp.x, (uint)RightUp.y].Count > 0)
            {
                if (Program.WORLD_MAP[(uint)RightUp.x, (uint)RightUp.y][0] is Block)
                {
                    return false;
                }
                if (Program.WORLD_MAP[(uint)RightUp.x, (uint)RightUp.y][0] is People)
                {
                    if (((People)Program.WORLD_MAP[(uint)RightUp.x, (uint)RightUp.y][0]).id != id)
                    {
                        return false;
                    }
                }
            }
            if (Program.WORLD_MAP[(uint)LeftUp.x, (uint)LeftUp.y].Count > 0)
            {
                if (Program.WORLD_MAP[(uint)LeftUp.x, (uint)LeftUp.y][0] is Block)
                {
                    return false;
                }
                if (Program.WORLD_MAP[(uint)LeftUp.x, (uint)LeftUp.y][0] is People)
                {
                    if (((People)Program.WORLD_MAP[(uint)LeftUp.x, (uint)LeftUp.y][0]).id != id)
                    {
                        return false;
                    }
                }
            }
            if (Program.WORLD_MAP[(uint)LeftDown.x, (uint)LeftDown.y].Count > 0)
            {
                if (Program.WORLD_MAP[(uint)LeftDown.x, (uint)LeftDown.y][0] is Block)
                {
                    return false;
                }
                if (Program.WORLD_MAP[(uint)LeftDown.x, (uint)LeftDown.y][0] is People)
                {
                    if (((People)Program.WORLD_MAP[(uint)LeftDown.x, (uint)LeftDown.y][0]).id != id)
                    {
                        return false;
                    }
                }
            }
            if (Program.WORLD_MAP[(uint)RightDown.x, (uint)RightDown.y].Count > 0)
            {
                if (Program.WORLD_MAP[(uint)RightDown.x, (uint)RightDown.y][0] is Block)
                {
                    return false;
                }
                if (Program.WORLD_MAP[(uint)RightDown.x, (uint)RightDown.y][0] is People)
                {
                    if (((People)Program.WORLD_MAP[(uint)RightDown.x, (uint)RightDown.y][0]).id != id)
                    {
                        return false;
                    }
                }
            }
            return true;
        }
        private void CorrectPosition()
        {
            if (xyPosition.x <= 0 || xyPosition.x >= WORLD_MAP_WIDTH - 1 || xyPosition.y <= 0 || xyPosition.y >= WORLD_MAP_HEIGHT - 1)
            {
                xyPosition.x = WORLD_MAP_WIDTH * 0.5;
                xyPosition.y = WORLD_MAP_HEIGHT * 0.5;
            }
            if (CheckXYPosition(xyPosition))
                return;
            XY_Position[] searchers = new XY_Position[4];
            for (int i = 0; i < 4; i++)
            {
                searchers[i] = new XY_Position(xyPosition.x, xyPosition.y);
            }
            while (true)
            {
                searchers[0] = searchers[0] + new XY_Position(1, 0);
                searchers[1] = searchers[1] + new XY_Position(0, 1);
                searchers[2] = searchers[2] + new XY_Position(-1, 0);
                searchers[3] = searchers[3] + new XY_Position(0, -1);

                foreach (XY_Position searcher in searchers)
                {
                    if (CheckXYPosition(searcher))
                    {
                        xyPosition = searcher;
                        return;
                    }
                }
            }
        }
        public override void Move(DIRECTION direction)
        {
            //Program.WORLD_MAP[(uint)xyPosition.x, (uint)xyPosition.y].RemoveAll(obj => { return obj.GetType().Name == "People"; });
            XY_Position aim = OPERATION[(uint)direction] + xyPosition;
            if (CheckXYPosition(aim))
            {
                if ((uint)xyPosition.x != (uint)aim.x || (uint)xyPosition.y != (uint)aim.y)
                {
                    Program.WORLD_MAP[(uint)xyPosition.x, (uint)xyPosition.y][0].RemoveSelf();
                    xyPosition = aim;
                    Program.WORLD_MAP[(uint)xyPosition.x, (uint)xyPosition.y].Insert(0, new People(xyPosition.x, xyPosition.y, id));
                }
                else
                {
                    xyPosition = aim;
                    Program.WORLD_MAP[(uint)xyPosition.x, (uint)xyPosition.y][0].xyPosition = xyPosition;
                }
                SendMessage(
                    new MessageToClient(
                    id,
                    xyPosition,
                    true,
                    new Obj(0, 0)
                )
                );
            }
            Console.WriteLine("player {0} 's position : {1}", id.ToString(), xyPosition.ToString());
        }
        private void RecieveMessage()
        {
            while (true)
            {
                try
                {
                    int bufferLength = socket.Receive(recieveBuffer);
                    byte[] realBuffer = new Byte[bufferLength];
                    Array.Copy(recieveBuffer, 0, realBuffer, 0, bufferLength);

                    bool isSucces = false;
                    MessageToServer msgToSvr = MessageToServer.FromBytes(realBuffer, 0, out isSucces);

                    if (!isSucces)
                        continue;

                    Console.WriteLine("{0} send.", socket.RemoteEndPoint);
                    msgToSvr.senderID = id;
                    Program.messageQueue.Add(msgToSvr);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    break;
                }
            }
        }
        public void SendMessage(MessageToClient msgToClt)
        {
            try
            {
                byte[] sendByte = msgToClt.ToBytes();
                socket.Send(sendByte, sendByte.Length, 0);
            }
            catch (Exception ex)
            {
                Console.WriteLine("发送失败。");
                Console.WriteLine(ex);
                throw;
            }

        }
    }

    class Server
    {
        private IPAddress serverIP;//本机ip地址
        private IPHostEntry IpEntry;
        private IPEndPoint iPEndPoint;
        private Socket socket;
        private const int serverPort = 8888;
        private const int bufferSize = 1024;
        //private static int count = 0;//表示对话序号
        Player[] players;
        Thread serverThread;
        Thread executeMessageQueueThread;
        public Server()
        {
            Console.Write("请输入游戏人数： ");
            int playerNumber = Convert.ToInt32(Console.ReadLine());

            IpEntry = Dns.GetHostEntry(Dns.GetHostName());
            for (int i = 0; i < IpEntry.AddressList.Length; i++)
                if (IpEntry.AddressList[i].AddressFamily == AddressFamily.InterNetwork)
                {
                    Console.WriteLine("主机地址： {0}", IpEntry.AddressList[i].ToString());
                    serverIP = IPAddress.Parse(IpEntry.AddressList[i].ToString());
                }

            iPEndPoint = new IPEndPoint(serverIP, serverPort);

            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                socket.Bind(iPEndPoint);
                socket.Listen(playerNumber);
                Console.WriteLine("等待连接……");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            players = new Player[playerNumber];
            for (int i = 0; i < playerNumber; i++)
            {
                players[i] = new Player((byte)i, 50, 50, socket);
            }

            while (true)
            {
                bool isAllConnected = true;
                for (int i = 0; i < playerNumber; i++)
                    isAllConnected &= players[i].IsConnected;
                if (isAllConnected)
                    break;
            }

            serverThread = new Thread(Run);
            serverThread.Start();

            Console.WriteLine("Server constructed");
        }

        public void Run()
        {
            Program.InitializeTime();
            Console.WriteLine("Server begin to run");

            executeMessageQueueThread = new Thread(ExecuteMessageQueue);
            executeMessageQueueThread.Start();
            /*
            这里应该放定时、刷新物品等代码。
            */


            Console.WriteLine("Server stop running");
        }
        public void ExecuteMessageQueue()
        {
            Console.WriteLine("Begin to execute message queue");
            while (true)
            {
                Program.RefreshGameTime();
                Console.WriteLine("Time : " + Program.GameTime.TotalSeconds.ToString() + "s");

                MessageToServer msgToSvr = Program.messageQueue.Take();
                if (msgToSvr.commandType < 0 || msgToSvr.commandType >= COMMAND_TYPE.SIZE)
                    continue;


                if (msgToSvr.commandType == COMMAND_TYPE.MOVE && msgToSvr.parameter1 >= 0 && msgToSvr.parameter1 < (byte)DIRECTION.SIZE)
                {
                    players[msgToSvr.senderID].Move((DIRECTION)msgToSvr.parameter1);
                }
            }

        }

    }


}
