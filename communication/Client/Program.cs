using Communication.CAPI;
using System;
using System.Net;
using System.Threading;

namespace Client
{
    public class Program
    {
        private static byte[] result = new byte[1024];
        private static ICAPI CAPI = new CAPI();
        public static void Operation()//人工操作，wasd控制移动
        {
            while (!CAPI.isConnected()) ;
            char key;
            while (true)
            {
                CAPI.Stop();
                key = Console.ReadKey().KeyChar;
                if (key == 'w') CAPI.Move(MOVE_DIRECTION.UP);
                else if (key == 's') CAPI.Move(MOVE_DIRECTION.DOWN);
                else if (key == 'a') CAPI.Move(MOVE_DIRECTION.LEFT);
                else if (key == 'd') CAPI.Move(MOVE_DIRECTION.RIGHT);
                Thread.Sleep(25);
            }
        }
        public static void ScreenRefresh()//控制台显示,大约30帧左右
        {
            while (!CAPI.isConnected()) ;
            while (true)
            {
                lock (CAPI)
                {
                    for (int i = 0; i < CAPI.PlayerCount(); i++)
                    {
                        double x, y;
                        CAPI.GetPos(i, out x, out y);
                        if (i != CAPI.MyPlayer()) Console.WriteLine("player {0} pos :({1},{2})", x, y);
                        else Console.WriteLine("your pos :({0},{1})", x, y);
                    }
                }
                Thread.Sleep(30);
            }
        }

        public static void Main(string[] args)
        {
            new Thread(new ThreadStart(Operation)).Start();
            new Thread(new ThreadStart(ScreenRefresh)).Start();
            CAPI.ConnectServer(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 9999));
        }
    }
}
