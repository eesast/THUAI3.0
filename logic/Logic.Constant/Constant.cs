using System;
using System.Collections;
using System.Collections.Generic;
using static Map;
namespace Logic.Constant
{
    public static class Constant
    {

        public const double MoveSpeed = 5;
        public const int FrameRate = 20;
        public const double TimeInterval = 1 / FrameRate;
        public const double MoveDistancePerFrame = MoveSpeed / FrameRate;
        //长度为1的向量。
        public static readonly Dictionary<Direction, XY_Position> Operations = new Dictionary<Direction, XY_Position> {
            { Direction.Right,new XY_Position (1, 0) },
            { Direction.RightUp, new XY_Position(1 / Math.Sqrt(2),1 / Math.Sqrt(2)) },
            { Direction.Up, new XY_Position(0, 1) },
            { Direction.LeftUp, new XY_Position(-1 / Math.Sqrt(2),1 / Math.Sqrt(2)) },
            { Direction.Left, new XY_Position(-1, 0) },
            { Direction.LeftDown,new XY_Position(-1 / Math.Sqrt(2),-1 / Math.Sqrt(2)) },
            { Direction.Down, new XY_Position(0, -1) },
            { Direction.RightDown, new XY_Position(1 / Math.Sqrt(2),-1 / Math.Sqrt(2)) }
        };
        //长度为MoveDistancePerFrame的向量
        public static readonly Dictionary<Direction, XY_Position> MoveOperations = new Dictionary<Direction, XY_Position> {
            { Direction.Right, MoveDistancePerFrame * Operations[Direction.Right] },
            { Direction.RightUp, MoveDistancePerFrame * Operations[Direction.RightUp] },
            { Direction.Up, MoveDistancePerFrame * Operations[Direction.Up] },
            { Direction.LeftUp, MoveDistancePerFrame * Operations[Direction.LeftUp] },
            { Direction.Left, MoveDistancePerFrame * Operations[Direction.Left] },
            { Direction.LeftDown,MoveDistancePerFrame * Operations[Direction.LeftDown] },
            { Direction.Down, MoveDistancePerFrame * Operations[Direction.Down] },
            { Direction.RightDown, MoveDistancePerFrame * Operations[Direction.RightDown] }
        };
        //指向四个角的向量
        public static readonly Dictionary<Direction, XY_Position> CornerOperations = new Dictionary<Direction, XY_Position> {
            { Direction.Right, new XY_Position (0.5, 0) },
            { Direction.RightUp, new XY_Position(0.5,0.5) },
            { Direction.Up, new XY_Position(0, 0.5) },
            { Direction.LeftUp, new XY_Position(-0.5,0.5) },
            { Direction.Left, new XY_Position(-0.5, 0) },
            { Direction.LeftDown,new XY_Position(-0.5,-0.5) },
            { Direction.Down, new XY_Position(0, -0.5) },
            { Direction.RightDown, new XY_Position(0.5,-0.5) }
        };

        public const char messageSpiltSeperation = ',';
    }
    public enum Direction
    {
        Right = 0,
        RightUp,
        Up,
        LeftUp,
        Left,
        LeftDown,
        Down,
        RightDown,
        Size
    }
    public class Obj
    {
        public enum Type
        {
            Air,
            People,
            Block,
            Dish,
            Tools,
            Trigger,
            Size
        }
        public const byte BYTE_LENGTH = XY_Position.BYTE_LENGTH + 1;
        public XY_Position xyPosition;
        public Type type;
        public const int width = 1;
        public const int height = 1;

        public Obj(double x_t, double y_t)
        {
            xyPosition.x = x_t;
            xyPosition.y = y_t;
        }
        public override string ToString()
        {
            return ((byte)type).ToString();
        }

