using System;
using HPSocketCS;
using System.Net;
using System.Text;
using System.Threading;
using System.IO;
using Google.Protobuf;
using Communication;

namespace Communication.CAPI
{
    public class CAPI : ICAPI
    {
        private readonly object myLock = new object();
        int self_number, player_number;//玩家编号，全部玩家数量
        MOVE_SPEED self_move_speed;//移动速度结构体（speed为绝对速度，x_speed为x方向速度，y_speed为y方向速度，由于只能上下左右移动，xy方向速度中必有一个为0，另一个绝对值和speed相等）
        Position self_position;//自身的位置
        Position[] position;//所有玩家的位置，按照编号排序，自身位置也在其中
        MOVE_DIRECTION MoveDirection;//移动方向：上下左右
        TcpClient clientSocket;
        string[] recv_msg;//用来将一条服务器信息转为多条字符串信息
        string send_buf;//发送信息专用
        string last_order;//上次发送的信息（防止客户端不断发送同样信息造成麻烦）
        bool initialized = false;
        public long Time()//计时器，返回毫秒时间
        {
            return DateTime.Now.ToFileTimeUtc() / 10000;
        }
        private HandleResult OnReceive(IClient sender, byte[] bytes)
        {
            Console.WriteLine("data received");
            PacketType type = (PacketType) BitConverter.ToInt32(bytes);
            switch (type)
            {
                case PacketType.ServerPacket:
                    ServerPacket packet = ServerPacket.Parser.ParseFrom(new MemoryStream(bytes, 4, bytes.Length - 4));
                //信息初始化
                    if (!initialized)
                    {
                        recv_msg = packet.Data.Split(' ');
                        self_number = Convert.ToInt32(recv_msg[0]);
                        player_number = Convert.ToInt32(recv_msg[1]);
                        self_move_speed.speed = 5;
                        self_move_speed.x_speed = 0;
                        self_move_speed.y_speed = 0;
                        self_position.x = self_position.y = -1;
                        position = new Position[player_number];
                        MoveDirection = MOVE_DIRECTION.STOP;
                        initialized = true;
                    }
                    else
                    {
                        recv_msg = packet.Data.Split(' ');
                        Update();
                    }
                    break;
            }
            return new HandleResult();
        }
        public void ConnectServer(IPEndPoint endPoint)
        {
            clientSocket = new TcpPackClient();
            clientSocket.OnReceive += OnReceive;
            clientSocket.OnClose += TryReconnect;
            while (!clientSocket.Connect(endPoint.Address.ToString(), (ushort) endPoint.Port))
            {
                Console.WriteLine("连接失败");
                Thread.Sleep(1000);
            }
            Console.WriteLine("client connected");
            //网络连接完毕

        }
        private HandleResult TryReconnect(IClient sender, SocketOperation enOperation, int errcode)
        {
            clientSocket = new TcpPackClient();
            while (!clientSocket.Connect("127.0.0.1", 9999, false))
            {
                Console.WriteLine("重连失败");
                Thread.Sleep(1000);
            }
            Console.WriteLine("重连成功");
            return HandleResult.Ok;
        }

        public void Sendmsg()//将send_buf中的信息发送给服务器
        {
            Console.WriteLine("data sent");
            if (send_buf != last_order)
            {
                ClientPacket packet = new ClientPacket();
                packet.Data = send_buf;
                MemoryStream ms = new MemoryStream();
                BinaryWriter bw = new BinaryWriter(ms);
                bw.Write((int)PacketType.ClientPacket);
                packet.WriteTo(ms);
                byte[] raw = ms.ToArray();
                clientSocket.Send(raw, raw.Length);
                last_order = send_buf;
            }
        }
        public void Move(MOVE_DIRECTION md)//移动指令，参数为移动方向
        {
            lock (myLock)
            {
                MoveDirection = md;
                //TODO: use switch case instead.
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
        private void Update()//从服务器接收信息并更新信息
        {
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
        public int MyPlayer()
        {
            return self_number;
        }

        public void GetPos(int client, out double x, out double y)
        {
            x = position[client].x;
            y = position[client].y;
        }

        public int PlayerCount()
        {
            return player_number;
        }

        public bool isConnected()
        {
            return initialized;
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
        public double x, y;
    }
}