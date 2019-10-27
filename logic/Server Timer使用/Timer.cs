using System;
using System.Threading;
namespace CSharp测试
{
    class Program
    {
        static void Main()
        {
            Thread timeUpdate = new Thread(func);
            timeUpdate.Start();
            //定时器使用演示
            //定时器可执行多个委托
            int count = 100;//回调函数的总数
            TimerCallback[] callback = new TimerCallback[count];//定义回调函数
            object[] state = new object[count];//回调函数的参数
            double[] dueTime = new double[count];//两次回调函数执行的间隔
            for (int i = 0; i < count; i++)
            {
                dueTime[i] = 1;//一秒执行一次
                callback[i] = new TimerCallback(getGameTime);//委托设定为读时间，委托需要void返回值和object参数
            }
            Timer myTimer=new Timer(callback,state,dueTime, count); //用上面的参数初始化一个计时器
            Thread check = new Thread(myTimer.check);//检查线程，按Q可以强制计时器停止，按其他键检测到下一次回调的时间
            myTimer.Start();//计时器启动，回调函数等待的时间也是从该语句执行的时间开始计算的
            check.Start();//检查函数启动

        }
        private static DateTime initTime = new DateTime();
        public static bool run = true;
        public static void func()
        {
            initializeTime();
            while (run)
            {
                refreshGameTime();
            }
        }
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
        public static void getGameTime(object a = null)
        {
            Console.WriteLine(gameTime);
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
            while(true)
            {
                key=Console.ReadKey().KeyChar;
                if (key == 'q' || key == 'Q')
                {
                    Quit();
                    Program.run = false;
                    break;
                }
                Console.WriteLine("\b \b距离下次回调还有"+leftTime+'s');
            }
        }
    }
}
