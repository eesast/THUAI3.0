using System;
using System.Collections;
using System.Collections.Generic;
using Communication.Proto;
using Communication.Server;
using THUnity2D;
using static THUnity2D.Tools;
using System.Configuration;
namespace Logic.Constant
{
    public static class Constant
    {
        public const double MoveSpeed = 5;
        public const int FrameRate = 20;
        public const double TimeInterval = 1 / FrameRate;
        public const double MoveDistancePerFrame = MoveSpeed / FrameRate;
<<<<<<< HEAD
        public const char messageSpiltSeperation = ',';
    }
    public enum ObjType
    {
        Air = 0,
        People,
        Block,
        Dish,
        Tools,
        Trigger,
        Size
    }
    public enum BlockType
    {
        Wall,
        Table,
        FoodPoint,
        Cooker,
        RubbishBin,
        TaskPoint,
        Size
    }
    public enum DishType
    {
        Empty = 0,//空
                  //以下为食材
        Apple,
        Banana,
        Potato,
        Tomato,
        Size1,
        //以下为菜品

        DarkDish,
        Size2
    }
    public enum ToolType
    {
        Empty = 0,
        TigerShoes,//虎头鞋
        SpeedBuff,//极速buff
        StrenthBuff,//力量buff
        TeleScope,//望远镜
        Condiment,//调料
        Fertilizer,//肥料
        BreastPlate,//护心镜
        SpaceGate,//传送门
        Eye,//眼

        WaveGlue,//胶水
        LandMine,//地雷
        Trap,//陷阱
        FlashBomb,//闪光弹
        Hammer,//锤子
        Brick,//砖头
        Stealer,//分数偷取

