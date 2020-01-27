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
                msgToCl.GameObjectMessageList.Add(this.ID, new GameObjectMessage
                {
                    Type = ObjectTypeMessage.People,
                    Position = new XYPositionMessage { X = this.Position.x, Y = this.Position.y },
                    Direction = (DirectionMessage)(int)this.facingDirection
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
