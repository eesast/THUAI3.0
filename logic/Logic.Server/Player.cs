using System;
using System.Collections.Generic;
using System.Text;
using Logic.Constant;
using static Logic.Constant.Constant;
using THUnity2D;
using static Logic.Constant.Map;
using static THUnity2D.Tools;
using Communication.Proto;
using Communication.Server;
using Timer;
using System.Configuration;

namespace Logic.Server
{
    class Player : Character
    {
        public System.Threading.Timer timer;
        public System.Threading.Timer StopTimer;
        public COMMAND_TYPE status;
        public TimeSpan LastActTime;
        public TimeSpan LeftTime;

        public Player(double x, double y) :
            base(x, y)
        {
            status = COMMAND_TYPE.STOP;
            LastActTime = Time.GameTime();
            //timer = new System.Threading.Timer(new System.Threading.TimerCallback(Move), (object)1, -1, 0);
            StopTimer = new System.Threading.Timer(new System.Threading.TimerCallback(Stop), (object)1, 0, 0);
        }
        public void ExecuteMessage(CommunicationImpl communication, MessageToServer msg)
        {
            if (msg.CommandType < 0 || msg.CommandType >= (int)COMMAND_TYPE.SIZE)
                return;
            if (msg.CommandType == (int)COMMAND_TYPE.MOVE && msg.Parameter1 >= 0 && msg.Parameter1 < (int)Direction.Size)
            {
                Move(new MoveEventArgs(msg.Parameter1 * Math.PI / 4, MoveDistancePerFrame));
                MessageToClient msgToCl = new MessageToClient();
                msgToCl.GameObjectsList.Add(this.ID, new GameObjects
                {
                    Type = OBJECTS_TYPE.Player,
                    Position = new XY_Position { X = this.Position.x, Y = this.Position.y },
                    Direction = (DIRECTION)(int)this.facingDirection
                }); ;
                Program.server.ServerCommunication.SendMessage(new ServerMessage
                {
                    Agent = -2,
                    Client = -2,
                    Message = msgToCl
                }
                );
            }
        }
        //public bool CheckXYPosition(XYPosition xyPos)
        //{
        //    Dictionary<Direction, XYPosition> fourCorners = new Dictionary<Direction, XYPosition>
        //    {
        //        { Direction.RightUp, xyPos + CornerOperations[Direction.RightUp] },
        //        { Direction.LeftUp , xyPos + CornerOperations[Direction.LeftUp] },
        //        { Direction.LeftDown, xyPos + CornerOperations[Direction.LeftDown] },
        //        { Direction.RightDown,xyPos + CornerOperations[Direction.RightDown] }
        //    };
        //    foreach (var item in fourCorners)
        //    {
        //        //Console.WriteLine("corner : " + item.Key.ToString() + " , " + item.Value.ToString());
        //        if (item.Value.x <= 0 || item.Value.x >= WORLD_MAP_WIDTH - 1
        //            || item.Value.y <= 0 || item.Value.y >= WORLD_MAP_HEIGHT - 1)
        //        {
        //            return false;
        //        }
        //        if (WORLD_MAP[(uint)item.Value.x, (uint)item.Value.y, 0] != null)
        //        {
        //            //Console.WriteLine("not null");
        //            if (WORLD_MAP[(uint)item.Value.x, (uint)item.Value.y, 0] is Block)
        //            {
        //                //Console.WriteLine("is Block");
        //                return false;
        //            }
        //            if (WORLD_MAP[(uint)item.Value.x, (uint)item.Value.y, 0] is People)
        //            {
        //                //Console.WriteLine("is People, id: "+ ((People)WORLD_MAP[(uint)item.Value.x, (uint)item.Value.y, 0]).id.ToString()+" , xyPosition : "+ ((People)WORLD_MAP[(uint)item.Value.x, (uint)item.Value.y, 0]).xyPosition.ToString());
        //                if (((People)WORLD_MAP[(uint)item.Value.x, (uint)item.Value.y, 0]).id != id
        //                    && Math.Abs(((People)WORLD_MAP[(uint)item.Value.x, (uint)item.Value.y, 0]).xyPosition.x - item.Value.x) <= 0.5
        //                    && Math.Abs(((People)WORLD_MAP[(uint)item.Value.x, (uint)item.Value.y, 0]).xyPosition.y - item.Value.y) <= 0.5)
        //                {
        //                    return false;
        //                }
        //            }
        //        }

