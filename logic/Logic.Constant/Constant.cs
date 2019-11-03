using System;
using static Map;
namespace Logic.Constant
{
    public static class CONSTANT
    {

        public const double MOVE_SPEED = 5;
        public const int FRAME_RATE = 20;
        public const double TIME_INTERVAL = 1 / FRAME_RATE;
        public const double MOVE_DISTANCE_PER_FRAME = MOVE_SPEED / FRAME_RATE;
        public static readonly XY_Position[] OPERATION = {
            new XY_Position (MOVE_DISTANCE_PER_FRAME, 0),
            new XY_Position(MOVE_DISTANCE_PER_FRAME / 1.4142135623731, MOVE_DISTANCE_PER_FRAME / 1.4142135623731),
            new XY_Position(0, MOVE_DISTANCE_PER_FRAME),
            new XY_Position(-MOVE_DISTANCE_PER_FRAME / 1.4142135623731, MOVE_DISTANCE_PER_FRAME / 1.4142135623731),
            new XY_Position(-MOVE_DISTANCE_PER_FRAME, 0),
            new XY_Position(-MOVE_DISTANCE_PER_FRAME / 1.4142135623731, -MOVE_DISTANCE_PER_FRAME / 1.4142135623731),
            new XY_Position(0, -MOVE_DISTANCE_PER_FRAME),
            new XY_Position(MOVE_DISTANCE_PER_FRAME / 1.4142135623731, -MOVE_DISTANCE_PER_FRAME / 1.4142135623731)
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
            WORLD_MAP[(uint)xyPosition.x, (uint)xyPosition.y].Remove(this);
        }
    }
    public class People : Obj
    {
        private Tuple<int, int> _id;
        public Tuple<int, int> id { get { return _id; } }
        public Dish dish;
        public Tool tool;
        public Direction facingDirection;
        public People(double x_t, double y_t, Tuple<int, int> id_t) : base(x_t, y_t)
        {
            _id = id_t;
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

        private Type _type;
        public new Type type { get { return _type; } }
        public Block(double x_t, double y_t, Type type_t) : base(x_t, y_t)
        {
            _type = type_t;
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
        public Type _type;
        public new Type type { get { return _type; } }
        public Dish(double x_t, double y_t, Type type_t) : base(x_t, y_t)
        {
            _type = type_t;
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
        public Type _type;
        public new Type type { get { return _type; } }
        public Tool(double x_t, double y_t, Type type_t) : base(x_t, y_t)
        {
            _type = type_t;
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
        public Type _type;
        public new Type type { get { return _type; } }

        public Trigger(double x_t, double y_t, Type type_t) : base(x_t, y_t)
        {
            _type = type_t;
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
            x.ToString("0.000") + CONSTANT.messageSpiltSeperation +
                y.ToString("0.000");
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
    };

    public enum COMMAND_TYPE
    {
        MOVE = 0,
        PICK,
        PUT,
        USE,
        SIZE
    }

    public class Character
    {
        public XY_Position xyPosition;
        protected double moveSpeed;
        public Direction facingDirection;
        public Tuple<int, int> id = new Tuple<int, int>(-1, -1);  //first:Agent, second:Client
        public Character(double x, double y)
        {
            xyPosition.x = x;
            xyPosition.y = y;
        }
        public virtual void Move(Direction direction)
        {
            ;
        }
    }
}