using System;
using System.Collections.Generic;
using System.Text;
using static Logic.Constant.MapInfo;
using System.Configuration;
using Logic.Constant;
using Communication.Proto;
using Timer;

namespace Logic.Server
{
    public class Block : Obj
    {
        public int RefreshTime;//食物刷新点的食物刷新速率，毫秒


        public HashSet<DishType> Task = null;//任务点的任务列表
        public Block(double x_t, double y_t, BlockType type_t) : base(x_t, y_t, ObjType.Block)
        {
            if (type_t == BlockType.Wall)
                Layer = (int)MapLayer.WallLayer;
            else
                Layer = (int)MapLayer.BlockLayer;
            Movable = false;
            blockType = type_t;
            switch (blockType)
            {
                case BlockType.FoodPoint:
                    AddToMessage();
                    Dish = (DishType)Program.Random.Next(1, (int)DishType.Size1 - 1);
                    lock (Program.MessageToClientLock)
                        Program.MessageToClient.GameObjectMessageList[ID].BlockType = BlockTypeMessage.FoodPoint;
                    RefreshTime = 4000;
                    Console.WriteLine("食品刷新：地点（" + Position.x + "," + Position.y + "）, 种类 : " + Dish);
                    break;
                case BlockType.Cooker:
                    AddToMessage();
                    Dish = DishType.Empty;
                    lock (Program.MessageToClientLock)
                        Program.MessageToClient.GameObjectMessageList[ID].BlockType = BlockTypeMessage.Cooker;
                    break;
                case BlockType.TaskPoint:
                    Task = new HashSet<DishType>();
                    TaskProduceTimer.Add(TaskProduce, 5000);
                    AddToMessage();
                    lock (Program.MessageToClientLock)
                        Program.MessageToClient.GameObjectMessageList[ID].BlockType = BlockTypeMessage.TaskPoint;
                    break;
            }
        }
        public override DishType GetDish(DishType t)
        {
            switch (this.blockType)
            {
                case BlockType.FoodPoint:
                    DishType temp = Dish;
                    Dish = DishType.Empty;
                    RefreshTimer.Change(RefreshTime, 0);
                    return temp;
                    break;
                default:
                    DishType tmp = Dish;
                    Dish = DishType.Empty;
                    return tmp;
            }

        }
        protected System.Threading.Timer _refreshTimer;
        public System.Threading.Timer RefreshTimer
        {
            get
            {
                _refreshTimer = _refreshTimer ?? new System.Threading.Timer(Refresh);
                return _refreshTimer;
            }
        }
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

            foreach (var dishType in dishTypeSet)
            {
                Material += dishType.ToString();
            }

            string result = ConfigurationManager.AppSettings[Material];
            System.Threading.Timer timer = new System.Threading.Timer(Cook, result, int.Parse(ConfigurationManager.AppSettings[result + "Time"]), 0);
            DishType GetResult(string s)
            {
                return DishType.DarkDish;
            }
            void Cook(object s)
            {
                if (s is string)
                    Dish = GetResult((string)s);
            }
        }

        protected MultiTaskTimer _taskProduceTimer;
        public MultiTaskTimer TaskProduceTimer
        {
            get
            {
                _taskProduceTimer = _taskProduceTimer ?? new MultiTaskTimer();
                return _taskProduceTimer;
            }
        }

        public void TaskProduce(object i, System.Timers.ElapsedEventArgs e)
        {
            DishType temp = (DishType)Program.Random.Next((int)DishType.Size1 + 1, (int)DishType.Size2);
            Task.Add(temp);
            //Server.ServerDebug("Produce task : " + temp);
            lock (Program.MessageToClientLock)
                Program.MessageToClient.GameObjectMessageList[ID].Task = (DishTypeMessage)temp;
            TaskProduceTimer.Add(remove, 5000);
            TaskProduceTimer.Add(TaskProduce, 8000);

            void remove(object o, System.Timers.ElapsedEventArgs e)
            {
                Task.Remove(temp);
                //Server.ServerDebug("Remove task : " + temp);
                lock (Program.MessageToClientLock)
                    Program.MessageToClient.GameObjectMessageList[ID].Task = (DishTypeMessage)DishType.Empty;
            }
        }

        public override int HandIn(DishType dish_t)
        {
            if (Task.Contains(dish_t))
            {
                Task.Remove(dish_t);
                lock (Program.MessageToClientLock)
                    Program.MessageToClient.GameObjectMessageList[ID].Task = (DishTypeMessage)DishType.Empty;
                return int.Parse(ConfigurationManager.AppSettings[dish_t.ToString() + "Score"]);//菜品名+Score，在App.config里加，里面有AppleScore
                //测试的时候能直接把食材交进去，比赛的只会产生菜品任务
            }
            return -10;
        }
    }
}
