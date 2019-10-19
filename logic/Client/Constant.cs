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

        public const int WORLD_MAP_WIDTH = 100;
        public const int WORLD_MAP_HEIGHT = 100;
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
    public class Obj
    {
        public enum TYPE
        {
            AIR,
            PEOPLE,
            BLOCK,
            FOOD,
            TOOLS,
            SIZE

        };
        public const byte BYTE_LENGTH = XY_Position.BYTE_LENGTH + 1;
        public XY_Position xyPosition;
        public TYPE type;
        public const int width = 1;
        public const int height = 1;
        public Obj(double x_t, double y_t)
        {
            xyPosition.x = x_t;
            xyPosition.y = y_t;
        }
        public Obj()
        { }
        public override string ToString()
        {
            return ((byte)type).ToString();
        }
        public void RemoveSelf()
        {
            //Server.Program.WORLD_MAP[(uint)xyPosition.x, (uint)xyPosition.y].Remove(this);
        }
        public byte[] ToBytes()
        {
            byte[] xyBytes = xyPosition.ToBytes();
            byte[] result = new byte[BYTE_LENGTH];
            Buffer.BlockCopy(xyBytes, 0, result, 0, xyBytes.Length);
            result[xyBytes.Length] = (byte)type;
            return result;
        }
        public static Obj FromBytes(byte[] source, uint startIndex, out bool isSuccess)
        {
            isSuccess = false;
            if (source.Length - startIndex < BYTE_LENGTH)
            {
                isSuccess = false;
                return new Obj();
            }
            if (source[BYTE_LENGTH + startIndex - 1] >= (byte)TYPE.SIZE)
            {
                isSuccess = false;
                return new Obj();
            }
            isSuccess = false;
            XY_Position xyPos = XY_Position.FromBytes(source, startIndex, out isSuccess);
            return new Obj(xyPos.x, xyPos.y);
        }
    }
    public class People : Obj
    {
        private byte _id;
        public byte id
        {
            get
            {
                return _id;
            }
        }
        public People(
            double x_t, double y_t,
            byte id_t
        )
        :
         base(x_t, y_t)
        {
            _id = id_t;
        }
    }
    public class Block : Obj
    {
        public new enum TYPE
        {
            WALL,
            POT,
            CHOPPING_TABLE,
            BUBBISH_BIN,
            SIZE
        };
        public Block(double x_t, double y_t) : base(x_t, y_t)
        {

        }
    }
    public class Food : Obj
    {
        public new enum TYPE
        {
            APPLE,
            BANANA,
            SIZE
        };
        public Food(double x_t, double y_t) : base(x_t, y_t)
        {

        }
    }
    public class Tools : Obj
    {
        public new enum TYPE
        {
            TIGER_SHOES,
            WAVE_GLUE,
            SIZE
        };
        public Tools(double x_t, double y_t) : base(x_t, y_t)
        {

        }
    }
    // public struct MAP_CELL
    // {
    //     public Obj.TYPE objectType;
    //     public byte type;
    //     public MAP_CELL(
    //         Obj.TYPE objectType_t,
    //         byte type_t
    //         )
    //     {
    //         objectType = objectType_t;
    //         type = type_t;
    //     }
    //     public override string ToString()
    //     {
    //         return
    //         ((byte)objectType).ToString() + CONSTANT.messageSpiltSeperation +
    //         type.ToString();
    //     }
    // };
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
        public byte[] ToBytes()
        {
            byte[] result = new byte[BYTE_LENGTH];
            Buffer.BlockCopy(BitConverter.GetBytes(x), 0, result, 0, 8);
            Buffer.BlockCopy(BitConverter.GetBytes(y), 0, result, 8, 8);
            return result;
        }
        public static XY_Position FromBytes(byte[] source, uint startIndex, out bool isSuccess)
        {
            if (source.Length - startIndex < BYTE_LENGTH)
            {
                isSuccess = false;
                return new XY_Position();
            }
            isSuccess = true;
            return new XY_Position(BitConverter.ToDouble(source, (int)startIndex + 0), BitConverter.ToDouble(source, (int)startIndex + 8));
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
        public byte senderID;
        public COMMAND_TYPE commandType;
        public Byte parameter1;
        public Byte parameter2;
        public MessageToServer
        (
            byte senderID_t,
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
        public byte[] ToBytes()
        {
            return new byte[] { senderID, (byte)commandType, parameter1, parameter2 };
        }
        public static MessageToServer FromBytes(byte[] source, uint startIndex, out bool isSuccess)
        {
            if (source.Length - startIndex < 4)
            {
                isSuccess = false;
                return new MessageToServer(0, (COMMAND_TYPE)0, 0, 0);
            }
            if (source[1 + startIndex] >= (byte)COMMAND_TYPE.SIZE)
            {
                isSuccess = false;
                return new MessageToServer(0, (COMMAND_TYPE)0, 0, 0);
            }
            isSuccess = true;
            return new MessageToServer(source[startIndex + 0], (COMMAND_TYPE)source[startIndex + 1], source[startIndex + 2], source[startIndex + 3]);
        }

    };
    public struct MessageToClient
    {
        public byte recieverID;
        public XY_Position playerPosition;
        public bool isAdd;
        public Obj obj;
        public const byte BYTE_LENGTH = XY_Position.BYTE_LENGTH + Obj.BYTE_LENGTH + 2;
        public MessageToClient
        (
            byte recieverID_t,
            XY_Position playerPosition_t,
            bool isAdd_t,
            Obj obj_t
        )
        {
            recieverID = recieverID_t;
            playerPosition = playerPosition_t;
            isAdd = isAdd_t;
            obj = obj_t;
        }

        public override string ToString()
        {
            return
            recieverID.ToString() + CONSTANT.messageSpiltSeperation +
            playerPosition.ToString() + CONSTANT.messageSpiltSeperation +
            isAdd.ToString() + CONSTANT.messageSpiltSeperation +
            obj.ToString();
        }
        public byte[] ToBytes()
        {
            byte[] xyBytes = playerPosition.ToBytes();
            byte[] objBytes = obj.ToBytes();
            byte[] result = new byte[BYTE_LENGTH];
            result[0] = recieverID;

            Buffer.BlockCopy(xyBytes, 0, result, 1, xyBytes.Length);
            result[xyBytes.Length + 1] = Convert.ToByte(isAdd);
            Buffer.BlockCopy(objBytes, 0, result, xyBytes.Length + 2, objBytes.Length);
            return result;
        }
        public static MessageToClient FromBytes(byte[] source, uint startIndex, out bool isSuccess)
        {
            if (source.Length - startIndex < BYTE_LENGTH)
            {
                isSuccess = false;
                return new MessageToClient();
            }
            byte recieverID_t = source[0];
            XY_Position playerPosition_t = XY_Position.FromBytes(source, startIndex + 1, out isSuccess);
            bool isAdd_t = Convert.ToBoolean(source[startIndex + 1 + XY_Position.BYTE_LENGTH]);
            Obj obj_t = Obj.FromBytes(source, startIndex + 1 + XY_Position.BYTE_LENGTH + 1, out isSuccess);
            return new MessageToClient(recieverID_t, playerPosition_t, isAdd_t, obj_t);
        }
    };

    public class Character
    {
        protected XY_Position xyPosition;
        protected double moveSpeed;
        protected byte id;
        public Character(byte id_t, double x, double y)
        {
            id = id_t;
            xyPosition.x = x;
            xyPosition.y = y;
        }
        public virtual void Move(DIRECTION direction)
        {
            ;
        }

    }

}