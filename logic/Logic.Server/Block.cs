using Communication.Proto;
using Logic.Constant;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using static Logic.Constant.Constant;
using static Logic.Constant.MapInfo;

namespace Logic.Server
{
    public class Block : Obj
    {
        public int RefreshTime;//食物刷新点的食物刷新速率，毫秒

        public Block(double x_t, double y_t, BlockType type_t) : base(x_t, y_t, ObjType.Block)
        {
            if (type_t == BlockType.Wall || type_t == BlockType.FoodPoint || type_t == BlockType.TaskPoint)
                Layer = (int)MapLayer.WallLayer;
            else
                Layer = (int)MapLayer.BlockLayer;
            Movable = false;
            blockType = type_t;
            Movable = false;

            switch (blockType)
            {
                case BlockType.FoodPoint:
                    AddToMessage();
                    Dish = (DishType)Program.Random.Next(1, (int)DishType.Size1 - 1);
                    lock (Program.MessageToClientLock)
                        Program.MessageToClient.GameObjectMessageList[ID].BlockType = BlockTypeMessage.FoodPoint;
                    RefreshTime = (int)Configs["FoodPointInitRefreshTime"];
                    Console.WriteLine("食品刷新：地点（" + Position.x + "," + Position.y + "）, 种类 : " + Dish);
                    break;
                case BlockType.Cooker:
                    AddToMessage();
                    Dish = DishType.Empty;
                    lock (Program.MessageToClientLock)
                        Program.MessageToClient.GameObjectMessageList[ID].BlockType = BlockTypeMessage.Cooker;
                    break;
            }
        }
        public override DishType GetDish(DishType t)
        {
            DishType temp = Dish;
            Dish = DishType.Empty;
            switch (blockType)
            {
                case BlockType.FoodPoint:
                    RefreshTimer.Change(RefreshTime, 0);
                    break;
                case BlockType.Cooker:
                    cookingResult = "Empty";
                    Cooking = false;
                    ProtectTimer.Change(0, 0);
                    break;
            }
            return temp;
        }

        //Refresh
        protected System.Threading.Timer _refreshTimer;
        public System.Threading.Timer RefreshTimer
        {
            get
            {
                if (this.blockType == BlockType.FoodPoint) _refreshTimer = _refreshTimer ?? new System.Threading.Timer(Refresh);
                return _refreshTimer;
            }
        }
        public void Refresh(object i)
        {
            Dish = (DishType)Program.Random.Next(1, (int)DishType.Size1 - 1);
            Console.WriteLine("食品刷新：地点（" + Position.x + "," + Position.y + "）, 种类 : " + Dish);
        }
        //Refresh End

        //Cook
        protected System.Threading.Timer _cookingTimer;
        protected System.Threading.Timer CookingTimer
        {
            get
            {
                _cookingTimer = _cookingTimer ?? new System.Threading.Timer(Cook);
                return _cookingTimer;
            }
        }
        protected void Cook(object o)
        {
            Dish = (DishType)Enum.Parse(typeof(DishType), cookingResult);
            if (Dish > DishType.Empty && Dish < DishType.Size2 && Dish != DishType.Size1)
            {
                if((int)Dish<(int)DishType.XiangGuo) CookingTimer.Change((int)(0.5*(double)Configs[cookingResult]["CookTime"]), 0);
                else CookingTimer.Change((int)(0.5 * (double)Configs["XiangGuo"]["CookTime"]), 0);
                cookingResult = "OverCookedDish";
            }
        }
        protected string cookingResult;
        public bool Cooking = false;

        public int ProtectedTeam = -1;
        protected System.Threading.Timer _protectTimer;
        public System.Threading.Timer ProtectTimer
        {
            get
            {
                _protectTimer = _protectTimer ?? new System.Threading.Timer((i)=> { ProtectedTeam = -1; });
                return _cookingTimer;
            }
        }

