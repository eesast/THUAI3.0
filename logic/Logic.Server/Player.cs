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
using System.IO;

namespace Logic.Server
{
    class Player : Character
    {
        public System.Threading.Timer MoveStopTimer;
        public CommandType status = CommandType.Stop;

        public Player(double x, double y) :
            base(x, y)
        {
            Parent = WorldMap;
            MoveStopTimer = new System.Threading.Timer(Stop, new object(), -1, -1);
            void Stop(object i)
            {
                Velocity = new Vector(0, 0);
                status = CommandType.Stop;
            }
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
            this.MoveComplete += new MoveCompleteHandler(
                (thisGameObject) =>
                {
                    lock (Program.MessageToClientLock)
                    {
                        Program.MessageToClient.GameObjectMessageList[thisGameObject.ID].Position.X = thisGameObject.Position.x;
                        Program.MessageToClient.GameObjectMessageList[thisGameObject.ID].Position.Y = thisGameObject.Position.y;
                        Program.MessageToClient.GameObjectMessageList[thisGameObject.ID].Direction = (DirectionMessage)((Player)thisGameObject).facingDirection;
                    }
                });
        }
        public void ExecuteMessage(CommunicationImpl communication, MessageToServer msg)
        {
            if (msg.CommandType < 0 || msg.CommandType >= CommandTypeMessage.CommandTypeSize)
                return;
            switch (msg.CommandType)
            {
                case CommandTypeMessage.Move:
                    if (msg.MoveDirection >= 0 && msg.MoveDirection < DirectionMessage.DirectionSize)
                        Move((Direction)msg.MoveDirection, 1000);
                    break;
                case CommandTypeMessage.Pick:
                    Pick();
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
        }

        public override void Move(Direction direction, int durationMilliseconds)
        {
            this.facingDirection = direction;
            this.Velocity = new Vector(0, 0);
            this.Velocity = new Vector(((double)(int)direction) * Math.PI / 4, moveSpeed);
            this.status = CommandType.Move;

            //FileStream fs = new FileStream("log.txt", FileMode.Append);// 初始化文件流 
            //byte[] array = Encoding.UTF8.GetBytes(TimeSpan.FromMilliseconds(durationMilliseconds).ToString() + " ");//给字节数组赋值 
            //fs.Write(array, 0, array.Length);//将字节数组写入文件流 
            //fs.Close();

            MoveStopTimer.Change(durationMilliseconds, 0);
        }
        public override void Pick()
        {
            XYPosition Check()
            {
                bool CheckItem(XYPosition xypos)
                {
                    if (WorldMap.Grid[(int)xypos.x, (int)xypos.y].ContainsType(typeof(Block)))
                        foreach (var gameObject in WorldMap.Grid[(int)xypos.x, (int)xypos.y].GetType(typeof(Block)))
                        {
                            if (((Block)gameObject).dish != DishType.Empty)
                                return true;
                        }

                    foreach (var item in WorldMap.Grid[(int)xypos.x, (int)xypos.y].GetLayer((int)Constant.Map.MapLayer.ItemLayer))
                    {
                        if (item is Dish || item is Tool)
                            return true;
                    }
                    //等地图做完写
                    return false;
                }
                XYPosition xyPosition1 = Position + 2 * THUnity2D.Tools.EightCornerVector[facingDirection];

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
                if (WorldMap.Grid[(int)xypos.x, (int)xypos.y].ContainsType(typeof(Block)))
                {
                    foreach (Block block in WorldMap.Grid[(int)xypos.x, (int)xypos.y].GetType(typeof(Block)))
                    {
                        if ((block.blockType == BlockType.FoodPoint || block.blockType == BlockType.Cooker)
                            && block.dish != DishType.Empty)
                        {
                            dish = block.GetDish(dish);
                            return;
                        }
                        break;
                    }
                }

                foreach (var item in WorldMap.Grid[(int)xypos.x, (int)xypos.y].GetLayer((int)MapLayer.ItemLayer))
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
            int dueTime = distance / 5;
            //XYPosition aim = Position;
            //XYPosition d_xyPos = THUnity2D.Tools.EightUnitVector[facingDirection];

            if ((int)dish != (int)DishType.Empty && ThrowDish != 0)
            {
                Dish dishToThrow = new Dish(Position.x, Position.y, dish);
                dishToThrow.Parent = WorldMap;
                dishToThrow.Layer = (int)MapLayer.FlyingLayer;
                dishToThrow.Velocity = new Vector((double)(int)facingDirection * Math.PI / 4, 5);
                dishToThrow.StopMovingTimer.Change(TimeSpan.FromSeconds(dueTime), TimeSpan.FromSeconds(-1));

            }
            else if ((int)tool != (int)ToolType.Empty && ThrowDish == 0)
            {
                Dish toolToThrow = new Dish(Position.x, Position.y, dish);
                toolToThrow.Parent = WorldMap;
                toolToThrow.Layer = (int)MapLayer.FlyingLayer;
                toolToThrow.Velocity = new Vector((double)(int)facingDirection * Math.PI / 4, 5);
                toolToThrow.StopMovingTimer.Change(TimeSpan.FromSeconds(dueTime), TimeSpan.FromSeconds(-1));
            }
            else Console.WriteLine("没有可以扔的东西");
            status = CommandType.Stop;
        }
        public override void Use(int type, int parameter)
        {
            if (type == 0)//type为0表示使用厨具做菜和提交菜品
            {
                XYPosition xyPosition1 = Position + 2 * THUnity2D.Tools.EightCornerVector[facingDirection];
                if (WorldMap.Grid[(int)xyPosition1.x, (int)xyPosition1.y].ContainsType(typeof(Block)))
                {
                    foreach (Block block in WorldMap.Grid[(int)xyPosition1.x, (int)xyPosition1.y].GetType(typeof(Block)))
                    {
                        if ((int)block.type == (int)BlockType.Cooker)
                        {
                            block.UseCooker();
                        }
                        else if (block.blockType == BlockType.TaskPoint)
                        {
                            int temp = block.HandIn(dish);
                            if (temp > 0)
                            { score += temp; dish = DishType.Empty; }
                        }
                        break;
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
            if (tool == ToolType.TigerShoes || tool == ToolType.TeleScope || tool == ToolType.BreastPlate || tool == ToolType.Condiment || tool == ToolType.Empty)
            { Console.WriteLine("物品使用失败（为空或无需使用）！"); }
            else if (tool == ToolType.SpeedBuff)
            {
                void Off(object i)
                {
                    moveSpeed -= Convert.ToDouble(ConfigurationManager.AppSettings["SpeedBuffExtraMoveSpeed"]);
                }
                moveSpeed += Convert.ToDouble(ConfigurationManager.AppSettings["SpeedBuffExtraMoveSpeed"]);
                System.Threading.Timer t = new System.Threading.Timer(Off, null,
                    Convert.ToInt32(ConfigurationManager.AppSettings["SpeedBuffDuration"]), 0);
                tool = ToolType.Empty;
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
                tool = ToolType.Empty;
            }
            else if (tool == ToolType.Fertilizer)
            {
                XYPosition xyPosition1 = Position.GetMid() + 2 * THUnity2D.Tools.EightCornerVector[facingDirection];
                if (WorldMap.Grid[(int)xyPosition1.x, (int)xyPosition1.y].ContainsType(typeof(Block)))
                    foreach (Block block in WorldMap.Grid[(int)xyPosition1.x, (int)xyPosition1.y].GetType(typeof(Block)))
                    {
                        if (block.blockType == BlockType.FoodPoint)
                            block.RefreshTime /= 2;
                        else
                        { Console.WriteLine("物品使用失败（未检测到施肥对象）！"); }
                    }
            }
            else if (tool == ToolType.WaveGlue)
            {
                XYPosition xyPosition1 = Position.GetMid() + 2 * THUnity2D.Tools.EightCornerVector[facingDirection];
                Trigger t = new Trigger(xyPosition1.x, xyPosition1.y, TriggerType.WaveGlue);
                t.Parent = WorldMap;
                new System.Threading.Timer
                    ((i) => { t.Parent = null; },
                    null, Convert.ToInt32(ConfigurationManager.AppSettings["WaveGlueDuration"]), 0);
            }
        }
    }
}
