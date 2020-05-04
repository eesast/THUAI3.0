using System;
using System.Collections.Concurrent;
using Logic.Constant;
using static Logic.Constant.Constant;
using Communication.Proto;

namespace Logic.Server
{
    public static class TaskSystem
    {
        public static ConcurrentDictionary<DishType, uint> TaskQueue = new ConcurrentDictionary<DishType, uint>();
        private static int SpicedPotNum = 2;
        private static Timer.MultiTaskTimer? _removeTaskTimer;
        public static Timer.MultiTaskTimer RemoveTaskTimer
        {
            get
            {
                _removeTaskTimer = _removeTaskTimer ?? new Timer.MultiTaskTimer();
                return _removeTaskTimer;
            }
        }
        private static void AddTask(DishType task)
        {
            if (!TaskQueue.ContainsKey(task))
                TaskQueue.TryAdd(task, 0);
            TaskQueue[task]++;
            Program.MessageToClient.Tasks.Add(task);
            Server.ServerDebug("Add task : " + task);
            //PrintAllTask();
        }
        private static void RemoveTask(DishType task)
        {
            TaskQueue[task]--;
            if (TaskQueue[task] <= 0)
            {
                uint i = 0;
                TaskQueue.TryRemove(task, out i);
            }
            Program.MessageToClient.Tasks.Remove(task);
            Server.ServerDebug("Remove task : " + task);
            //PrintAllTask();
        }
        public static System.Threading.Timer? _refreshTimer;
        public static System.Threading.Timer RefreshTimer
        {
            get
            {
                _refreshTimer = _refreshTimer ?? new System.Threading.Timer(TaskProduce);
                return _refreshTimer;
            }
        }
        public static void TaskProduce(object? i)
        {
            DishType temp = DishType.DishEmpty;
            for (; ; )
            {
                if (Timer.Time.GameTime() > TimeSpan.FromMinutes(5) && SpicedPotNum > 0)
                { 
                    temp = (DishType)Program.Random.Next((int)DishType.CookedRice, (int)DishType.SpicedPot + 1); 
                    if(temp== DishType.SpicedPot) SpicedPotNum--; 
                }
                else temp = (DishType)Program.Random.Next((int)DishType.CookedRice, (int)DishType.SpicedPot); 
                if (temp == DishType.DishSize1)
                    continue;
                break;
            }
            AddTask(temp);
            RemoveTaskTimer.Add((o, e) => { RemoveTask(temp); }, (uint)Configs(temp.ToString(), "TaskTime"));
            //需要广播产生的任务
            //感觉只需要广播任务的产生，而任务被完成以及任务因过时而gg都不用广播，需要玩家自己把握？
        }
        public static int HandIn(DishType dish_t)
        {
            int score = 0;
            if (dish_t < DishType.SpicedPot && TaskQueue.ContainsKey(dish_t))
            {
                score = (int)Configs(dish_t.ToString(), "Score");//菜品名+Score，在App.config里加
                RemoveTask(dish_t);
            }
            else if (dish_t >= DishType.SpicedPot && dish_t <= DishType.SpicedPot_6 && TaskQueue.ContainsKey(DishType.SpicedPot))
            {
                string[] i = dish_t.ToString().Split('_');
                double temp = Convert.ToDouble(i[1]);
                score = (int)((1 + temp / 10) * temp * 15);
                RemoveTask(DishType.SpicedPot);
            }
            //PrintAllTask();
            return score;
        }
        public static void PrintAllTask()
        {
            Server.ServerDebug("Tasks : ");
            foreach (var task in TaskQueue)
            {
                Server.ServerDebug("\t" + task);
            }
        }
    }
}