        //    }
        //    return true;
        //}
        //private void CorrectPosition()
        //{
        //    XYPosition[] searchers = new XYPosition[8];
        //    for (int i = 0; i < 8; i++)
        //    {
        //        searchers[i] = new XYPosition(xyPosition.x, xyPosition.y);
        //    }
        //    while (true)
        //    {
        //        {
        //            uint i = 0;
        //            foreach (var item in Operations)
        //            {
        //                searchers[i] = searchers[i] + item.Value;
        //                i++;
        //            }
        //        }
        //        foreach (XYPosition searcher in searchers)
        //        {
        //            if (CheckXYPosition(searcher))
        //            {
        //                xyPosition = searcher;
        //                return;
        //            }
        //        }
        //    }
        //}
        //public override void Move(Direction direction)
        //{
        //    facingDirection = direction;
        //    XYPosition aim = moveSpeedCoefficient * MoveOperations[facingDirection] + xyPosition;
        //    double minInterval = moveSpeedCoefficient * MoveDistancePerFrame;
        //    //Console.WriteLine("init minInterval : " + minInterval.ToString());
        //    bool isSuccessful = true;
        //    if (WORLD_MAP[(uint)aim.x, (uint)aim.y, 0] != null
        //        && !(WORLD_MAP[(uint)aim.x, (uint)aim.y, 0] is People
        //            && ((People)WORLD_MAP[(uint)aim.x, (uint)aim.y, 0]).id == id))
        //    {
        //        isSuccessful = false;
        //        if (Convert.ToBoolean((byte)direction & 1))
        //        {
        //            Direction[] boundDirection = new Direction[2] { (Direction)(((int)direction - 1) % 8), (Direction)(((int)direction + 1) % 8) };
        //            minInterval = Math.Max(
        //                ((WORLD_MAP[(uint)aim.x, (uint)aim.y, 0].xyPosition - CornerOperations[boundDirection[0]] - (this.xyPosition + CornerOperations[boundDirection[0]])).get(Convert.ToBoolean((int)boundDirection[0] & 2))) * (Convert.ToBoolean((int)boundDirection[0] & 4) ? -1 : 1),
        //                ((WORLD_MAP[(uint)aim.x, (uint)aim.y, 0].xyPosition - CornerOperations[boundDirection[1]] - (this.xyPosition + CornerOperations[boundDirection[1]])).get(Convert.ToBoolean((int)boundDirection[1] & 2))) * (Convert.ToBoolean((int)boundDirection[1] & 4) ? -1 : 1)
        //                );
        //        }
        //        else
        //        {
        //            minInterval = (WORLD_MAP[(uint)aim.x, (uint)aim.y, 0].xyPosition - CornerOperations[direction] - (this.xyPosition + CornerOperations[direction])).get(Convert.ToBoolean((int)direction & 2)) * (Convert.ToBoolean((int)direction & 4) ? -1 : 1);
        //        }
        //    }
        //    else
        //    {
        //        for (int i = 5; i <= 11; i++)
        //        {
        //            XYPosition toCheck = new XYPosition((uint)aim.x + 0.5, (uint)aim.y + 0.5) + Operations[(Direction)(((byte)facingDirection + i) % 8)];
        //            if (WORLD_MAP[(uint)toCheck.x, (uint)toCheck.y, 0] != null)
        //            {
        //                if ((WORLD_MAP[(uint)toCheck.x, (uint)toCheck.y, 0] is Block
        //                    || (WORLD_MAP[(uint)toCheck.x, (uint)toCheck.y, 0] is People && ((People)WORLD_MAP[(uint)toCheck.x, (uint)toCheck.y, 0]).id != id))
        //                    && Math.Abs(WORLD_MAP[(uint)toCheck.x, (uint)toCheck.y, 0].xyPosition.x - aim.x) < 1
        //                    && Math.Abs(WORLD_MAP[(uint)toCheck.x, (uint)toCheck.y, 0].xyPosition.y - aim.y) < 1)
        //                {
        //                    isSuccessful = false;
        //                    double tempMinInterval = 1;
        //                    if (Convert.ToBoolean((byte)direction & 1))
        //                    {
        //                        Direction[] boundDirection = new Direction[2] { (Direction)(((int)direction - 1) % 8), (Direction)(((int)direction + 1) % 8) };
        //                        tempMinInterval = Math.Max(
        //                            ((WORLD_MAP[(uint)toCheck.x, (uint)toCheck.y, 0].xyPosition - CornerOperations[boundDirection[0]] - (this.xyPosition + CornerOperations[boundDirection[0]])).get(Convert.ToBoolean((int)boundDirection[0] & 2))) * (Convert.ToBoolean((int)boundDirection[0] & 4) ? -1 : 1),
        //                            ((WORLD_MAP[(uint)toCheck.x, (uint)toCheck.y, 0].xyPosition - CornerOperations[boundDirection[1]] - (this.xyPosition + CornerOperations[boundDirection[1]])).get(Convert.ToBoolean((int)boundDirection[1] & 2))) * (Convert.ToBoolean((int)boundDirection[1] & 4) ? -1 : 1)
        //                            );
        //                    }
        //                    else
        //                    {
        //                        //Console.WriteLine("direction : " + ((int)direction).ToString());
        //                        //Console.WriteLine("Block : " + WORLD_MAP[(uint)toCheck.x, (uint)toCheck.y, 0].xyPosition.ToString());
        //                        //Console.WriteLine("Corner : " + CornerOperations[direction].ToString());

