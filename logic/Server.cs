using System;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.Text;
namespace Server
{
    class Program
    {
        static void Main()
        {
            Server server = new Server();
            Thread[] threads = new Thread[server.player_number + 1];

            for (int i = 0; i < server.player_number; i++)
            {
                threads[i] = new Thread(new ParameterizedThreadStart(server.Recv_msg));
                threads[i].Start(i);
            }//每个玩家对应一个线程用来接收客户端发送的信息
            threads[server.player_number] = new Thread(new ThreadStart(server.Update));//该线程负责信息更新并发送给玩家
            threads[server.player_number].Start();

            for (int i = 0; i < server.player_number + 1; i++) threads[i].Join();
        }
    }
    public class Server
    {
        private readonly object myLock = new object();
        public int player_number;
        public byte[][] recv_buf;//用来接收玩家发送的信息
        public string send_buf;//用来向玩家发送信息
        public int myProt = 8885;   //端口 
        public Socket serverSocket;

        public Socket[] clientSocket;
        Position[] position;//玩家坐标
        MOVE_SPEED[] move_speed;//玩家移动速度
        MOVE_DIRECTION[] MoveDirection;//移动方向：上下左右
        long[] last_update_time;//上次刷新时间


        IPAddress ip;

        MAP_SIZE map_size;//地图尺寸
        bool[][] block;//用来判断碰撞的东西


        public Server()
        {
            //连接到客户端
            Console.Write("请输入游戏人数： ");
            player_number = Convert.ToInt32(Console.ReadLine());

            string hostName = Dns.GetHostName();
            IPHostEntry IpEntry = Dns.GetHostEntry(hostName);

            for (int i = 0; i < IpEntry.AddressList.Length; i++) if (IpEntry.AddressList[i].AddressFamily == AddressFamily.InterNetwork)
                { Console.WriteLine("主机地址： {0}", IpEntry.AddressList[i].ToString()); ip = IPAddress.Parse(IpEntry.AddressList[i].ToString()); }

            serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            serverSocket.Bind(new IPEndPoint(ip, myProt));  //绑定IP地址：端口 
            serverSocket.Listen(player_number);    //设定最多player_number个排队连接请求 
            Console.WriteLine("启动监听{0}成功", serverSocket.LocalEndPoint.ToString());

            clientSocket = new Socket[player_number];
            //建立连接，并将游戏人数和玩家编号发给玩家
            for (int i = 0; i < player_number; i++) { clientSocket[i] = serverSocket.Accept(); send_buf = i.ToString() + " " + player_number.ToString() + " "; Sendmsg(i); }
            //地图尺寸初始化（目前写成200*200）
            map_size.x_width = 200;
            map_size.y_width = 200;
            block = new bool[map_size.x_width][];
            for (int i = 0; i < map_size.y_width; i++) block[i] = new bool[map_size.y_width];
            //地形初始化，目前是200*200的平地，只有撞人才会被卡住
            map_init();
            //各种信息初始化
            recv_buf = new byte[player_number][];
            for (int i = 0; i < player_number; i++) recv_buf[i] = new byte[1000];
            move_speed = new MOVE_SPEED[player_number];
            for (int i = 0; i < player_number; i++) move_speed[i].speed = 5;
            position = new Position[player_number];
            position[0].x = 10; position[0].y = 10;
            MoveDirection = new MOVE_DIRECTION[player_number];
            last_update_time = new long[player_number];
        }
        public void Recv_msg(object i)//从编号为i的玩家处接收信息
        {
            int j = Convert.ToInt32(i);
            while (true)
            {
                clientSocket[j].Receive(recv_buf[j]);
                char[] str_msg = Encoding.ASCII.GetChars(recv_buf[j]);
                lock (myLock)
                {
                    if (str_msg[0] == 'u')
                    {
                        MoveDirection[j] = MOVE_DIRECTION.UP;
                        move_speed[j].y_speed = move_speed[j].speed;
                        move_speed[j].x_speed = 0;
                    }
                    else if (str_msg[0] == 'd')
                    {
                        MoveDirection[j] = MOVE_DIRECTION.DOWN;
                        move_speed[j].y_speed = -move_speed[j].speed;
                        move_speed[j].x_speed = 0;
                    }
                    else if (str_msg[0] == 'l')
                    {
                        MoveDirection[j] = MOVE_DIRECTION.LEFT;
                        move_speed[j].y_speed = 0;
                        move_speed[j].x_speed = -move_speed[j].speed;
                    }
                    else if (str_msg[0] == 'r')
                    {
                        MoveDirection[j] = MOVE_DIRECTION.RIGHT;
                        move_speed[j].y_speed = 0;
                        move_speed[j].x_speed = move_speed[j].speed;
                    }
                    else if (str_msg[0] == 'p')
                    {
                        MoveDirection[j] = MOVE_DIRECTION.STOP;
                    }
                }
            }
        }
        public void Sendmsg(int i)//将send_buf中的信息发送给编号为i的
        {
            clientSocket[i].Send(Encoding.ASCII.GetBytes(send_buf));
        }
        public void Update()//信息更新并向玩家发送
        {
            long t = 0;

            while (true)
            {
                send_buf = "";
                //位置信息更新，含判断碰撞
                for (int i = 0; i < player_number; i++)
                {
                    if (last_update_time[i] >= 0 && MoveDirection[i] != MOVE_DIRECTION.STOP)
                    {
                        t = Time();
                        int x = (int)position[i].x;
                        int y = (int)position[i].y;
                        double aim_x = position[i].x + move_speed[i].x_speed * 0.001 * (t - last_update_time[i]);
                        double aim_y = position[i].y + move_speed[i].y_speed * 0.001 * (t - last_update_time[i]);
                        if ((block[(int)aim_x][(int)aim_y] == false || (x == (int)aim_x && y == (int)aim_y)) && aim_x >= 0 && aim_y >= 0 && aim_x < map_size.x_width && aim_y < map_size.y_width)
                        {
                            position[i].x = aim_x;
                            position[i].y = aim_y;
                            if (x != (int)aim_x || y != (int)aim_y) { block[x][y] = false; block[(int)aim_x][(int)aim_y] = true; }
                        }
                        last_update_time[i] = t;
                    }
                    else { last_update_time[i] = Time(); }
                    send_buf += position[i].x.ToString() + " " + position[i].y.ToString() + " ";
                }
                Console.Clear();
                Console.WriteLine(send_buf);//打印一下发送给客户端的信息
                for (int i = 0; i < player_number; i++) Sendmsg(i);
                Thread.Sleep(30);//大约30帧的发送速率
            }
        }
        public void map_init()//地图初始化函数
        {
            for (int i = 0; i < map_size.x_width; i++)
            {
                for (int j = 0; j < map_size.y_width; j++)
                {
                    block[i][j] = false;
                }
            }
        }
        public long Time()//计时器，返回毫秒时间
        {
            return DateTime.Now.ToFileTimeUtc() / 10000;
        }
    }
    public enum MOVE_DIRECTION
    {
        STOP = 0,
        UP = 1,
        DOWN = 2,
        LEFT = 3,
        RIGHT = 4
    }
    struct MOVE_SPEED
    {
        public float speed;
        public float y_speed;
        public float x_speed;
    }
    struct Position
    {
        public double x;
        public double y;
    }
    struct MAP_SIZE
    {
        public int x_width, y_width;
    }
}
