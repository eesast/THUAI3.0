﻿using System;
using System.Collections.Generic;
using System.Text;
using THUnity2D;
using System.Configuration;
using Logic.Constant;

namespace Logic.Server
{
    public class Block : Obj
    {
        public int RefreshTime;//食物刷新点的食物刷新速率，毫秒

        public List<DishType> Task = null;//任务点的任务列表
        public Block(double x_t, double y_t, BlockType type_t) : base(x_t, y_t)
        {
            Blockable = true;
            Movable = false;
            blockType = type_t;
            if (blockType == BlockType.FoodPoint)
            {
                dish = (DishType)new Random().Next(1, (int)DishType.Size1 - 1);
                RefreshTime = 1000;
                Console.WriteLine("食品刷新：地点（" + Position.x + "," + Position.y + "）,种类" + blockType);
                lock (Program.MessageToClientLock)
                {
                    Program.MessageToClient.GameObjectMessageList.Add(
                        this.ID,
                        new Communication.Proto.GameObjectMessage
                        {
                            ObjType = Communication.Proto.ObjTypeMessage.Block,
                            BlockType = Communication.Proto.BlockTypeMessage.FoodPoint,
                            DishType = (Communication.Proto.DishTypeMessage)this.dish,
                            Position = new Communication.Proto.XYPositionMessage { X = this.Position.x, Y = this.Position.y }
                        }
                        );
                }
            }
            else if (blockType == BlockType.TaskPoint)
            {
                Task = new List<DishType>();
                new System.Threading.Timer(TaskProduce, null, 1000, Convert.ToInt32(ConfigurationManager.AppSettings["TaskRefreshTime"]));
                lock (Program.MessageToClientLock)
                {
                    Program.MessageToClient.GameObjectMessageList.Add(
                        this.ID,
                        new Communication.Proto.GameObjectMessage
                        {
                            ObjType = Communication.Proto.ObjTypeMessage.Block,
                            BlockType = Communication.Proto.BlockTypeMessage.TaskPoint,
                            Position = new Communication.Proto.XYPositionMessage { X = this.Position.x, Y = this.Position.y }
                        }
                        );
                }
            }
        }
        public override DishType GetDish(DishType t)
        {
            DishType temp = dish;
            dish = DishType.Empty;
            new System.Threading.Timer(new System.Threading.TimerCallback(Refresh), 0, RefreshTime, 0);
            return temp;
        }
        public void Refresh(object i)
        {
            dish = DishType.Apple;//(Dish.Type)new Random().Next(1, (int)Dish.Type.Size1 - 1);
            Console.WriteLine("食品刷新：地点（" + Position.x + "," + Position.y + "）,种类" + blockType);
        }

        public override void UseCooker()
        {
            string Material = "";

            SortedSet<DishType> dishTypeSet = new SortedSet<DishType>();
            foreach (var unBlockableObject in Logic.Constant.Map.WorldMap.Grid[(int)Position.x, (int)Position.y].unblockableObjects)
            {
                if (unBlockableObject is Dish)
                    dishTypeSet.Add(((Dish)unBlockableObject).dish);
            }

            foreach (var dishType in dishTypeSet)
            {
                Material += dishType.ToString() + " ";
            }

            string result = ConfigurationManager.AppSettings[Material];
            System.Threading.Timer timer = new System.Threading.Timer(Cook, result, Convert.ToInt32(ConfigurationManager.AppSettings[result + "Time"]), 0);
            DishType GetResult(string s)
            {
                return DishType.DarkDish;
            }
            void Cook(object s)
            {
                if (s is string)
                    dish = GetResult((string)s);
            }
        }

        public void TaskProduce(object i)
        {
            DishType temp = DishType.Apple;//(Dish.Type)new Random().Next();
            Task.Add(temp);
            new System.Threading.Timer(remove, temp,
                Convert.ToInt32(ConfigurationManager.AppSettings["TaskTimeLimit"]), 0);

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
                return Convert.ToInt32(ConfigurationManager.AppSettings[dish_t.ToString() + "Score"]);//菜品名+Score，在App.config里加，里面有AppleScore
                //测试的时候能直接把食材交进去，比赛的只会产生菜品任务
            }
            return 0;
        }
    }
}
