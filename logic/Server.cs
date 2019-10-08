using System;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.Text;
using Constant;
using static Constant.CONSTANT;
using System.Collections.Generic;


namespace Server
{
    static class Program
    {
        public static Queue<MessageToServer> messageQueue = new Queue<MessageToServer>();
        public static MAP_CELL[,] WORLD_MAP = new MAP_CELL[WORLD_MAP_WIDTH, WORLD_MAP_HEIGHT];
        public static DateTime initTime = new DateTime();
        public static TimeSpan gameTime = new TimeSpan();
        public static void Main(string[] args)
        {
            for (int i = 0; i < WORLD_MAP_WIDTH; i++)
                for (int j = 0; j < WORLD_MAP_HEIGHT; j++)
                {
                    if (
                        (i >= 50 && i <= 150 && j >= 100 && j <= 200) ||
                        i == 0 ||
                        i == WORLD_MAP_WIDTH - 1 ||
                        j == 0 ||
                        j == WORLD_MAP_HEIGHT - 1
                        )
                    {

                        WORLD_MAP[i, j] = new MAP_CELL(OBJECT_TYPE.BLOCK, (byte)BLOCK_TYPE.WALL);
                    }
                    else
                    {
                        WORLD_MAP[i, j] = new MAP_CELL(OBJECT_TYPE.AIR, 0);
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
        public Player(uint id_t, double x, double y, Socket serverSocket)
        :
        base(id_t, x, y)
        {
            socket = serverSocket.Accept();
            Console.WriteLine("{0} has connected to local.", socket.RemoteEndPoint);
            isConnected = true;

            correctPosition();
            Program.WORLD_MAP[(uint)xyPosition.x, (uint)xyPosition.y] = new MAP_CELL(OBJECT_TYPE.PEOPLE, 0);
            recieveThread = new Thread(recieveMessage);
            recieveThread.Start();

        }
        public bool checkXYPosition(XY_Position xyPos, uint width, uint height)
        {
            double halfWidth = width * 0.5;
            double halfHeight = height * 0.5;
            if (
                Program.WORLD_MAP[(uint)(xyPos.x + halfWidth), (uint)(xyPos.y + halfHeight)].objectType != OBJECT_TYPE.BLOCK &&
                Program.WORLD_MAP[(uint)(xyPos.x - halfWidth), (uint)(xyPos.y + halfHeight)].objectType != OBJECT_TYPE.BLOCK &&
                Program.WORLD_MAP[(uint)(xyPos.x - halfWidth), (uint)(xyPos.y - halfHeight)].objectType != OBJECT_TYPE.BLOCK &&
                Program.WORLD_MAP[(uint)(xyPos.x + halfWidth), (uint)(xyPos.y - halfHeight)].objectType != OBJECT_TYPE.BLOCK
            )
                return true;
            else
                return false;
        }
        private void correctPosition()
        {
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
            Program.WORLD_MAP[(uint)xyPosition.x, (uint)xyPosition.y] = new MAP_CELL(OBJECT_TYPE.AIR, 0);
            XY_Position aim = OPERATION[(uint)direction] + xyPosition;
            if (checkXYPosition(aim, 1, 1))
            {

                xyPosition = aim;
                Program.WORLD_MAP[(uint)xyPosition.x, (uint)xyPosition.y] = new MAP_CELL(OBJECT_TYPE.PEOPLE, 0);

                sendMessage(
                    new MessageToClient(
                    id,
                    xyPosition,
                    xyPosition,
                    xyPosition,
                    Program.WORLD_MAP[(uint)xyPosition.x, (uint)xyPosition.y]
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
                    string str = Encoding.Default.GetString(realBuffer);
                    Console.WriteLine("{0} : {1}.", socket.RemoteEndPoint, str);
                    string[] message = str.Split(messageSpiltSeperation);

                    UInt32 id_t = Convert.ToUInt32(message[0]);
                    // if (id_t != id)
                    //     continue;

                    byte type = Convert.ToByte(message[1]);
                    if (type < 0 || type >= (byte)COMMAND_TYPE.SIZE)
                        continue;

                    byte param1 = Convert.ToByte(message[2]);
                    byte param2 = Convert.ToByte(message[3]);
                    MessageToServer msgToSvr = new MessageToServer(id, (COMMAND_TYPE)type, param1, param2);
                    Program.messageQueue.Enqueue(msgToSvr);
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
                byte[] sendByte = Encoding.Default.GetBytes(msgToClt.ToString());
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
                players[i] = new Player((uint)i, 100, 150, socket);
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
        }

        public void run()
        {
            Program.initTime = DateTime.Now;

            while (true)
            {
                if (Program.messageQueue.Count == 0)
                    continue;
                Program.gameTime = DateTime.Now - Program.initTime;
                Console.WriteLine("Time : " + Program.gameTime.TotalSeconds.ToString() + "s");

                MessageToServer msgToSvr = Program.messageQueue.Dequeue();
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
