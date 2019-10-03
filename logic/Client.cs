using System;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.Text;
namespace Client
{
    class Program
    {
        private static byte[] result = new byte[1024];
        
        static void Main()
        {
            Player player = new Player();
            Thread[] threads = new Thread[3];
            threads[0] = new Thread(new ThreadStart(player.Update));//该线程负责从服务器接收信息
            threads[1] = new Thread(new ThreadStart(player.Operation));//人工操作线程，向服务器发送信息
            threads[2]= new Thread(new ThreadStart(player.ScreenRefresh));
            threads[0].Start();
            threads[1].Start();
            threads[2].Start();
            threads[0].Join();
            threads[1].Join();
            threads[2].Join();
        }
    }
    public class Player
    {
        private readonly object myLock = new object();
        int self_number, player_number;//玩家编号，全部玩家数量
        MOVE_SPEED self_move_speed;//移动速度结构体（speed为绝对速度，x_speed为x方向速度，y_speed为y方向速度，由于只能上下左右移动，xy方向速度中必有一个为0，另一个绝对值和speed相等）
        Position self_position;//自身的位置
        Position[] position;//所有玩家的位置，按照编号排序，自身位置也在其中
        MOVE_DIRECTION MoveDirection;//移动方向：上下左右
        IPAddress ip;//服务器地址
        Socket clientSocket;
        byte[] recv_buf = new byte[1000];//接收服务器信息专用
        string[] recv_msg;//用来将一条服务器信息转为多条字符串信息
        string send_buf;//发送信息专用
        string last_order;//上次发送的信息（防止客户端不断发送同样信息造成麻烦）
        public long Time()//计时器，返回毫秒时间
        {
            return DateTime.Now.ToFileTimeUtc() / 10000;
        }
        public Player()
        {
            //和服务器网络连接
            Console.Write("请输入主机地址： ");
            string ipadd = Console.ReadLine();
            ip = IPAddress.Parse(ipadd);
            clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                clientSocket.Connect(new IPEndPoint(ip, 8885)); //配置服务器IP与端口 
                clientSocket.Receive(recv_buf);
                recv_msg = Encoding.ASCII.GetString(recv_buf).Split(' ');
                self_number = Convert.ToInt32(recv_msg[0]);
                player_number = Convert.ToInt32(recv_msg[1]);
                Console.WriteLine("连接服务器成功");
            }
            catch
            {
                Console.WriteLine("连接服务器失败，请按回车键退出！");
                return;
            }
            //网络连接完毕
            //信息初始化
            self_move_speed.speed = 5;
            self_move_speed.x_speed = 0;
            self_move_speed.y_speed = 0;
            self_position.x = self_position.y = -1;
            position = new Position[player_number];
            MoveDirection = MOVE_DIRECTION.STOP;
        }
        public void Sendmsg()//将send_buf中的信息发送给服务器
        {
            if (send_buf != last_order) { clientSocket.Send(Encoding.ASCII.GetBytes(send_buf));last_order = send_buf; }
        }
        public void Move(MOVE_DIRECTION md)//移动指令，参数为移动方向
        {
            lock (myLock)
            {
                MoveDirection = md;
                if (md == MOVE_DIRECTION.UP)
                {
                    send_buf = "u";
                    Sendmsg();
                }
                else if (md == MOVE_DIRECTION.DOWN)
                {
                    send_buf = "d";
                    Sendmsg();
                }
                else if (md == MOVE_DIRECTION.LEFT)
                {
                    send_buf = "l";
                    Sendmsg();
                }
                else if (md == MOVE_DIRECTION.RIGHT)
                {
                    send_buf = "r";
                    Sendmsg();
                }
            }
        }
        public void Stop()//停止指令
        {
            MoveDirection = MOVE_DIRECTION.STOP;
            send_buf = "p";
            Sendmsg();
        }
        public void Update()//从服务器接收信息并更新信息
        {
            while (true)
            {
                clientSocket.Receive(recv_buf);
                recv_msg = Encoding.ASCII.GetString(recv_buf).Split(' ');
                int j = 0;
                for (int i = 0; i < player_number; i++)
                {
                    lock (myLock)
                    {
                        position[i].x = Convert.ToDouble(recv_msg[j]); j++;
                        position[i].y = Convert.ToDouble(recv_msg[j]); j++;
                        if (i == self_number) self_position = position[i];
                    }
                }
            }
        }
        public void ScreenRefresh()//控制台显示,大约30帧左右
        {
            while (true)
            {
                Console.Clear();
                lock (myLock)
                {
                    for (int i = 0; i < player_number; i++)
                    {
                        if (i != self_number) Console.WriteLine("player {0} pos :({1},{2})", i, position[i].x, position[i].y);
                        else Console.WriteLine("your pos :({0},{1})", position[i].x, position[i].y);
                    }
                }
                Thread.Sleep(30);
            }
        }
        public void Operation()//人工操作，wasd控制移动
        {
            char key;
            while (true)
            {
                Stop();
                key = Console.ReadKey().KeyChar;
                if (key == 'w') Move(MOVE_DIRECTION.UP);
                else if (key == 's') Move(MOVE_DIRECTION.DOWN);
                else if (key == 'a') Move(MOVE_DIRECTION.LEFT);
                else if (key == 'd') Move(MOVE_DIRECTION.RIGHT);
                Thread.Sleep(25);
            }
        }
    }
    public enum MOVE_DIRECTION
    {
        STOP=0,
        UP =1,
        DOWN=2,
        LEFT=3,
        RIGHT=4
    }
    struct MOVE_SPEED
    {
        public float speed;
        public float y_speed;
        public float x_speed;
    }
    struct Position
    {
        public double x, y;
    }
}
