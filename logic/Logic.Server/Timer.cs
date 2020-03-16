using System;
using System.Collections.Generic;
using System.Threading;
//#pragma warning disable CS8622
namespace Timer
{
    public static class Time
    {
        public static DateTime initTime = new DateTime();
        public static bool run = true;
        public static void InitializeTime()
        {
            initTime = DateTime.Now;
        }
        public static TimeSpan GameTime()
        {
            return DateTime.Now - initTime;
        }
    }
    public class MultiTaskTimer
    {
        private readonly object privateLock = new object();
        private readonly SortedDictionary<DateTime, System.Timers.ElapsedEventHandler> dict = new SortedDictionary<DateTime, System.Timers.ElapsedEventHandler>();
        private readonly System.Timers.Timer timer = new System.Timers.Timer();
        private System.Timers.ElapsedEventHandler _control;
        private System.Timers.ElapsedEventHandler Control
        {
            get
            {
                _control = _control ?? new System.Timers.ElapsedEventHandler(
                    (o, e) =>
                    {
                        lock (privateLock)
                        {
                            //Console.WriteLine("Entering Control");
                            SortedDictionary<DateTime, System.Timers.ElapsedEventHandler>.Enumerator en = dict.GetEnumerator();
                            en.MoveNext();
                            dict.Remove(en.Current.Key);
                            timer.Elapsed -= en.Current.Value;
                            timer.Elapsed -= Control;
                            if (dict.Count == 0)
                            {
                                //Console.WriteLine("function Control : Count == 0");
                                return;
                            }
                            en = dict.GetEnumerator();
                            en.MoveNext();
                            timer.Elapsed += en.Current.Value;
                            timer.Elapsed += Control;
                            double milliseconds = (en.Current.Key - DateTime.Now).TotalMilliseconds;
                            if (milliseconds <= 0)
                            {
                                milliseconds = 0.0001;
                            }
                            timer.Interval = milliseconds;
                            timer.Enabled = true;
                            //Console.WriteLine("Exit Control");
                        }
                    });
                return _control;
            }
        }
        public MultiTaskTimer()
        {
            timer.AutoReset = false;
        }
        public void Add(System.Timers.ElapsedEventHandler function, uint milliseconds)
        {
            lock (privateLock)
            {
                //Console.WriteLine("Entering Add");
                if (dict.Count == 0)
                {
                    //Console.WriteLine("function Add : Count == 0");
                    dict.Add(DateTime.Now + TimeSpan.FromMilliseconds(milliseconds), function);
                    timer.Elapsed += function;
                    timer.Elapsed += Control;
                    timer.Interval = milliseconds;
                    timer.Enabled = true;
                    //Console.WriteLine("Exit Add");
                    return;
                }
                timer.Enabled = false;
                //Console.WriteLine("function Add : Count != 0");
                DateTime dateTime = DateTime.Now + TimeSpan.FromMilliseconds(milliseconds);

                SortedDictionary<DateTime, System.Timers.ElapsedEventHandler>.Enumerator en = dict.GetEnumerator();
                en.MoveNext();
                if (dateTime >= en.Current.Key)
                {
                    dict.Add(dateTime, function);
                    //Console.WriteLine(en.Current.Key);
                    //Console.WriteLine(DateTime.Now);
                    double ms = (en.Current.Key - DateTime.Now).TotalMilliseconds;
                    if (ms <= 0)
                        ms = 0.00011;
                    timer.Interval = ms;
                    timer.Enabled = true;
                    //Console.WriteLine("Exit Add");
                    return;
                }
                //Console.WriteLine("Add to first");
                timer.Elapsed -= en.Current.Value;
                timer.Elapsed -= Control;
                dict.Add(dateTime, function);
                timer.Elapsed += function;
                timer.Elapsed += Control;
                timer.Interval = milliseconds;
                timer.Enabled = true;
                //Console.WriteLine("Exit Add");
            }
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

        public Timers(TimerCallback[] callback_t, object[] state_t, double[] dueTime_t, int count_t)
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
                endTime = Time.GameTime() + TimeSpan.FromSeconds(dueTime[i]);
                timer[i] = new System.Threading.Timer(callback[i], state[i], TimeSpan.FromSeconds(dueTime[i]), TimeSpan.FromSeconds(0));
                Thread.Sleep(TimeSpan.FromSeconds(dueTime[i]));
            }
        }
        public void threadFunctionType2()//上一个计时器的回调函数执行完之后再开始下一个计时器的计时
        {
            void func(object k)
            {
                int i = Convert.ToInt32(k);
                callback[i](state[i]);
                if (i != count - 1)
                {
                    endTime = Time.GameTime() + TimeSpan.FromSeconds(dueTime[(int)k + 1]); flag++;

                    timer[i + 1] = new System.Threading.Timer(new TimerCallback(func), (int)k + 1, TimeSpan.FromSeconds(dueTime[i + 1]), TimeSpan.FromSeconds(0));

                }
            }
            endTime = Time.GameTime() + TimeSpan.FromSeconds(dueTime[0]); flag++;
            timer[0] = new System.Threading.Timer(new TimerCallback(func), 0, TimeSpan.FromSeconds(dueTime[0]), TimeSpan.FromSeconds(0));
        }
        public void Quit()//退出计时器，且停止当前正在运行的计时器
        {
            TimerBreak = true;
            timer[flag].Dispose();
        }
        public TimeSpan getLeftTime()//输出并返回下一个回调函数开始执行的时间
        {
            if (Time.GameTime() < endTime) Console.WriteLine("\b \b距离下次回调还有" + (endTime - Time.GameTime()) + " 当前正在进行第" + flag + "个委托");
            else Console.WriteLine("已经全部执行");
            return (endTime - Time.GameTime());
        }
        public void check()//手动检查，按q退出计时器，其他键调用getLeftTime
        {
            char key;
            while (true)
            {
                key = Console.ReadKey().KeyChar;
                if (key == 'q' || key == 'Q')
                {
                    Quit();
                    //Program.run = false;
                    Console.WriteLine("定时器停止运行！");
                    break;
                }
                getLeftTime();
            }
        }
    }
}