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
                callback[i] = new TimerCallback(f);//委托设定为读时间，委托需要void返回值和object参数
            }
            Timers myTimer=new Timers(callback,state,dueTime, count); //用上面的参数初始化一个计时器
            Thread check = new Thread(myTimer.check);//检查线程，按Q可以强制计时器停止，按其他键检测到下一次回调的时间

            myTimer.Start_1();//计时器启动，前一个计时器的回调函数开始执行时即开始下一个计时器的计时
            myTimer.Start_2();//计时器启动，前一个计时器的回调函数执行结束之后再开始下一个计时器的计时

            check.Start();//检查函数启动

        }
        private static DateTime initTime = new DateTime();
        public static bool run = true;
        public static void f(object j)
        {
            Console.WriteLine(10);
            Thread.Sleep(1000);
        }
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
    public class Timers
    {
        int count;//总共要执行的回调数

        TimerCallback[] callback;//要执行的方法
        object[] state;//第i个回调传入的参数
        double[] dueTime;//调用第i个回调函数等待的时间，默认不等待

        public TimeSpan endTime;//最晚开始的计时器结束时间
        public int flag;//正在执行的timer编号，-1表示还没开始
        System.Threading.Timer[] timer;
        bool TimerBreak;

        public Timers(TimerCallback[] callback_t, object[] state_t = null, double[] dueTime_t = null, int count_t = 1)
        {
            count = count_t;
            callback = callback_t;
            if (state_t != null) state = state_t;
            else state = new object[count];
            dueTime = dueTime_t;
            TimerBreak = false;
            timer = new System.Threading.Timer[count];
            flag = -1;
        }
        public void Start_1()//调用threadFunctionType1
        {
            Thread TimerThread = new Thread(threadFunctionType1);
            TimerThread.Start();
        }
        public void Start_2()//调用threadFunctionType2
        {
            Thread TimerThread = new Thread(threadFunctionType2);
            TimerThread.Start();
        }
        public void threadFunctionType1()//该函数使得各回调函数的开始时间间隔保持一致，而非一个执行完另一个再开始计时
        {
            for (int i = 0; i < count; i++)
            {
                if (TimerBreak == true) { break; }
                flag++;
                endTime = Program.gameTime + TimeSpan.FromSeconds(dueTime[i]);
                timer[i] = new System.Threading.Timer(callback[i], state[i], TimeSpan.FromSeconds(dueTime[i]), TimeSpan.FromSeconds(0));
                Thread.Sleep(TimeSpan.FromSeconds(dueTime[i]));
            }
        }
        public void threadFunctionType2(object j)//上一个计时器的回调函数执行完之后再开始下一个计时器的计时
        {
            void func(object k)
            {
                int i = Convert.ToInt32(k);
                callback[i](state[i]);
                if (i != count - 1)
                {
                    endTime = Program.gameTime + TimeSpan.FromSeconds(dueTime[(int)k + 1]);flag++;
                    timer[i + 1] = new System.Threading.Timer(new TimerCallback(func), (int)k + 1, TimeSpan.FromSeconds(dueTime[i + 1]), TimeSpan.FromSeconds(0));
                }
            }
            endTime = Program.gameTime + TimeSpan.FromSeconds(dueTime[0]);flag++;
            timer[0] = new System.Threading.Timer(new TimerCallback(func), 0, TimeSpan.FromSeconds(dueTime[0]), TimeSpan.FromSeconds(0));
        }
        public void Quit()//退出计时器，且停止当前正在运行的计时器
        {
            TimerBreak = true;
            timer[flag].Dispose();
        }
        public TimeSpan getLeftTime()//输出并返回下一个回调函数开始执行的时间
        {
            if (Program.gameTime < endTime) Console.WriteLine("\b \b距离下次回调还有" + (endTime - Program.gameTime) + " 当前正在进行第" + flag + "个委托");
            else Console.WriteLine("已经全部执行");
            return (endTime - Program.gameTime);
        }
        public void check()//手动检查，按q退出计时器，其他键调用getLeftTime
        {
            char key;
            while(true)
            {
                key=Console.ReadKey().KeyChar;
                if (key == 'q' || key == 'Q')
                {
                    Quit();
                    //Program.run = false;
                    Console.WriteLine("\b \b定时器停止运行！");
                    break;
                }
                getLeftTime();
            }
        }
    }
}