        Size
    }
    public enum TriggerType
    {
        Trap,
        Mine,
        Size
    }
=======
        public static readonly Dictionary<Direction, XYPosition> MoveOperations = new Dictionary<Direction, XYPosition> {
            { Direction.Right, MoveDistancePerFrame * EightUnitVector[Direction.Right] },
            { Direction.RightUp, MoveDistancePerFrame * EightUnitVector[Direction.RightUp] },
            { Direction.Up, MoveDistancePerFrame * EightUnitVector[Direction.Up] },
            { Direction.LeftUp, MoveDistancePerFrame * EightUnitVector[Direction.LeftUp] },
            { Direction.Left, MoveDistancePerFrame * EightUnitVector[Direction.Left] },
            { Direction.LeftDown, MoveDistancePerFrame * EightUnitVector[Direction.LeftDown] },
            { Direction.Down, MoveDistancePerFrame * EightUnitVector[Direction.Down] },
            { Direction.RightDown, MoveDistancePerFrame * EightUnitVector[Direction.RightDown] }
        };
        public const char messageSpiltSeperation = ',';
    }
    public class Obj : GameObject
    {
        public enum Type
        {
            Air = 0,
            People,
            Block,
            Dish,
            Tools,
            Trigger,
            Size
        }
        public Type type;
        public Dish.Type dish;
        public Tool.Type tool;
        public Block.Type blockType;
        //public const int width = 1;
        //public const int height = 1;

        public Obj(double x_t, double y_t) : base(new XYPosition(x_t, y_t))
        {
        }
        public virtual Dish.Type GetDish(Dish.Type t) { return Dish.Type.Empty; }
        public virtual Tool.Type GetTool(Tool.Type t) { return Tool.Type.Empty; }
        public virtual void UseCooker() { }
        public virtual int HandIn(Dish.Type dish_t) { return 0; }
    }
    public class Block : Obj
    {
        public new enum Type
        {
            Wall,
            Table,
            FoodPoint,
            Cooker,
            RubbishBin,
            TaskPoint,
            Size
        }

        public int RefreshTime;//食物刷新点的食物刷新速率，毫秒

        public List<Dish.Type> Task = null;//任务点的任务列表
        public Block(double x_t, double y_t, Type type_t) : base(x_t, y_t)
        {
            Blockable = true;
            Movable = false;
            blockType = type_t;
            if (blockType == Type.FoodPoint)
            {
                dish = (Dish.Type)new Random().Next(1, (int)Dish.Type.Size1 - 1);
                RefreshTime = 1000;
                Console.WriteLine("食品刷新：地点（" + Position.x + "," + Position.y + "）,种类" + blockType);
            }
            else if (blockType == Type.TaskPoint)
            {
                Task = new List<Dish.Type>();
                new System.Threading.Timer(TaskProduce, null, 1000, Convert.ToInt32(ConfigurationManager.AppSettings["TaskRefreshTime"]));

            }
        }
        public override Dish.Type GetDish(Dish.Type t)
        {
            Dish.Type temp = dish;
            dish = Dish.Type.Empty;
            new System.Threading.Timer(new System.Threading.TimerCallback(Refresh), 0, RefreshTime, 0);
            return temp;
        }
        public void Refresh(object i)
        {
            dish = Dish.Type.Apple;//(Dish.Type)new Random().Next(1, (int)Dish.Type.Size1 - 1);
            Console.WriteLine("食品刷新：地点（" + Position.x + "," + Position.y + "）,种类" + blockType);
        }

        public override void UseCooker()
        {
            string Material = "";

            SortedSet<Dish.Type> dishTypeSet = new SortedSet<Dish.Type>();
            foreach (var unBlockableObject in Map.WorldMap.Grid[(int)Position.x, (int)Position.y].unblockableObjects)
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
            Dish.Type GetResult(string s)
            {
                return Dish.Type.DarkDish;
            }
            void Cook(object s)
            {
                if (s is string)
                    dish = GetResult((string)s);
            }
        }

        public void TaskProduce(object i)
        {
            Dish.Type temp = Dish.Type.Apple;//(Dish.Type)new Random().Next();
            Task.Add(temp);
            new System.Threading.Timer(remove, temp,
                Convert.ToInt32(ConfigurationManager.AppSettings["TaskTimeLimit"]), 0);

            void remove(object task)
            {
                if (task is Dish.Type)
                    Task.Remove((Dish.Type)task);
            }
        }

        public override int HandIn(Dish.Type dish_t)
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
    public class Dish : Obj //包括食材和做好的菜
    {
        public new enum Type
        {
            Empty = 0,//空
            //以下为食材
            Apple,
            Banana,
            Potato,
            Tomato,
            Size1,
            //以下为菜品

            DarkDish,
            Size2
        }

        public int distance;
        public Direction direction;
        public TimeSpan LastActTime;

        public Dish(double x_t, double y_t, Type type_t) : base(x_t, y_t)
        {
            Blockable = false;
            Movable = false;
            dish = type_t;
        }
        public override Type GetDish(Type t)
        {
            Type temp = dish;
            if (t == Type.Empty) this.Parent = null;
            else dish = t;
            return temp;
        }
    }

    public class Tool : Obj
    {
        public new enum Type
        {
            Empty = 0,
            TigerShoes,//虎头鞋
            SpeedBuff,//极速buff
            StrenthBuff,//力量buff
            TeleScope,//望远镜
            Condiment,//调料
            Fertilizer,//肥料
            BreastPlate,//护心镜
            SpaceGate,//传送门
            Eye,//眼

            WaveGlue,//胶水
            LandMine,//地雷
            Trap,//陷阱
            FlashBomb,//闪光弹
            Hammer,//锤子
            Brick,//砖头
            Stealer,//分数偷取

            Size
        }
        public new Type type;
        public Tool(double x_t, double y_t, Type type_t) : base(x_t, y_t)
        {
            Blockable = false;
            Movable = false;
            type = type_t;
        }
        public override Type GetTool(Type t)
        {
            Type temp = type;
            if (t == Type.Empty) this.Parent = null;
            else type = t;
            return temp;
        }

    }
    public class Trigger : Obj
    {
        public new enum Type
        {
            Trap,
            Mine,
            Size
        }
        public new Type type;

        public Trigger(double x_t, double y_t, Type type_t) : base(x_t, y_t)
        {
            Blockable = false;
            Movable = false;
            type = type_t;
        }
    }
>>>>>>> 1563b56fedd49c24139c431ce521aed9346ea401
    public enum CommandType
    {
        Move = 0,
        Pick,
        Put,
        Use,
        Stop,
        Size
    }

    public enum TALENT
    {
        None,
        Run,
        Strenth,
        Cook,
        Technology,
        Luck,
        Bag,
        Drunk
    }
}