using System;
using System.Collections.Generic;
using System.Text;
using static Logic.Constant.MapInfo;
using System.Configuration;
using Logic.Constant;
using Communication.Proto;

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
                    //new System.Threading.Timer(TaskProduce, null, 1000, Convert.ToInt32(ConfigurationManager.AppSettings["TaskRefreshTime"]));
                    //lock (Program.MessageToClientLock)
                    //{
                    //    Program.MessageToClient.GameObjectMessageList.Add(
                    //        this.ID,
                    //        new GameObjectMessage
                    //        {
                    //            ObjType = ObjTypeMessage.Block,
                    //            BlockType = BlockTypeMessage.TaskPoint,
                    //            DishType = (DishTypeMessage)Dish,
                    //            Position = new XYPositionMessage { X = Position.x, Y = Position.y }
                    //        });
                    //}
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

        public void TaskProduce(object i)
        {
            DishType temp = DishType.Apple;//(Dish.Type)new Random().Next();
            Task.Add(temp);
            new System.Threading.Timer(remove, temp,
                int.Parse(ConfigurationManager.AppSettings["TaskTimeLimit"]), 0);

            void remove(object task)
            {
                if (task is DishType)
                    Task.Remove((DishType)task);
            }
        }

        public override int HandIn(DishType dish_t)
        {
            if (Task.Contains(dish_t))
            {
                Task.Remove(dish_t);
                return int.Parse(ConfigurationManager.AppSettings[dish_t.ToString() + "Score"]);//菜品名+Score，在App.config里加，里面有AppleScore
                //测试的时候能直接把食材交进去，比赛的只会产生菜品任务
            }
            return 0;
        }
    }
}
