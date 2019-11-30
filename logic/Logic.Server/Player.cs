using System;
using System.Collections.Generic;
using System.Text;
using static Map;
using Logic.Constant;
using static Logic.Constant.CONSTANT;
using Communication.Proto;
using Communication.Server;

namespace Logic.Server
{
    class Player : Character
    {
        public Player(Tuple<int, int> id_t, double x, double y) :
            base(x, y)
        {
            id = id_t;
            CorrectPosition();
            WORLD_MAP[(uint)xyPosition.x, (uint)xyPosition.y].Add(new People(xyPosition.x, xyPosition.y, id));
        }

        static readonly Dictionary<Direction, XY_Position> operation = new Dictionary<Direction, XY_Position>
        {
            { Direction.RightUp, new XY_Position(0.5, 0.5) },
            { Direction.LeftUp , new XY_Position(-0.5, 0.5) },
            { Direction.LeftDown, new XY_Position(-0.5, -0.5) },
            { Direction.RightDown,new XY_Position(0.5, -0.5) }
        };
        public bool CheckXYPosition(XY_Position xyPos)
        {
            XY_Position RightUp = xyPos + operation[Direction.RightUp];
            XY_Position LeftUp = xyPos + operation[Direction.LeftUp];
            XY_Position LeftDown = xyPos + operation[Direction.LeftDown];
            XY_Position RightDown = xyPos + operation[Direction.RightDown];

            if (WORLD_MAP[(uint)RightUp.x, (uint)RightUp.y].Count > 0)
            {
                if (WORLD_MAP[(uint)RightUp.x, (uint)RightUp.y][0] is Block)
                {
                    return false;
                }
                if (WORLD_MAP[(uint)RightUp.x, (uint)RightUp.y][0] is People)
                {
                    if (((People)WORLD_MAP[(uint)RightUp.x, (uint)RightUp.y][0]).id != id)
                    {
                        return false;
                    }
                }
            }
            if (WORLD_MAP[(uint)LeftUp.x, (uint)LeftUp.y].Count > 0)
            {
                if (WORLD_MAP[(uint)LeftUp.x, (uint)LeftUp.y][0] is Block)
                {
                    return false;
                }
                if (WORLD_MAP[(uint)LeftUp.x, (uint)LeftUp.y][0] is People)
                {
                    if (((People)WORLD_MAP[(uint)LeftUp.x, (uint)LeftUp.y][0]).id != id)
                    {
                        return false;
                    }
                }
            }
            if (WORLD_MAP[(uint)LeftDown.x, (uint)LeftDown.y].Count > 0)
            {
                if (WORLD_MAP[(uint)LeftDown.x, (uint)LeftDown.y][0] is Block)
                {
                    return false;
                }
                if (WORLD_MAP[(uint)LeftDown.x, (uint)LeftDown.y][0] is People)
                {
                    if (((People)WORLD_MAP[(uint)LeftDown.x, (uint)LeftDown.y][0]).id != id)
                    {
                        return false;
                    }
                }
            }
            if (WORLD_MAP[(uint)RightDown.x, (uint)RightDown.y].Count > 0)
            {
                if (WORLD_MAP[(uint)RightDown.x, (uint)RightDown.y][0] is Block)
                {
                    return false;
                }
                if (WORLD_MAP[(uint)RightDown.x, (uint)RightDown.y][0] is People)
                {
                    if (((People)WORLD_MAP[(uint)RightDown.x, (uint)RightDown.y][0]).id != id)
                    {
                        return false;
                    }
                }
            }
            return true;
        }
        private void CorrectPosition()
        {
            if (xyPosition.x <= 0 || xyPosition.x >= WORLD_MAP_WIDTH - 1 || xyPosition.y <= 0 || xyPosition.y >= WORLD_MAP_HEIGHT - 1)
            {
                xyPosition.x = WORLD_MAP_WIDTH * 0.5;
                xyPosition.y = WORLD_MAP_HEIGHT * 0.5;
            }
            if (CheckXYPosition(xyPosition))
                return;
            XY_Position[] searchers = new XY_Position[4];
            for (int i = 0; i < 4; i++)
            {
                searchers[i] = new XY_Position(xyPosition.x, xyPosition.y);
            }
            while (true)
            {
                searchers[0] = searchers[0] + new XY_Position(1, 0);
                searchers[1] = searchers[1] + new XY_Position(0, 1);
                searchers[2] = searchers[2] + new XY_Position(-1, 0);
                searchers[3] = searchers[3] + new XY_Position(0, -1);

                foreach (XY_Position searcher in searchers)
                {
                    if (CheckXYPosition(searcher))
                    {
                        xyPosition = searcher;
                        return;
                    }
                }
            }
        }
        public override void Move(Direction direction)
        {
            facingDirection = direction;
            XY_Position aim = OPERATION[(uint)direction] + xyPosition;
            if (CheckXYPosition(aim))
            {
                if ((uint)xyPosition.x != (uint)aim.x || (uint)xyPosition.y != (uint)aim.y)
                {
                    WORLD_MAP[(uint)xyPosition.x, (uint)xyPosition.y][0].RemoveSelf();
                    xyPosition = aim;
                    WORLD_MAP[(uint)xyPosition.x, (uint)xyPosition.y].Insert(0, new People(xyPosition.x, xyPosition.y, id));
                }
                else
                {
                    xyPosition = aim;
                    WORLD_MAP[(uint)xyPosition.x, (uint)xyPosition.y][0].xyPosition = xyPosition;
                }

                Program.server.ServerCommunication.SendMessage(new ServerMessage
                {
                    Agent = -2,
                    Client = -2,
                    Message = new MessageToClient
                    {
                        PlayerIDAgent = id.Item1,
                        PlayerIDClient = id.Item2,
                        PlayerPositionX = BitConverter.DoubleToInt64Bits(xyPosition.x),
                        PlayerPositionY = BitConverter.DoubleToInt64Bits(xyPosition.y),
                        FacingDirection = (int)facingDirection,
                        IsAdd = false,
                        ObjType = 0,
                        ObjType2 = 0
                    }
                }
                );

            }
            Console.WriteLine("player {0} 's position : {1}", id.ToString(), xyPosition.ToString());
        }
        public override void Put()
        {
            if(dish != null)
            {

            }
        }
    }
}