        //                        tempMinInterval = (WORLD_MAP[(uint)toCheck.x, (uint)toCheck.y, 0].xyPosition - CornerOperations[direction] - (this.xyPosition + CornerOperations[direction])).get(Convert.ToBoolean((int)direction & 2)) * (Convert.ToBoolean((int)direction & 4) ? -1 : 1);
        //                        //Console.WriteLine("tempInInterval : " + tempMinInterval.ToString());
        //                    }
        //                    if (tempMinInterval < minInterval)
        //                    {
        //                        minInterval = tempMinInterval;
        //                    }
        //                }
        //            }
        //        }
        //    }

        //    if (!isSuccessful)
        //    {
        //        aim = (minInterval * 2) * CornerOperations[direction] + xyPosition;
        //    }

        //    if ((uint)xyPosition.x != (uint)aim.x || (uint)xyPosition.y != (uint)aim.y)
        //    {
        //        WORLD_MAP[(uint)xyPosition.x, (uint)xyPosition.y, 0].RemoveSelf();
        //        xyPosition = aim;
        //        WORLD_MAP[(uint)xyPosition.x, (uint)xyPosition.y, 0] = new People(xyPosition.x, xyPosition.y, id);
        //        //Console.WriteLine("new self , " + xyPosition.ToString());
        //    }
        //    else
        //    {
        //        xyPosition = aim;
        //        WORLD_MAP[(uint)xyPosition.x, (uint)xyPosition.y, 0].xyPosition = xyPosition;
        //    }