        public override void UseCooker(int TeamNumber,TALENT t)
        {
            string Material = "";

            SortedSet<DishType> dishTypeSet = new SortedSet<DishType>();
            bool XiangGuo = false;
            if(WorldMap.Grid[(int)Position.x, (int)Position.y].ContainsType(typeof(Tool)))
            {
                foreach (Tool GameObject in WorldMap.Grid[(int)Position.x, (int)Position.y].GetType(typeof(Tool)))
                {
                    if (GameObject.Tool == ToolType.Condiment)
                    {
                        GameObject.Parent = null;
                        XiangGuo = true;
                        break;
                    }
                }
            }
            foreach (Dish GameObject in WorldMap.Grid[(int)Position.x, (int)Position.y].GetType(typeof(Dish)))
            {
                dishTypeSet.Add(GameObject.Dish);
                GameObject.Parent = null;
            }
            if (dishTypeSet.Count == 0) return;
            if (!XiangGuo)
            {
                Cooking = true;
                ProtectedTeam = TeamNumber;
                Dish = DishType.DarkDish;//未煮熟之前都是黑暗料理
                foreach (var dishType in dishTypeSet)
                {
                    Material += dishType.ToString();
                }
                cookingResult = (string)Configs["CookingTable"][Material];
                if (cookingResult == null) cookingResult = "DarkDish";
                CookingTimer.Change((int)Configs[cookingResult]["CookTime"], 0);
                ProtectTimer.Change((int)(1.25 * (double)Configs[cookingResult]["CookTime"]), 0);
            }
            else
            {
                int score = 0;
                foreach (var dishType in dishTypeSet)
                {
                    score += (int)Configs[dishType.ToString()]["Score"];
                }
                if (score < 60) return;
                Cooking = true;
                ProtectedTeam = TeamNumber;
                Dish = DishType.DarkDish;
                cookingResult = "XiangGuo_" + (score / 20).ToString();
                CookingTimer.Change((int)Configs["XiangGuo"]["CookTime"], 0);
                ProtectTimer.Change((int)(1.25 * (double)Configs["XiangGuo"]["CookTime"]), 0);
            }
        }
        //Cook End

        public override int HandIn(DishType dish_t)
        {
            return TaskSystem.HandIn(dish_t);
        }
    }

    public static class TaskSystem
    {
        public static ConcurrentDictionary<DishType, uint> TaskQueue = new ConcurrentDictionary<DishType, uint>();
        private static Timer.MultiTaskTimer _removeTaskTimer;
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
            Program.MessageToClient.Tasks.Add((DishTypeMessage)task);
            Console.WriteLine("Add task : " + task);
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
            Program.MessageToClient.Tasks.Remove((DishTypeMessage)task);
            Console.WriteLine("Remove task : " + task);
            //PrintAllTask();
        }
        public static System.Threading.Timer _refreshTimer;
        public static System.Threading.Timer RefreshTimer
        {
            get
            {
                _refreshTimer = _refreshTimer ?? new System.Threading.Timer(TaskProduce);
                return _refreshTimer;
            }
        }
        public static void TaskProduce(object i)
        {
            DishType temp=DishType.Empty;
            for (; ; )
            {
                if(Timer.Time.GameTime()<TimeSpan.FromMinutes(10))temp = (DishType)Program.Random.Next((int)DishType.TomatoFriedEgg, (int)DishType.XiangGuo);
                else temp = (DishType)Program.Random.Next((int)DishType.TomatoFriedEgg, (int)DishType.XiangGuo_3);
                if (temp == DishType.Size1)
                    continue;
                break;
            }
            AddTask(temp);
            RemoveTaskTimer.Add((o, e) => { RemoveTask(temp); }, (uint)Configs[temp.ToString()]["TaskTime"]);
            //需要广播产生的任务
            //感觉只需要广播任务的产生，而任务被完成以及任务因过时而gg都不用广播，需要玩家自己把握？
        }
        public static int HandIn(DishType dish_t)
        {
            int score = 0;
            if (TaskQueue.ContainsKey(dish_t))
            {
                score = (int)Configs[dish_t.ToString()]["Score"];//菜品名+Score，在App.config里加
                RemoveTask(dish_t);
            }
            //PrintAllTask();
            return score;
        }
        public static void PrintAllTask()
        {
            Console.WriteLine("Tasks : ");
            foreach (var task in TaskQueue)
            {
                Console.WriteLine("\t" + task);
            }
        }
    }
}
