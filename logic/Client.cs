using System;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.Text;
using Constant;
using System.Diagnostics;
using static Constant.CONSTANT;
namespace Client
{
    class Program
    {
        public static void Main(string[] args)

        {
            Player player = new Player(10, 10);
        }
    }

    class Player : Character
    {
        private static IPAddress serverIP;
        private static IPEndPoint iPEndPoint;
        private static int port = 8888;
        private static int bufferSize = 1024;
        static byte[] recieveBuffer = new Byte[bufferSize];
        static IPAddress clientIP;
        static Socket socket;
        static Thread recieveThread;
        static Thread operationThread;
        public static DateTime lastSendTime = new DateTime();
        public Player(double x, double y)
        :
        base(0, x, y)
        {
            Console.Write("请输入主机地址： ");
            serverIP = IPAddress.Parse(Console.ReadLine());

            iPEndPoint = new IPEndPoint(serverIP, port);

            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                socket.Connect(iPEndPoint);
                Console.WriteLine("成功连接到{0}。", socket.RemoteEndPoint);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
            recieveThread = new Thread(recieveMessage);
            recieveThread.Start();
            operationThread = new Thread(operation);
            operationThread.Start();
        }
        private void operation()
        {
            TimeSpan deltaTime = DateTime.Now - lastSendTime;
            if (deltaTime.TotalSeconds <= TIME_INTERVAL)
                return;
            char key;
            while (true)
            {
                key = Console.ReadKey().KeyChar;
                if (key == 'w') move(DIRECTION.UP);
                else if (key == 's') move(DIRECTION.DOWN);
                else if (key == 'a') move(DIRECTION.LEFT);
                else if (key == 'd') move(DIRECTION.RIGHT);
                lastSendTime = DateTime.Now;
            }
        }
        private void recieveMessage()
        {
            while (true)
            {
                try
                {
                    int length = socket.Receive(recieveBuffer);
                    byte[] realBuffer = new Byte[length];
                    Array.Copy(recieveBuffer, 0, realBuffer, 0, length);
                    string str = System.Text.Encoding.Default.GetString(realBuffer);
                    //Console.WriteLine("{0} : {1}.", socket.RemoteEndPoint, str);

                    string[] message = str.Split(messageSpiltSeperation);

                    uint id_t = Convert.ToUInt32(message[0]);
                    // if (id_t != id)
                    //     continue;

                    xyPosition.x = Convert.ToDouble(message[1]);
                    xyPosition.y = Convert.ToDouble(message[2]);

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    //Console.ReadKey();
                    break;
                }
                Console.Clear();
                Console.WriteLine("position : " + xyPosition.ToString());
            }
        }

        public void sendMessage(MessageToServer msgToSvr)
        {
            try
            {
                byte[] sendByte = Encoding.Default.GetBytes(msgToSvr.ToString());
                socket.Send(sendByte, sendByte.Length, 0);
            }
            catch (Exception ex)
            {
                Console.WriteLine("发送失败。");
                Console.WriteLine(ex);
                throw;
            }

        }
        public override void move(DIRECTION direction)
        {
            sendMessage(
                new MessageToServer
                (
                id,
                COMMAND_TYPE.MOVE,
                (byte)direction,
                0
            )
            );
        }

    }


}
