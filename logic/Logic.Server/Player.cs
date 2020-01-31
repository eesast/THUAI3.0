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
        public System.Threading.Timer MoveStopTimer = new System.Threading.Timer((o) => { });
        public CommandType status;

        public Player(double x, double y) :
            base(x, y)
        {
            Parent = WorldMap;
            status = CommandType.Stop;
            lock (Program.MessageToClientLock)
            {
                Program.MessageToClient.GameObjectMessageList.Add(
                    this.ID,
                    new GameObjectMessage
                    {
                        ObjType = ObjTypeMessage.People,
                        IsMoving = false,
                        Position = new XYPositionMessage { X = this.Position.x, Y = this.Position.y },
                        Direction = (DirectionMessage)(int)this.facingDirection
                    });
            }
        }
        public void ExecuteMessage(CommunicationImpl communication, MessageToServer msg)
        {
            if (msg.CommandType < 0 || msg.CommandType >= CommandTypeMessage.CommandTypeSize)
                return;
            switch (msg.CommandType)
            {
                case CommandTypeMessage.Move:
                    if (msg.MoveDirection >= 0 && msg.MoveDirection < DirectionMessage.DirectionSize)
                        Move((Direction)msg.MoveDirection);
                    break;
                case CommandTypeMessage.Pick:
                    break;
                case CommandTypeMessage.Put:
                    break;
                case CommandTypeMessage.Stop:
                    break;
                case CommandTypeMessage.Use:
                    break;
                default:
                    break;
            }
        }

        public void Move(Direction direction)
        {
            this.facingDirection = direction;
            Move(new MoveEventArgs((int)direction * Math.PI / 4, moveSpeed / Constant.Constant.FrameRate));
            lock (Program.MessageToClientLock)
            {
                Program.MessageToClient.GameObjectMessageList[this.ID].Position.X = this.Position.x;
                Program.MessageToClient.GameObjectMessageList[this.ID].Position.Y = this.Position.y;
                Program.MessageToClient.GameObjectMessageList[this.ID].Direction = (DirectionMessage)this.facingDirection;
            }
        }
        public void Move(double angle, int durationMilliseconds)
        {
            this.Velocity = new Vector(angle, moveSpeed);
            this.status = CommandType.Move;
            new System.Threading.Timer(
                (o) =>
                {
                    this.Velocity = new Vector(angle, 0);
                    this.status = CommandType.Stop;
                }, new object(), TimeSpan.FromMilliseconds(durationMilliseconds), TimeSpan.FromMilliseconds(-1));
        }
        public override void Pick()
        {
            XYPosition Check()
            {
                bool CheckItem(XYPosition xypos)
                {
                    if (WorldMap.Grid[(int)xypos.x, (int)xypos.y].BlockableObject is Block && ((Block)WorldMap.Grid[(int)xypos.x, (int)xypos.y].BlockableObject).dish != DishType.Empty)
                        return true;
                    foreach (var item in WorldMap.Grid[(int)xypos.x, (int)xypos.y].unblockableObjects)
                    {
                        if (item is Dish || item is Tool)
                            return true;
                    }
                    //等地图做完写
                    return false;
                }
                XYPosition xyPosition1 = Position;
                switch (facingDirection)
                {
                    case Direction.Down: xyPosition1 = new XYPosition(xyPosition1.x, xyPosition1.y - 1); break;
                    case Direction.Left: xyPosition1 = new XYPosition(xyPosition1.x - 1, xyPosition1.y); break;
                    case Direction.LeftDown: xyPosition1 = new XYPosition(xyPosition1.x - 1, xyPosition1.y - 1); break;
                    case Direction.LeftUp: xyPosition1 = new XYPosition(xyPosition1.x - 1, xyPosition1.y + 1); break;
                    case Direction.Right: xyPosition1 = new XYPosition(xyPosition1.x + 1, xyPosition1.y); break;
                    case Direction.RightDown: xyPosition1 = new XYPosition(xyPosition1.x + 1, xyPosition1.y - 1); break;
                    case Direction.RightUp: xyPosition1 = new XYPosition(xyPosition1.x + 1, xyPosition1.y + 1); break;
                    case Direction.Up: xyPosition1 = new XYPosition(xyPosition1.x, xyPosition1.y + 1); break;
                }

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
                if (WorldMap.Grid[(int)xypos.x, (int)xypos.y].BlockableObject is Block
                    && ((Block)WorldMap.Grid[(int)xypos.x, (int)xypos.y].BlockableObject).blockType == BlockType.FoodPoint
                    && ((Block)WorldMap.Grid[(int)xypos.x, (int)xypos.y].BlockableObject).dish != DishType.Empty)
                {
                    dish = ((Block)WorldMap.Grid[(int)xypos.x, (int)xypos.y].BlockableObject).GetDish(dish);
                }

                foreach (var item in WorldMap.Grid[(int)xypos.x, (int)xypos.y].unblockableObjects)
                {
                    if (item is Dish)
                    {
                        dish = ((Dish)item).GetDish(dish);
                    }
                    else if (item is Tool)
                    {
                        Console.Write("GetTool!");
                        DeFunction(tool);
                        tool = ((Tool)item).GetTool(tool);
                        Function(tool);
                    }
                }
            }
            XYPosition xypos = Check();
            if (xypos.x < 0) Console.WriteLine("没东西捡");
            else
            {
                GetItem(xypos);
            }
            status = CommandType.Stop;
        }
        public override void Put(int distance, int ThrowDish)
        {
            if (distance > MaxThrowDistance) distance = MaxThrowDistance;
            XYPosition d_xyPos, aim = Position;
            switch (facingDirection)
            {
                case Direction.Down: d_xyPos = new XYPosition(0, -1); break;
                case Direction.Left: d_xyPos = new XYPosition(-1, 0); break;
                case Direction.LeftDown: d_xyPos = new XYPosition(-0.7071, -0.7071); break;
                case Direction.LeftUp: d_xyPos = new XYPosition(-0.7071, 0.7071); break;
                case Direction.Right: d_xyPos = new XYPosition(1, 0); break;
                case Direction.RightDown: d_xyPos = new XYPosition(0.7071, -0.7071); break;
                case Direction.RightUp: d_xyPos = new XYPosition(0.7071, 0.7071); break;
                case Direction.Up: d_xyPos = new XYPosition(0, 1); break;
            }

            while (distance > 0)
            {
                distance--;

            }

            if ((int)dish != (int)DishType.Empty && ThrowDish != 0)
            {

            }
            else if ((int)tool != (int)ToolType.Empty && ThrowDish == 0)
            {

            }
            else Console.WriteLine("没有可以扔的东西");
            status = CommandType.Stop;
        }
        public override void Use(int type, int parameter)
        {
            if (type == 0)//type为0表示使用厨具做菜和提交菜品
            {
                XYPosition xyPosition1 = Position;
                switch (facingDirection)
                {
                    case Direction.Down: xyPosition1 = new XYPosition(xyPosition1.x, xyPosition1.y - 1); break;
                    case Direction.Left: xyPosition1 = new XYPosition(xyPosition1.x - 1, xyPosition1.y); break;
                    case Direction.LeftDown: xyPosition1 = new XYPosition(xyPosition1.x - 1, xyPosition1.y - 1); break;
                    case Direction.LeftUp: xyPosition1 = new XYPosition(xyPosition1.x - 1, xyPosition1.y + 1); break;
                    case Direction.Right: xyPosition1 = new XYPosition(xyPosition1.x + 1, xyPosition1.y); break;
                    case Direction.RightDown: xyPosition1 = new XYPosition(xyPosition1.x + 1, xyPosition1.y - 1); break;
                    case Direction.RightUp: xyPosition1 = new XYPosition(xyPosition1.x + 1, xyPosition1.y + 1); break;
                    case Direction.Up: xyPosition1 = new XYPosition(xyPosition1.x, xyPosition1.y + 1); break;
                }
                if (WorldMap.Grid[(int)xyPosition1.x, (int)xyPosition1.y].BlockableObject is Block)
                {
                    if ((int)((Block)WorldMap.Grid[(int)xyPosition1.x, (int)xyPosition1.y].BlockableObject).type == (int)BlockType.Cooker)
                    {
                        ((Block)WorldMap.Grid[(int)xyPosition1.x, (int)xyPosition1.y].BlockableObject).UseCooker();
                    }
                    else if (((Block)WorldMap.Grid[(int)xyPosition1.x, (int)xyPosition1.y].BlockableObject).blockType == BlockType.TaskPoint)
                    {
                        int temp = ((Block)WorldMap.Grid[(int)xyPosition1.x, (int)xyPosition1.y].BlockableObject).HandIn(dish);
                        if (temp > 0)
                        { score += temp; dish = DishType.Empty; }
                    }
                }
            }
            else//否则为使用手中道具
            {
                UseTool(0);
            }
            status = CommandType.Stop;
        }

        public void Function(ToolType type)//在捡起装备时生效，仅对捡起即生效的装备有用
        {
            if (type == ToolType.TigerShoes) moveSpeed += Convert.ToDouble(ConfigurationManager.AppSettings["TigerShoeExtraMoveSpeed"]);
            else if (type == ToolType.TeleScope) SightRange += Convert.ToInt32(ConfigurationManager.AppSettings["TeleScopeExtraSightRange"]);
        }
        public void DeFunction(ToolType type)//在丢弃装备时生效
        {
            if (type == ToolType.TigerShoes) moveSpeed -= Convert.ToDouble(ConfigurationManager.AppSettings["TigerShoeExtraMoveSpeed"]);
            else if (type == ToolType.TeleScope) SightRange -= Convert.ToInt32(ConfigurationManager.AppSettings["TeleScopeExtraSightRange"]);
        }

        public void GetTalent(TALENT t)
        {
            talent = t;
            if (talent == TALENT.Run) moveSpeed += Convert.ToDouble(ConfigurationManager.AppSettings["RunnerTalentExtraMoveSpeed"]);
            else if (talent == TALENT.Strenth) MaxThrowDistance += Convert.ToInt32(ConfigurationManager.AppSettings["StrenthTalentExtraMoveSpeed"]);
        }

        public void UseTool(int parameter)
        {
            if (tool == ToolType.TigerShoes || tool == ToolType.TeleScope || tool == ToolType.Empty) { Console.WriteLine("物品使用失败（为空或无需使用）！"); }
            else if (tool == ToolType.SpeedBuff)
            {
                void Off(object i)
                {
                    moveSpeed -= Convert.ToDouble(ConfigurationManager.AppSettings["SpeedBuffExtraMoveSpeed"]);
                }
                moveSpeed += Convert.ToDouble(ConfigurationManager.AppSettings["SpeedBuffExtraMoveSpeed"]);
                System.Threading.Timer t = new System.Threading.Timer(Off, null,
                    Convert.ToInt32(ConfigurationManager.AppSettings["SpeedBuffDuration"]), 0);
            }
            else if (tool == ToolType.StrenthBuff)
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
