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
    //public class Obj
    //{
    //    public enum Type
    //    {
    //        Air = 0,
    //        People,
    //        Block,
    //        Dish,
    //        Tools,
    //        Trigger,
    //        Size
    //    }
    //    public const byte BYTE_LENGTH = XYPosition.BYTE_LENGTH + 1;
    //    public XYPosition xyPosition;
    //    public Type type;
    //    public const int width = 1;
    //    public const int height = 1;

    //    public Obj(double x_t, double y_t)
    //    {
    //        xyPosition.x = x_t;
    //        xyPosition.y = y_t;
    //    }
    //    public override string ToString()
    //    {
    //        return ((byte)type).ToString();
    //    }

    //    public void RemoveSelf()
    //    {
    //        WORLD_MAP[(int)xyPosition.x, (int)xyPosition.y].Remove(this);
    //    }
    //    public virtual Dish.Type GetDish(Dish.Type t) { return Dish.Type.Empty; }
    //    public virtual Tool.Type GetTool(Tool.Type t) { return Tool.Type.Empty; }
    //    public virtual void UseCooker() { }
    //    public virtual void HandIn() { }
    //}
    //public class People : GameObject
    //{
    //    public Dish.Type dish;
    //    public Tool.Type tool;
    //    public Direction facingDirection;
    //    public People(double x_t, double y_t) : base(x_t, y_t)
    //    {
    //        dish = Dish.Type.Empty;
    //        tool = Tool.Type.Empty;
    //    }

    //}
    public class Block : GameObject
    {
        public enum Type
        {
            Wall,
            Table,
            FoodPoint,
            Cooker,
            RubbishBin,
            TaskPoint,
            Size
        }
        public Type type;

        public Dish.Type Foodtype;//仅当type为FoodPoint时有效
        public int RefreshTime;

        public Dish.Type[] CookingFood = null;
        public Block(double x, double y, Type type) : base(new XYPosition(x, y))
        {
            Blockable = true;
            this.type = type;
            if (type == Type.FoodPoint)
            {
                Foodtype = (Dish.Type)new Random().Next(0, (int)Dish.Type.Size1 - 1);
                RefreshTime = 10000;
                Console.WriteLine("食品刷新：地点（" + Position.x + "," + Position.y + "）,种类" + type);
            }
            else if (type == Type.Cooker) CookingFood = new Dish.Type[10];
        }
        public Dish.Type GetDish(Dish.Type t)
        {
            Dish.Type temp = Foodtype;
            Foodtype = Dish.Type.Empty;
            System.Threading.Timer timer = new System.Threading.Timer(new System.Threading.TimerCallback(Refresh), 0, RefreshTime, 0);
            return temp;
        }
        public void Refresh(object i)
        {
            Foodtype = (Dish.Type)new Random().Next(1, (int)Dish.Type.Size1 - 1);
            Console.WriteLine("食品刷新：地点（" + Position.x + "," + Position.y + "）,种类" + type);
        }

        public void UseCooker()
        {

        }

        public void HandIn()
        {

        }
    }
    public class Dish : GameObject //包括食材和做好的菜
    {
        public enum Type
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
        public Type type;

        public int distance;
        public Direction direction;
        public TimeSpan LastActTime;

        public Dish(double x_t, double y_t, Type type_t) : base(new XYPosition(x_t, y_t))
        {
            type = type_t;
            type = Type.Empty;
        }
        public Type GetDish(Type t)
        {
            Type temp = type;
            if (t == Type.Empty) this.Parent = null;
            else type = t;
            return temp;
        }

        //public void move()
        //{
        //    if(Time.GameTime() - LastActTime>TimeSpan.FromSeconds(0.5)) LastActTime = Time.GameTime();
        //    XYPosition aim = CONSTANT.OPERATION[(uint)direction] * (10 * (Time.GameTime() - LastActTime).TotalSeconds) + xyPosition;
        //    LastActTime = Time.GameTime();
        //    if ((uint)xyPosition.x != (uint)aim.x || (uint)xyPosition.y != (uint)aim.y)
        //    {
        //        Type temp = type;
        //        WORLD_MAP[(uint)xyPosition.x, (uint)xyPosition.y][0].RemoveSelf();
        //        xyPosition = aim;
        //        WORLD_MAP[(uint)xyPosition.x, (uint)xyPosition.y].Insert(0, new Dish(xyPosition.x, xyPosition.y, temp));
        //    }
        //    else
        //    {
        //        xyPosition = aim;
        //        WORLD_MAP[(uint)xyPosition.x, (uint)xyPosition.y][0].xyPosition = xyPosition;
        //    }
        //}
    }

    public class Tool : GameObject
    {
        public enum Type
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
        public Type type;
        public Tool(double x_t, double y_t, Type type_t) : base(new XYPosition(x_t, y_t))
        {
            type = type_t;
        }
        public Type GetTool(Type t)
        {
            Type temp = type;
            if (t == Type.Empty) this.Parent = null;
            else type = t;
            return temp;
        }

    }
    public class Trigger : GameObject
    {
        public enum Type
        {
            Trap,
            Mine,
            Size
        }
        public Type type;

        public Trigger(double x_t, double y_t, Type type_t) : base(new XYPosition(x_t, y_t))
        {
            type = type_t;
        }
    }

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