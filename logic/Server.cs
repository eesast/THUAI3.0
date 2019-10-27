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
        public static void initializeTime()
        {
            initTime = DateTime.Now;
        }
        public static DateTime getInitTime()
        {
            return initTime;
        }
        public static TimeSpan gameTime = new TimeSpan();
        public static void refreshGameTime()
        {
            gameTime = DateTime.Now - initTime;
        }
        public static TimeSpan getGameTime()
        {
            return gameTime;
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

            correctPosition();
            Program.WORLD_MAP[(uint)xyPosition.x, (uint)xyPosition.y].Add(new People(xyPosition.x, xyPosition.y));
            recieveThread = new Thread(recieveMessage);
            recieveThread.Start();

        }
        public bool checkXYPosition(XY_Position xyPos, uint width, uint height)
        {
            double halfWidth = width * 0.5;
            double halfHeight = height * 0.5;

            if (Program.WORLD_MAP[(uint)(xyPos.x + halfWidth), (uint)(xyPos.y + halfHeight)].Count > 0)
            {
                if (Program.WORLD_MAP[(uint)(xyPos.x + halfWidth), (uint)(xyPos.y + halfHeight)][0].GetType().Name == "Block")
                {
                    return false;
                }
            }
            if (Program.WORLD_MAP[(uint)(xyPos.x - halfWidth), (uint)(xyPos.y + halfHeight)].Count > 0)
            {
                if (Program.WORLD_MAP[(uint)(xyPos.x - halfWidth), (uint)(xyPos.y + halfHeight)][0].GetType().Name == "Block")
                {
                    return false;
                }
            }
            if (Program.WORLD_MAP[(uint)(xyPos.x - halfWidth), (uint)(xyPos.y - halfHeight)].Count > 0)
            {
                if (Program.WORLD_MAP[(uint)(xyPos.x - halfWidth), (uint)(xyPos.y - halfHeight)][0].GetType().Name == "Block")
                {
                    return false;
                }
            }
            if (Program.WORLD_MAP[(uint)(xyPos.x + halfWidth), (uint)(xyPos.y - halfHeight)].Count > 0)
            {
                if (Program.WORLD_MAP[(uint)(xyPos.x + halfWidth), (uint)(xyPos.y - halfHeight)][0].GetType().Name == "Block")
                {
                    return false;
                }
            }

            return true;
        }
        private void correctPosition()
        {
            if (xyPosition.x <= 0 || xyPosition.x >= WORLD_MAP_WIDTH - 1 || xyPosition.y <= 0 || xyPosition.y >= WORLD_MAP_HEIGHT - 1)
            {
                xyPosition.x = WORLD_MAP_WIDTH * 0.5;
                xyPosition.y = WORLD_MAP_HEIGHT * 0.5;
            }
            if (checkXYPosition(xyPosition, 1, 1))
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
                    if (checkXYPosition(searcher, 1, 1))
                    {
                        xyPosition = searcher;
                        return;
                    }
                }
            }
        }
        public override void move(DIRECTION direction)
        {
            Program.WORLD_MAP[(uint)xyPosition.x, (uint)xyPosition.y].RemoveAll(obj => { return obj.GetType().Name == "People"; });
            XY_Position aim = OPERATION[(uint)direction] + xyPosition;
            if (checkXYPosition(aim, 1, 1))
            {

                xyPosition = aim;
                Program.WORLD_MAP[(uint)xyPosition.x, (uint)xyPosition.y].Insert(0, new People(xyPosition.x, xyPosition.y));

                sendMessage(
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
        private void recieveMessage()
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
                    // string[] message = str.Split(messageSpiltSeperation);

                    // byte id_t = Convert.ToByte(message[0]);
                    // // if (id_t != id)
                    // //     continue;

                    // byte type = Convert.ToByte(message[1]);
                    // if (type < 0 || type >= (byte)COMMAND_TYPE.SIZE)
                    //     continue;

                    // byte param1 = Convert.ToByte(message[2]);
                    // byte param2 = Convert.ToByte(message[3]);
                    // MessageToServer msgToSvr = new MessageToServer(id, (COMMAND_TYPE)type, param1, param2);
                    Program.messageQueue.Add(msgToSvr);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    break;
                }
            }
        }
        public void sendMessage(MessageToClient msgToClt)
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

    public class Timer
    {
        int count;//总共要执行的回调数
        TimerCallback[] callback;//要执行的方法
        object[] state;//第i个回调传入的参数
        double[] dueTime;//调用第i个回调函数等待的时间，默认不等待
        double[] callbackTime;//执行第i个回调函数的时间
        public double leftTime;//距离执行回调剩余的时间
        bool TimerBreak;
        public Timer(TimerCallback[] callback_t, object[] state_t = null, double[] dueTime_t = null, int count_t = 1)
        {
            count = count_t;
            callback = callback_t;
            if (state_t != null) state = state_t;
            else state = new object[count];
            dueTime = dueTime_t;
            callbackTime = new double[count];
            TimerBreak = false;
        }
        public void Start()
        {
            Thread TimerThread = new Thread(threadFunction);
            TimerThread.Start();
        }
        public void threadFunction()
        {
            for (int i = 0; i < count; i++)
            {
                if (i == 0) callbackTime[i] = Program.gameTime.TotalSeconds + dueTime[i];
                else callbackTime[i] = callbackTime[i - 1] + dueTime[i];
                leftTime = callbackTime[i] - Program.gameTime.TotalSeconds;
                while (leftTime > 0)
                {
                    if (TimerBreak == true) break;
                    leftTime = callbackTime[i] - Program.gameTime.TotalSeconds;
                    Thread.Sleep(10);
                }
                if (TimerBreak == true) break;
                callback[i](state[i]);
            }
        }
        public void Quit()
        {
            TimerBreak = true;
        }
        public void check()
        {
            char key;
            while (true)
            {
                key = Console.ReadKey().KeyChar;
                if (key == 'q' || key == 'Q')
                {
                    Quit();
                    break;
                }
                Console.WriteLine("\b \b距离下次回调还有" + leftTime + 's');
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

            serverThread = new Thread(run);
            serverThread.Start();

            Console.WriteLine("Server constructed");
        }

        public void run()
        {
            Program.initializeTime();
            Console.WriteLine("Server begin to run");

            executeMessageQueueThread = new Thread(executeMessageQueue);
            executeMessageQueueThread.Start();



            Console.WriteLine("Server stop running");
        }
        public void executeMessageQueue()
        {
            Console.WriteLine("Begin to execute message queue");
            while (true)
            {
                Program.refreshGameTime();
                Console.WriteLine("Time : " + Program.getGameTime().TotalSeconds.ToString() + "s");

                MessageToServer msgToSvr = Program.messageQueue.Take();
                if (msgToSvr.commandType < 0 || msgToSvr.commandType >= COMMAND_TYPE.SIZE)
                    continue;


                if (msgToSvr.commandType == COMMAND_TYPE.MOVE && msgToSvr.parameter1 >= 0 && msgToSvr.parameter1 < (byte)DIRECTION.SIZE)
                {
                    players[msgToSvr.senderID].move((DIRECTION)msgToSvr.parameter1);
                }
            }

        }

    }


}