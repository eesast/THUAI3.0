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
        //public System.Threading.Timer timer;
        public System.Threading.Timer MoveStopTimer;
        public CommandType status;
        //public TimeSpan LastActTime;
        //public TimeSpan LeftTime;

        public Player(double x, double y) :
            base(x, y)
        {
            Parent = WorldMap;
            status = CommandType.Stop;
            //LastActTime = Time.GameTime();
            //timer = new System.Threading.Timer(new System.Threading.TimerCallback(Move), (object)1, -1, 0);
            //StopTimer = new System.Threading.Timer(new System.Threading.TimerCallback(Stop), (object)1, 0, 0);
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
            Move(new MoveEventArgs((int)direction * Math.PI / 4, moveSpeed / Constant.Constant.FrameRate));
            Program.MessageToClient.GameObjectMessageList[this.ID].Position.X = this.Position.x;
            Program.MessageToClient.GameObjectMessageList[this.ID].Position.Y = this.Position.y;
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
                    if (WorldMap.Grid[(int)xypos.x, (int)xypos.y].blockableObject is Block && ((Block)WorldMap.Grid[(int)xypos.x, (int)xypos.y].blockableObject).dish != Dish.Type.Empty)
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
                if (facingDirection == Direction.Down) xyPosition1 = new XYPosition(xyPosition1.x, xyPosition1.y - 1);
                else if (facingDirection == Direction.Left) xyPosition1 = new XYPosition(xyPosition1.x - 1, xyPosition1.y);
                else if (facingDirection == Direction.LeftDown) xyPosition1 = new XYPosition(xyPosition1.x - 1, xyPosition1.y - 1);
                else if (facingDirection == Direction.LeftUp) xyPosition1 = new XYPosition(xyPosition1.x - 1, xyPosition1.y + 1);
                else if (facingDirection == Direction.Right) xyPosition1 = new XYPosition(xyPosition1.x + 1, xyPosition1.y);
                else if (facingDirection == Direction.RightDown) xyPosition1 = new XYPosition(xyPosition1.x + 1, xyPosition1.y - 1);
                else if (facingDirection == Direction.RightUp) xyPosition1 = new XYPosition(xyPosition1.x + 1, xyPosition1.y + 1);
                else if (facingDirection == Direction.Up) xyPosition1 = new XYPosition(xyPosition1.x, xyPosition1.y + 1);

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
                if (WorldMap.Grid[(int)xypos.x, (int)xypos.y].blockableObject is Block
                    && ((Block)WorldMap.Grid[(int)xypos.x, (int)xypos.y].blockableObject).blockType == Block.Type.FoodPoint
                    && ((Block)WorldMap.Grid[(int)xypos.x, (int)xypos.y].blockableObject).dish != Dish.Type.Empty)
                {
                    dish = ((Block)WorldMap.Grid[(int)xypos.x, (int)xypos.y].blockableObject).GetDish(dish);
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
            status = CommandType.Stop;
        }
        public override void Use(int type, int parameter)
        {
            if (type == 0)//type为0表示使用厨具做菜和提交菜品
            {
                XYPosition xyPosition1 = Position;
                if (facingDirection == Direction.Down) xyPosition1 = new XYPosition(xyPosition1.x, xyPosition1.y - 1);
                else if (facingDirection == Direction.Left) xyPosition1 = new XYPosition(xyPosition1.x - 1, xyPosition1.y);
                else if (facingDirection == Direction.LeftDown) xyPosition1 = new XYPosition(xyPosition1.x - 1, xyPosition1.y - 1);
                else if (facingDirection == Direction.LeftUp) xyPosition1 = new XYPosition(xyPosition1.x - 1, xyPosition1.y + 1);
                else if (facingDirection == Direction.Right) xyPosition1 = new XYPosition(xyPosition1.x + 1, xyPosition1.y);
                else if (facingDirection == Direction.RightDown) xyPosition1 = new XYPosition(xyPosition1.x + 1, xyPosition1.y - 1);
                else if (facingDirection == Direction.RightUp) xyPosition1 = new XYPosition(xyPosition1.x + 1, xyPosition1.y + 1);
                else if (facingDirection == Direction.Up) xyPosition1 = new XYPosition(xyPosition1.x, xyPosition1.y + 1);

                if (WorldMap.Grid[(int)xyPosition1.x, (int)xyPosition1.y].blockableObject is Block)
                {
                    if ((int)((Block)WorldMap.Grid[(int)xyPosition1.x, (int)xyPosition1.y].blockableObject).type == (int)Block.Type.Cooker)
                    {
                        ((Block)WorldMap.Grid[(int)xyPosition1.x, (int)xyPosition1.y].blockableObject).UseCooker();
                    }
                    else if (((Block)WorldMap.Grid[(int)xyPosition1.x, (int)xyPosition1.y].blockableObject).blockType == Block.Type.TaskPoint)
                    {
                        int temp = ((Block)WorldMap.Grid[(int)xyPosition1.x, (int)xyPosition1.y].blockableObject).HandIn(dish);
                        if (temp > 0)
                        { score += temp; dish = Dish.Type.Empty; }
                    }
                }
            }
            else//否则为使用手中道具
            {
                UseTool(0);
            }
            status = CommandType.Stop;
        }
        //public void Stop(object i = null)
        //{
        //    status = CommandType.Stop;
        //    if (timer != null)
        //        timer.Dispose();
        //    Move(0);
        //}

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
