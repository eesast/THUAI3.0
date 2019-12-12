using System;
using System.Collections.Generic;
using System.Text;
using static Map;
using Logic.Constant;
using static Logic.Constant.Constant;
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
            WORLD_MAP[(uint)xyPosition.x, (uint)xyPosition.y, 0] = new People(xyPosition.x, xyPosition.y, id);
        }

        public bool CheckXYPosition(XY_Position xyPos)
        {
            Dictionary<Direction, XY_Position> fourCorners = new Dictionary<Direction, XY_Position>
            {
                { Direction.RightUp, xyPos + CornerOperations[Direction.RightUp] },
                { Direction.LeftUp , xyPos + CornerOperations[Direction.LeftUp] },
                { Direction.LeftDown, xyPos + CornerOperations[Direction.LeftDown] },
                { Direction.RightDown,xyPos + CornerOperations[Direction.RightDown] }
            };
            foreach (var item in fourCorners)
            {
                //Console.WriteLine("corner : " + item.Key.ToString() + " , " + item.Value.ToString());
                if (item.Value.x <= 0 || item.Value.x >= WORLD_MAP_WIDTH - 1
                    || item.Value.y <= 0 || item.Value.y >= WORLD_MAP_HEIGHT - 1)
                {
                    return false;
                }
                if (WORLD_MAP[(uint)item.Value.x, (uint)item.Value.y, 0] != null)
                {
                    //Console.WriteLine("not null");
                    if (WORLD_MAP[(uint)item.Value.x, (uint)item.Value.y, 0] is Block)
                    {
                        //Console.WriteLine("is Block");
                        return false;
                    }
                    if (WORLD_MAP[(uint)item.Value.x, (uint)item.Value.y, 0] is People)
                    {
                        //Console.WriteLine("is People, id: "+ ((People)WORLD_MAP[(uint)item.Value.x, (uint)item.Value.y, 0]).id.ToString()+" , xyPosition : "+ ((People)WORLD_MAP[(uint)item.Value.x, (uint)item.Value.y, 0]).xyPosition.ToString());
                        if (((People)WORLD_MAP[(uint)item.Value.x, (uint)item.Value.y, 0]).id != id
                            && Math.Abs(((People)WORLD_MAP[(uint)item.Value.x, (uint)item.Value.y, 0]).xyPosition.x - item.Value.x) <= 0.5
                            && Math.Abs(((People)WORLD_MAP[(uint)item.Value.x, (uint)item.Value.y, 0]).xyPosition.y - item.Value.y) <= 0.5)
                        {
                            return false;
                        }
                    }
                }

            }
            return true;
        }
        private void CorrectPosition()
        {
            XY_Position[] searchers = new XY_Position[8];
            for (int i = 0; i < 8; i++)
            {
                searchers[i] = new XY_Position(xyPosition.x, xyPosition.y);
            }
            while (true)
            {
                {
                    uint i = 0;
                    foreach (var item in Operations)
                    {
                        searchers[i] = searchers[i] + item.Value;
                        i++;
                    }
                }
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
            XY_Position aim = moveSpeedCoefficient * MoveOperations[facingDirection] + xyPosition;
            double minInterval = moveSpeedCoefficient * MoveDistancePerFrame;
            //Console.WriteLine("init minInterval : " + minInterval.ToString());
            bool isSuccessful = true;
            if (WORLD_MAP[(uint)aim.x, (uint)aim.y, 0] != null
                && !(WORLD_MAP[(uint)aim.x, (uint)aim.y, 0] is People
                    && ((People)WORLD_MAP[(uint)aim.x, (uint)aim.y, 0]).id == id))
            {
                isSuccessful = false;
                if (Convert.ToBoolean((byte)direction & 1))
                {
                    Direction[] boundDirection = new Direction[2] { (Direction)(((int)direction - 1) % 8), (Direction)(((int)direction + 1) % 8) };
                    minInterval = Math.Max(
                        ((WORLD_MAP[(uint)aim.x, (uint)aim.y, 0].xyPosition - CornerOperations[boundDirection[0]] - (this.xyPosition + CornerOperations[boundDirection[0]])).get(Convert.ToBoolean((int)boundDirection[0] & 2))) * (Convert.ToBoolean((int)boundDirection[0] & 4) ? -1 : 1),
                        ((WORLD_MAP[(uint)aim.x, (uint)aim.y, 0].xyPosition - CornerOperations[boundDirection[1]] - (this.xyPosition + CornerOperations[boundDirection[1]])).get(Convert.ToBoolean((int)boundDirection[1] & 2))) * (Convert.ToBoolean((int)boundDirection[1] & 4) ? -1 : 1)
                        );
                }
                else
                {
                    minInterval = (WORLD_MAP[(uint)aim.x, (uint)aim.y, 0].xyPosition - CornerOperations[direction] - (this.xyPosition + CornerOperations[direction])).get(Convert.ToBoolean((int)direction & 2)) * (Convert.ToBoolean((int)direction & 4) ? -1 : 1);
                }
            }
            else
            {
                for (int i = 5; i <= 11; i++)
                {
                    XY_Position toCheck = new XY_Position((uint)aim.x + 0.5, (uint)aim.y + 0.5) + Operations[(Direction)(((byte)facingDirection + i) % 8)];
                    if (WORLD_MAP[(uint)toCheck.x, (uint)toCheck.y, 0] != null)
                    {
                        if ((WORLD_MAP[(uint)toCheck.x, (uint)toCheck.y, 0] is Block
                            || (WORLD_MAP[(uint)toCheck.x, (uint)toCheck.y, 0] is People && ((People)WORLD_MAP[(uint)toCheck.x, (uint)toCheck.y, 0]).id != id))
                            && Math.Abs(WORLD_MAP[(uint)toCheck.x, (uint)toCheck.y, 0].xyPosition.x - aim.x) < 1
                            && Math.Abs(WORLD_MAP[(uint)toCheck.x, (uint)toCheck.y, 0].xyPosition.y - aim.y) < 1)
                        {
                            isSuccessful = false;
                            double tempMinInterval = 1;
                            if (Convert.ToBoolean((byte)direction & 1))
                            {
                                Direction[] boundDirection = new Direction[2] { (Direction)(((int)direction - 1) % 8), (Direction)(((int)direction + 1) % 8) };
                                tempMinInterval = Math.Max(
                                    ((WORLD_MAP[(uint)toCheck.x, (uint)toCheck.y, 0].xyPosition - CornerOperations[boundDirection[0]] - (this.xyPosition + CornerOperations[boundDirection[0]])).get(Convert.ToBoolean((int)boundDirection[0] & 2))) * (Convert.ToBoolean((int)boundDirection[0] & 4) ? -1 : 1),
                                    ((WORLD_MAP[(uint)toCheck.x, (uint)toCheck.y, 0].xyPosition - CornerOperations[boundDirection[1]] - (this.xyPosition + CornerOperations[boundDirection[1]])).get(Convert.ToBoolean((int)boundDirection[1] & 2))) * (Convert.ToBoolean((int)boundDirection[1] & 4) ? -1 : 1)
                                    );
                            }
                            else
                            {
                                //Console.WriteLine("direction : " + ((int)direction).ToString());
                                //Console.WriteLine("Block : " + WORLD_MAP[(uint)toCheck.x, (uint)toCheck.y, 0].xyPosition.ToString());
                                //Console.WriteLine("Corner : " + CornerOperations[direction].ToString());

                                tempMinInterval = (WORLD_MAP[(uint)toCheck.x, (uint)toCheck.y, 0].xyPosition - CornerOperations[direction] - (this.xyPosition + CornerOperations[direction])).get(Convert.ToBoolean((int)direction & 2)) * (Convert.ToBoolean((int)direction & 4) ? -1 : 1);
                                //Console.WriteLine("tempInInterval : " + tempMinInterval.ToString());
                            }
                            
                            if (tempMinInterval < minInterval)
                            {
                                minInterval = tempMinInterval;
                            }
                        }
                    }
                }
            }

            if(!isSuccessful)
            {
                aim = (minInterval * 2) * CornerOperations[direction] + xyPosition;
            }

            if ((uint)xyPosition.x != (uint)aim.x || (uint)xyPosition.y != (uint)aim.y)
            {
                WORLD_MAP[(uint)xyPosition.x, (uint)xyPosition.y, 0].RemoveSelf();
                xyPosition = aim;
                WORLD_MAP[(uint)xyPosition.x, (uint)xyPosition.y, 0] = new People(xyPosition.x, xyPosition.y, id);
                //Console.WriteLine("new self , " + xyPosition.ToString());
            }
            else
            {
                xyPosition = aim;
                WORLD_MAP[(uint)xyPosition.x, (uint)xyPosition.y, 0].xyPosition = xyPosition;
            }

            Program.server.ServerCommunication.SendMessage(
                new ServerMessage
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
                });
            Constants.Debug("player " + id.ToString() + " 's position : " + xyPosition.ToString());
        }
        public override void Put()
        {
            if (dish != null)
            {

            }
        }
    }
}
