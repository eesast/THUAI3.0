using System;
namespace Constant
{
    public static class CONSTANT
    {
        // private static Constants instance = null;
        // private Constants() { }
        // public static Constants CONSTANT
        // {
        //     get
        //     {
        //         if (instance == null)
        //         {
        //             instance = new Constants();
        //         }
        //         return instance;
        //     }
        // }
        public const int WORLD_MAP_WIDTH = 300;
        public const int WORLD_MAP_HEIGHT = 300;
        public const double MOVE_SPEED = 5;
        public const int FRAME_RATE = 20;
        public const double TIME_INTERVAL = 1 / FRAME_RATE;
        public const double MOVE_DISTANCE_PER_FRAME = MOVE_SPEED / FRAME_RATE;
        private static readonly XY_Position xyRight = new XY_Position(MOVE_DISTANCE_PER_FRAME, 0);
        private static readonly XY_Position xyRightUp = new XY_Position(MOVE_DISTANCE_PER_FRAME / 1.4142135623731, MOVE_DISTANCE_PER_FRAME / 1.4142135623731);
        private static readonly XY_Position xyUp = new XY_Position(0, MOVE_DISTANCE_PER_FRAME);
        private static readonly XY_Position xyLeftUp = new XY_Position(-MOVE_DISTANCE_PER_FRAME / 1.4142135623731, MOVE_DISTANCE_PER_FRAME / 1.4142135623731);
        private static readonly XY_Position xyLeft = new XY_Position(-MOVE_DISTANCE_PER_FRAME, 0);
        private static readonly XY_Position xyLeftDown = new XY_Position(-MOVE_DISTANCE_PER_FRAME / 1.4142135623731, -MOVE_DISTANCE_PER_FRAME / 1.4142135623731);
        private static readonly XY_Position xyDown = new XY_Position(0, -MOVE_DISTANCE_PER_FRAME);
        private static readonly XY_Position xyRightDown = new XY_Position(MOVE_DISTANCE_PER_FRAME / 1.4142135623731, -MOVE_DISTANCE_PER_FRAME / 1.4142135623731);

        public static readonly XY_Position[] OPERATION =
        {
            xyRight,
            xyRightUp,
            xyUp,
            xyLeftUp,
            xyLeft,
            xyLeftDown,
            xyDown,
            xyRightDown
        };
        public const char messageSpiltSeperation = ',';
    }
    public enum DIRECTION
    {
        RIGHT = 0,
        RIGHT_UP,
        UP,
        LEFT_UP,
        LEFT,
        LEFT_DOWN,
        DOWN,
        RIGHT_DOWN,
        SIZE

    };
    public enum OBJECT_TYPE
    {
        AIR,
        PEOPLE,
        BLOCK,
        FOOD,
        TOOLS,
        SIZE
    };
    public enum BLOCK_TYPE
    {
        WALL,
        POT,
        CHOPPING_TABLE,
        BUBBISH_BIN,
        SIZE
    };
    public enum FOOD_TYPE
    {
        APPLE,
        BANANA,
        SIZE
    };
    public enum TOOLS_TYPE
    {
        TIGER_SHOES,
        WAVE_GLUE,
        SIZE
    };
    public struct MAP_CELL
    {
        public OBJECT_TYPE objectType;
        public byte type;
        public MAP_CELL(
            OBJECT_TYPE objectType_t,
            byte type_t
            )
        {
            objectType = objectType_t;
            type = type_t;
        }
        public override string ToString()
        {
            return
            ((byte)objectType).ToString() + CONSTANT.messageSpiltSeperation +
            type.ToString();
        }
    };
    public struct XY_Position
    {
        public double x;
        public double y;
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
        SIZE
    };
    public struct MessageToServer
    {
        public uint senderID;
        public COMMAND_TYPE commandType;
        public Byte parameter1;
        public Byte parameter2;
        public MessageToServer
        (
            uint senderID_t,
            COMMAND_TYPE cmdType,
            Byte param1,
            Byte param2
            )
        {
            senderID = senderID_t;
            commandType = cmdType;
            parameter1 = param1;
            parameter2 = param2;
        }

        public override string ToString()
        {
            return
            senderID.ToString() + CONSTANT.messageSpiltSeperation +
            ((byte)commandType).ToString() + CONSTANT.messageSpiltSeperation +
            parameter1.ToString() + CONSTANT.messageSpiltSeperation +
            parameter2.ToString();
        }
    };
    public struct MessageToClient
    {
        public uint recieverID;
        public XY_Position playerPosition;
        public XY_Position LeftDown;
        public XY_Position RightUp;
        public MAP_CELL cell;
        public MessageToClient
        (
            uint recieverID_t,
            XY_Position playerPosition_t,
            XY_Position LeftDown_t,
            XY_Position RightUp_t,
            MAP_CELL cell_t
        )
        {
            recieverID = recieverID_t;
            playerPosition = playerPosition_t;
            LeftDown = LeftDown_t;
            RightUp = RightUp_t;
            cell = cell_t;
        }

        public override string ToString()
        {
            return
            recieverID.ToString() + CONSTANT.messageSpiltSeperation +
            playerPosition.ToString() + CONSTANT.messageSpiltSeperation +
            LeftDown.ToString() + CONSTANT.messageSpiltSeperation +
            RightUp.ToString() + CONSTANT.messageSpiltSeperation +
            cell.ToString();
        }

    };

    public class Character
    {
        protected XY_Position xyPosition;
        protected double moveSpeed;
        protected uint id;
        public Character(uint id_t, double x, double y)
        {
            id = id_t;
            xyPosition.x = x;
            xyPosition.y = y;
        }
        public virtual void move(DIRECTION direction)
        {
            ;
        }

    }

}