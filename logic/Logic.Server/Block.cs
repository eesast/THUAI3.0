using System;
using System.Collections.Generic;
using System.Text;
using static Logic.Constant.MapInfo;
using System.Configuration;
using Logic.Constant;
using Communication.Proto;
using System.Collections.Concurrent;

namespace Logic.Server
{
    public class Block : Obj
    {
        public int RefreshTime;//食物刷新点的食物刷新速率，毫秒

        public Block(double x_t, double y_t, BlockType type_t) : base(x_t, y_t, ObjType.Block)
        {
            if (type_t == BlockType.Wall)
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
                    RefreshTime = Convert.ToInt32(ConfigurationManager.AppSettings["FoodPointInitRefreshTime"]);
                    Console.WriteLine("食品刷新：地点（" + Position.x + "," + Position.y + "）, 种类 : " + Dish);
                    break;
                case BlockType.Cooker:
                    AddToMessage();
                    Dish = DishType.Empty;
                    lock (Program.MessageToClientLock)
                        Program.MessageToClient.GameObjectMessageList[ID].BlockType = BlockTypeMessage.Cooker;
                    break;
                case BlockType.TaskPoint:
                    lock (Program.MessageToClientLock)
                    {
                        Program.MessageToClient.GameObjectMessageList.Add(
                            this.ID,
                            new GameObjectMessage
                            {
                                ObjType = ObjTypeMessage.Block,
                                BlockType = BlockTypeMessage.TaskPoint,
                                DishType = (DishTypeMessage)Dish,
                                Position = new XYPositionMessage { X = Position.x, Y = Position.y }
                            });
                    }
                    break;
            }
        }
        public override DishType GetDish(DishType t)
        {
            DishType temp = Dish;
            Dish = DishType.Empty;
            if (this.blockType == BlockType.FoodPoint) RefreshTimer.Change(RefreshTime, 0);
            return temp;
        }
        protected System.Threading.Timer _refreshTimer;
        public System.Threading.Timer RefreshTimer
        {
            get
            {
                if (this.blockType == BlockType.FoodPoint) _refreshTimer = _refreshTimer ?? new System.Threading.Timer(Refresh);
                return _refreshTimer;
            }
        }
        public System.Threading.Timer CookingTimer;
        public bool Cooking = false;
        public void Refresh(object i)
        {
            Dish = (DishType)Program.Random.Next(1, (int)DishType.Size1 - 1);
            Console.WriteLine("食品刷新：地点（" + Position.x + "," + Position.y + "）, 种类 : " + Dish);
        }

        public override void UseCooker()
        {
            string Material = "";

            SortedSet<DishType> dishTypeSet = new SortedSet<DishType>();
            foreach (Dish GameObject in WorldMap.Grid[(int)Position.x, (int)Position.y].GetType(typeof(Dish)))
            {
                dishTypeSet.Add(GameObject.Dish);
                GameObject.Parent = null;
            }
            if (dishTypeSet.Count == 0) return;
            Cooking = true;
            foreach (var dishType in dishTypeSet)
            {
                Material += dishType.ToString();
                
            }
            string result = ConfigurationManager.AppSettings[Material];
            if (result == null) result = "DarkDish";
            CookingTimer = new System.Threading.Timer(Cook, result, int.Parse(ConfigurationManager.AppSettings[result + "Time"]), 0);
            DishType GetResult(string s)
            {
                for (int i = 0; i < (int)DishType.Size2; i++)
                {
                    if ((Convert.ToString((DishType)i)) == s) return (DishType)i;
                }
                return DishType.DarkDish;
            }
            void Cook(object s)
            {
                if (s is string)
                { Dish = GetResult((string)s); Cooking = false; }
            }
        }


        public override int HandIn(DishType dish_t)
        {
            return TaskSystem.HandIn(dish_t);
        }
    }

    public static class TaskSystem
    {
        public class Task
        {
            public DishType type = (DishType)new Random((int)Timer.Time.GameTime().TotalMilliseconds).Next((int)DishType.Size1 + 1, (int)DishType.Size2 - 2);
            public bool Done = false;
            public System.Threading.Timer timer = new System.Threading.Timer(DeQueue, null, Convert.ToInt32(ConfigurationManager.AppSettings["TaskTimeLimit"]), 0);
            ~Task()
            {
                timer.Dispose();
            }
        }
        public static ConcurrentQueue<Task> TaskQueue = new ConcurrentQueue<Task>();
        public static Task DeQueueTask;
        public static void DeQueue(object i)
        {
            TaskQueue.TryDequeue(out DeQueueTask);
        }

        public static System.Threading.Timer RefreshTimer = new System.Threading.Timer(TaskProduce, null, 1000, Convert.ToInt32(ConfigurationManager.AppSettings["TaskRefreshTime"]));
        public static void TaskProduce(object i)
        {
            TaskQueue.Enqueue(new Task());
            foreach (Task task in TaskQueue)
            {
                Console.WriteLine(task.type.ToString() + " " + task.Done.ToString());
            }
            Console.WriteLine();
                //需要广播产生的任务
                //感觉只需要广播任务的产生，而任务被完成以及任务因过时而gg都不用广播，需要玩家自己把握？
        }
        public static int HandIn(DishType dish_t)
        {
            int i = 0;
            foreach(Task task in TaskQueue)
            {
                if (task.Done == false && task.type == dish_t) 
                {
                    task.Done = true;

                    i = Convert.ToInt32(ConfigurationManager.AppSettings[dish_t.ToString() + "Score"]);//菜品名+Score，在App.config里加
                }
            }
            foreach (Task task in TaskQueue)
            {
                Console.WriteLine(task.type.ToString() + " " + task.Done.ToString());
            }
            Console.WriteLine();
            return i;
        }
    }
}