        //    Program.server.ServerCommunication.SendMessage(
        //        new ServerMessage
        //        {
        //            Agent = -2,
        //            Client = -2,
        //            Message = new MessageToClient
        //            {
        //                PlayerIDAgent = id.Item1,
        //                PlayerIDClient = id.Item2,
        //                PlayerPositionX = BitConverter.DoubleToInt64Bits(xyPosition.x),
        //                PlayerPositionY = BitConverter.DoubleToInt64Bits(xyPosition.y),
        //                FacingDirection = (int)facingDirection,
        //                IsAdd = false,
        //                ObjType = 0,
        //                ObjType2 = 0
        //            }
        //        });
        //    Constants.Debug("player " + id.ToString() + " 's position : " + xyPosition.ToString());
        //}
        public override void Pick()
        {
            XYPosition Check()
            {
                bool CheckItem(XYPosition xypos)
                {
                    for (int i = 0; i < WorldMap.Grid[(int)xypos.x, (int)xypos.y].unblockableObjects.Count; i++)
                    {
                        if (WorldMap.Grid[(int)xypos.x, (int)xypos.y].unblockableObjects[i] is Dish
                            || WorldMap.Grid[(int)xypos.x, (int)xypos.y].unblockableObjects[i] is Tool
                            || WorldMap.Grid[(int)xypos.x, (int)xypos.y].blockableObject is Block) { return true; }
                    }
                    //等地图做完写
                    return false;
                }
                XYPosition xyPosition1 = Position;
                if (facingDirection == Direction.Down) xyPosition1.y--;
                else if (facingDirection == Direction.Left) xyPosition1.x--;
                else if (facingDirection == Direction.LeftDown) { xyPosition1.y--; xyPosition1.x--; }
                else if (facingDirection == Direction.LeftUp) { xyPosition1.y++; xyPosition1.x--; }
                else if (facingDirection == Direction.Right) xyPosition1.x++;
                else if (facingDirection == Direction.RightDown) { xyPosition1.y--; xyPosition1.x++; }
                else if (facingDirection == Direction.RightUp) { xyPosition1.y++; xyPosition1.x++; }
                else if (facingDirection == Direction.Up) xyPosition1.y++;

                if (CheckItem(Position))
                {
                    return Position;
                }
                if (CheckItem(xyPosition1))
                {
                    return xyPosition1;
                }
                return new XYPosition(-1, -1);
            }
            void GetItem(XYPosition xypos)
            {
                for (int i = 0; i < WorldMap.Grid[(int)xypos.x, (int)xypos.y].unblockableObjects.Count; i++)
                {
                    if (WorldMap.Grid[(int)xypos.x, (int)xypos.y].unblockableObjects[i] is Dish)
                    {
                        dish = ((Dish)WorldMap.Grid[(int)xypos.x, (int)xypos.y].unblockableObjects[i]).GetDish(dish);
                    }
                    else if (WorldMap.Grid[(int)xypos.x, (int)xypos.y].unblockableObjects[i] is Tool)
                    {
                        Console.Write("GetTool!");
                        DeFunction(tool);
                        tool = ((Tool)WorldMap.Grid[(int)xypos.x, (int)xypos.y].unblockableObjects[i]).GetTool(tool);
                        Function(tool);
                    }
                    else if (WorldMap.Grid[(int)xypos.x, (int)xypos.y].unblockableObjects[i] is Block
                        && ((Block)WorldMap.Grid[(int)xypos.x, (int)xypos.y].unblockableObjects[i]).type == Block.Type.FoodPoint)
                    {
                        dish = ((Dish)WorldMap.Grid[(int)xypos.x, (int)xypos.y].unblockableObjects[i]).GetDish(dish);
                    }
                }
            }
            XYPosition xypos = Check();
            if (xypos.x < 0) Console.WriteLine("没东西捡");
            else
            {
                GetItem(xypos);
            }
            status = COMMAND_TYPE.STOP;
        }
        public override void Put(int distance, int ThrowDish)
        {
            if (distance > MaxThrowDistance) distance = MaxThrowDistance;
            XYPosition d_xyPos, aim = Position;
            if (facingDirection == Direction.Down) d_xyPos = new XYPosition(0, -1);
            else if (facingDirection == Direction.Left) d_xyPos = new XYPosition(-1, 0);
            else if (facingDirection == Direction.LeftDown) d_xyPos = new XYPosition(-0.7071, -0.7071);
            else if (facingDirection == Direction.LeftUp) d_xyPos = new XYPosition(-0.7071, 0.7071);
            else if (facingDirection == Direction.Right) d_xyPos = new XYPosition(1, 0);
            else if (facingDirection == Direction.RightDown) d_xyPos = new XYPosition(0.7071, -0.7071);
            else if (facingDirection == Direction.RightUp) d_xyPos = new XYPosition(0.7071, 0.7071);
            else if (facingDirection == Direction.Up) d_xyPos = new XYPosition(0, 1);

            while (distance > 0)
            {
                distance--;

            }

            if ((int)dish != (int)Dish.Type.Empty && ThrowDish != 0)
            {

            }
            else if ((int)tool != (int)Tool.Type.Empty && ThrowDish == 0)
            {

            }
            else Console.WriteLine("没有可以扔的东西");
            status = COMMAND_TYPE.STOP;
        }
        public override void Use(int type, int parameter)
        {
            if (type == 0)//type为0表示使用厨具做菜和提交菜品
            {
                XYPosition xyPosition1 = Position;
                if (facingDirection == Direction.Down) xyPosition1.y--;
                else if (facingDirection == Direction.Left) xyPosition1.x--;
                else if (facingDirection == Direction.LeftDown) { xyPosition1.y--; xyPosition1.x--; }
                else if (facingDirection == Direction.LeftUp) { xyPosition1.y++; xyPosition1.x--; }
                else if (facingDirection == Direction.Right) xyPosition1.x++;
                else if (facingDirection == Direction.RightDown) { xyPosition1.y--; xyPosition1.x++; }
                else if (facingDirection == Direction.RightUp) { xyPosition1.y++; xyPosition1.x++; }
                else if (facingDirection == Direction.Up) xyPosition1.y++;

                if (WorldMap.Grid[(int)xyPosition1.x, (int)xyPosition1.y].blockableObject is Block)
                {
                    if (((Block)WorldMap.Grid[(int)xyPosition1.x, (int)xyPosition1.y].blockableObject).type == Block.Type.Cooker)
                    {
                        ((Block)WorldMap.Grid[(int)xyPosition1.x, (int)xyPosition1.y].blockableObject).UseCooker();
                    }
                    else if (((Block)WorldMap.Grid[(int)xyPosition1.x, (int)xyPosition1.y].blockableObject).type == Block.Type.TaskPoint)
                    {

                    }
                }
            }
            else//否则为使用手中道具
            {
                UseTool(0);
            }
            status = COMMAND_TYPE.STOP;
        }
        public void Stop(object i = null)
        {
            status = COMMAND_TYPE.STOP;
            if (timer != null)
                timer.Dispose();
            Move(0);
        }

