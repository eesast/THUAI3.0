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
        public BlockType blockType;
        
        public Block(double x_t, double y_t, BlockType type_t) : base(x_t, y_t, ObjType.Block)
        {
            if (type_t == BlockType.Wall || type_t == BlockType.FoodPoint || type_t == BlockType.TaskPoint)
                Layer = WallLayer;
            else
                Layer = BlockLayer;
            Movable = false;
            blockType = type_t;
            
        }

        public override DishType GetDish(DishType t)
        {
            DishType temp = Dish;
            Dish = DishType.DishEmpty;
            return temp;
        }
    }

    public class FoodPoint : Block
    {
        public int RefreshTime = (int)Configs("FoodPointInitRefreshTime");//食物刷新点的食物刷新速率，毫秒
        public DishType foodType;
        public FoodPoint(double x_t, double y_t, DishType foodType_t = DishType.DishEmpty) : base(x_t, y_t, BlockType.FoodPoint)
        {
            foodType = foodType_t;
            Layer = WallLayer;
            AddToMessage();
            Dish = foodType;
            //lock (Program.MessageToClientLock)
            Program.MessageToClient.GameObjectList[ID].BlockType = BlockType.FoodPoint;
            Server.ServerDebug("食品刷新：地点（" + Position.x + "," + Position.y + "）, 种类 : " + Dish);
        }

        public override DishType GetDish(DishType t)
        {
            RefreshTimer.Change(RefreshTime, 0);
            return base.GetDish(t);
        }

        //Refresh
        protected System.Threading.Timer? _refreshTimer;
        public System.Threading.Timer RefreshTimer
        {
            get
            {
                _refreshTimer = _refreshTimer ?? new System.Threading.Timer(Refresh);
                return _refreshTimer;
            }
        }
        public void Refresh(object? i)
        {
            Dish = foodType;
            Server.ServerDebug("食品刷新：地点（" + Position.x + "," + Position.y + "）, 种类 : " + Dish);
        }
        //Refresh End
    }

    public class Cooker : Block
    {
        public Cooker(double x_t, double y_t) : base(x_t, y_t, BlockType.Cooker)
        {
            Layer = BlockLayer;
            AddToMessage();
            Dish = DishType.DishEmpty;
            //lock (Program.MessageToClientLock)
            Program.MessageToClient.GameObjectList[ID].BlockType = BlockType.Cooker;

        }

        public override DishType GetDish(DishType t)
        {
            cookingResult = "DishEmpty";
            isCooking = false;
            ProtectTimer.Change(0, 0);
            return base.GetDish(t);
        }

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
        protected void Cook(object? o)
        {
            isCooking = false;
            Dish = (DishType)Enum.Parse(typeof(DishType), cookingResult);
            if (Dish > DishType.DishEmpty && Dish < DishType.DishSize2 && Dish != DishType.DishSize1)
            {
                if (Dish < DishType.SpicedPot)
                    CookingTimer.Change((int)(0.5 * (double)Configs(cookingResult, "CookTime")), 0);//0.5倍CookTime是保鲜时间
                else
                    CookingTimer.Change((int)(0.5 * (double)Configs("SpicedPot", "CookTime")), 0);
                cookingResult = "OverCookedDish";
            }
        }
        protected string cookingResult = "";
        public bool isCooking = false;

        public int ProtectedTeam = -1;
        protected System.Threading.Timer _protectTimer;
        public System.Threading.Timer ProtectTimer
        {
            get
            {
                _protectTimer = _protectTimer ?? new System.Threading.Timer((i) => { ProtectedTeam = -1; });
                return _protectTimer;
            }
        }

        public override void UseCooker(int TeamNumber, Talent t)
        {
            if (isCooking)
                return;
            string Material = "";

            SortedSet<DishType> dishTypeSet = new SortedSet<DishType>();
            bool isSpicedPot = false;
            var toolObjects = WorldMap.Grid[(int)Position.x, (int)Position.y].GetObjects(typeof(Tool));
            if (toolObjects != null)
            {
                foreach (Tool tool in toolObjects)
                {
                    if (tool.Tool == ToolType.Condiment)
                    {
                        tool.Parent = null;
                        isSpicedPot = true;
                        break;
                    }
                }
            }
            var dishObjects = WorldMap.Grid[(int)Position.x, (int)Position.y].GetObjects(typeof(Dish));
            if (dishObjects != null)
                foreach (Dish dish in dishObjects)
                {
                    dishTypeSet.Add(dish.Dish);
                    dish.Parent = null;
                }
            if (dishTypeSet.Count == 0) return;
            if (!isSpicedPot)
            {
                isCooking = true;
                ProtectedTeam = TeamNumber;
                Dish = DishType.CookingDish;//未煮熟
                foreach (var dishType in dishTypeSet)
                {
                    Material += dishType.ToString();
                }
                cookingResult = (string)Configs("CookingTable", Material);
                if (cookingResult == null) cookingResult = "DarkDish";
                CookingTimer.Change((int)Configs(cookingResult, "CookTime"), 0);
                ProtectTimer.Change((int)(1.25 * (double)Configs(cookingResult, "CookTime")), 0);
            }
            else
            {
                int score = 0;
                foreach (var dishType in dishTypeSet)
                {
                    if ((int)dishType <= (int)DishType.CookedRice)
                        score += (int)Configs(dishType.ToString(), "Score");
                }
                if (score < 60) return;
                isCooking = true;
                ProtectedTeam = TeamNumber;
                Dish = DishType.CookingDish;
                if (t != Talent.Cook && score > 80) score = 80;
                cookingResult = "SpicedPot" + (score / 20).ToString();
                CookingTimer.Change((int)Configs("SpicedPot", "CookTime"), 0);
                ProtectTimer.Change((int)(1.25 * (double)Configs("SpicedPot", "CookTime")), 0);
            }
        }
        //Cook End

    }

    public class TaskPoint : Block
    {
        public TaskPoint(double x_t, double y_t) : base(x_t, y_t, BlockType.TaskPoint)
        {
            Layer = WallLayer;
        }

        public override int HandIn(DishType dish_t)
        {
            return TaskSystem.HandIn(dish_t);
        }
    }

    public class Wall : Block
    {
        public Wall(double x_t, double y_t) : base(x_t, y_t, BlockType.Wall)
        {
            Layer = WallLayer;
        }
    }

    public class Table : Block
    {
        public Table(double x_t, double y_t) : base(x_t, y_t, BlockType.Table)
        {
            Layer = BlockLayer;
        }
    }

    public class RubbishBin : Block
    {
        public RubbishBin(double x_t, double y_t) : base(x_t, y_t, BlockType.RubbishBin)
        {
            Layer = BlockLayer;
        }
    }

}