        public void RemoveSelf()
        {
            if (WORLD_MAP[(uint)xyPosition.x, (uint)xyPosition.y, 0] == this)
            {
                //Console.WriteLine("RemoveSelf , " + WORLD_MAP[(uint)xyPosition.x, (uint)xyPosition.y, 0].xyPosition.ToString());
                WORLD_MAP[(uint)xyPosition.x, (uint)xyPosition.y, 0] = null;
                return;
            }
            if (WORLD_MAP[(uint)xyPosition.x, (uint)xyPosition.y, 1] == this)
                WORLD_MAP[(uint)xyPosition.x, (uint)xyPosition.y, 1] = null;
        }
    }
    public class People : Obj
    {
        public Tuple<int, int> id { get; }
        public Dish? dish = null;
        public Tool? tool = null;
        public Direction facingDirection;
        public People(double x_t, double y_t, Tuple<int, int> id_t) : base(x_t, y_t)
        {
            id = id_t;
        }
    }
    public class Block : Obj
    {
        public new enum Type
        {
            Wall,
            NewFood,
            Pot,
            ChoppingTable,
            RubbishBin,
            Size
        }
        public new Type type { get; }
        public Block(double x_t, double y_t, Type type_t) : base(x_t, y_t)
        {
            type = type_t;
        }
    }
    public class Dish : Obj
    {
        public new enum Type
        {
            Apple,
            Banana,
            Size
        }
        public new Type type { get; }
        public Dish(double x_t, double y_t, Type type_t) : base(x_t, y_t)
        {
            type = type_t;
        }
    }
    public class Tool : Obj
    {
        public new enum Type
        {
            TigerShoes,
            WaveGlue,
            Size
        }
        public new Type type { get; }
        public Tool(double x_t, double y_t, Type type_t) : base(x_t, y_t)
        {
            type = type_t;
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
        public new Type type { get; }

        public Trigger(double x_t, double y_t, Type type_t) : base(x_t, y_t)
        {
            type = type_t;
        }
    }

    public struct XY_Position
    {
        public double x;
        public double y;
        public const byte BYTE_LENGTH = 2 * sizeof(double);
        public XY_Position(double x_t = 0, double y_t = 0)
        {
            this.x = x_t;
            this.y = y_t;
        }
        public override string ToString()
        {
            return
            x.ToString("0.000") + Constant.messageSpiltSeperation +
                y.ToString("0.000");
        }
        public double get(bool flag)
        {
            if (flag) return y;
            else return x;
        }
        public static XY_Position operator +(XY_Position a, XY_Position b)
        {
            XY_Position result = new XY_Position();
            result.x = a.x + b.x;
            result.y = a.y + b.y;
            return result;
        }
        public static XY_Position operator -(XY_Position a, XY_Position b)
        {
            XY_Position result = new XY_Position();
            result.x = a.x - b.x;
            result.y = a.y - b.y;
            return result;
        }
        public static XY_Position operator *(XY_Position a, double b)
        {
            XY_Position result = new XY_Position();
            result.x = a.x * b;
            result.y = a.y * b;
            return result;
        }
        public static XY_Position operator *(double b, XY_Position a)
        {
            XY_Position result = new XY_Position();
            result.x = a.x * b;
            result.y = a.y * b;
            return result;
        }

    };
    public enum COMMAND_TYPE
    {
        MOVE = 0,
        PICK,
        PUT,
        USE,
        SIZE
    }

    public abstract class Character
    {
        public XY_Position xyPosition;
        protected double moveSpeedCoefficient = 1;
        public Direction facingDirection;
        public uint score = 0;
        public Tool? tool = null;
        public Dish? dish = null;
        public Tuple<int, int> id = new Tuple<int, int>(-1, -1);  //first:Agent, second:Client
        public Character(double x, double y)
        {
            xyPosition.x = x;
            xyPosition.y = y;
        }
        public virtual void Move(Direction direction)
        { }
        public virtual void Put()
        { }
        public virtual void Pick()
        { }
        public virtual void Use()
        { }
    }
}