        public void Function(Tool.Type type)//在捡起装备时生效，仅对捡起即生效的装备有用
        {
            if (type == Tool.Type.TigerShoes) moveSpeed += Convert.ToDouble(ConfigurationManager.AppSettings["TigerShoeExtraMoveSpeed"]);
            else if (type == Tool.Type.TeleScope) SightRange += Convert.ToInt32(ConfigurationManager.AppSettings["TeleScopeExtraSightRange"]);
        }
        public void DeFunction(Tool.Type type)//在丢弃装备时生效
        {
            if (type == Tool.Type.TigerShoes) moveSpeed -= Convert.ToDouble(ConfigurationManager.AppSettings["TigerShoeExtraMoveSpeed"]);
            else if (type == Tool.Type.TeleScope) SightRange -= Convert.ToInt32(ConfigurationManager.AppSettings["TeleScopeExtraSightRange"]);
        }

        public void GetTalent(TALENT t)
        {
            talent = t;
            if (talent == TALENT.Run) moveSpeed += Convert.ToDouble(ConfigurationManager.AppSettings["RunnerTalentExtraMoveSpeed"]);
            else if (talent == TALENT.Strenth) MaxThrowDistance += Convert.ToInt32(ConfigurationManager.AppSettings["StrenthTalentExtraMoveSpeed"]);
        }

        public void UseTool(int parameter)
        {
            if (tool == Tool.Type.TigerShoes || tool == Tool.Type.TeleScope || tool == Tool.Type.Empty) { Console.WriteLine("物品使用失败（为空或无需使用）！"); }
            else if (tool == Tool.Type.SpeedBuff)
            {
                void Off(object i)
                {
                    moveSpeed -= Convert.ToDouble(ConfigurationManager.AppSettings["SpeedBuffExtraMoveSpeed"]);
                }
                moveSpeed += Convert.ToDouble(ConfigurationManager.AppSettings["SpeedBuffExtraMoveSpeed"]);
                System.Threading.Timer t = new System.Threading.Timer(Off, null,
                    Convert.ToInt32(ConfigurationManager.AppSettings["SpeedBuffDuration"]), 0);
            }
            else if (tool == Tool.Type.StrenthBuff)
            {
                void Off(object i)
                {
                    MaxThrowDistance -= Convert.ToInt32(ConfigurationManager.AppSettings["StrenthBuffExtraThrowDistance"]);
                }
                MaxThrowDistance += Convert.ToInt32(ConfigurationManager.AppSettings["StrenthBuffExtraThrowDistance"]);
                System.Threading.Timer t = new System.Threading.Timer(Off, null,
                    Convert.ToInt32(ConfigurationManager.AppSettings["StrenthBuffDuration"]), 0);
            }
        }
    }
}
