using System;
using System.Collections.Generic;
using System.Text;
using Logic.Constant;
using static Logic.Constant.Constant;
using THUnity2D;
using static THUnity2D._Map;
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
        public System.Threading.Timer SpeedBuffTimer;
        public System.Threading.Timer StrenthBuffTimer;
        public CommandType status = CommandType.Stop;

        public bool _isStun = false;
        public bool IsStun
        {
            get { return _isStun; }
            set
            {
                _isStun = value;
                if (value)
                {
                    StunTimer.Change(Convert.ToInt32(ConfigurationManager.AppSettings["TrapStunDuration"]), 0);
                }
            }
        }
        public System.Threading.Timer StunTimer;

        public Player(double x, double y) :
            base(x, y)
        {
            Parent = WorldMap;
            MoveStopTimer = new System.Threading.Timer((i)=> { Velocity = new Vector(0, 0);status = CommandType.Stop; }, new object(), -1, -1);
            StunTimer = new System.Threading.Timer((i) => { _isStun = false; }, null, -1, -1);
            SpeedBuffTimer= new System.Threading.Timer((i) => { moveSpeed -= Convert.ToDouble(ConfigurationManager.AppSettings["SpeedBuffExtraMoveSpeed"]); }, null, -1, -1);
            StrenthBuffTimer= new System.Threading.Timer((i) => { MaxThrowDistance -= Convert.ToInt32(ConfigurationManager.AppSettings["StrenthBuffExtraThrowDistance"]); }, null, -1, -1);
           
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
            if (IsStun) return;
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
                    Put(1, 0);
                    break;
                case CommandTypeMessage.Use:
                    Use(1, 0);
                    break;
                default:
                    break;
            }
        }

        public void Move(Direction direction)
        {
            this.facingDirection = direction;
            Move(new MoveEventArgs((int)direction * Math.PI / 4, (moveSpeed + GlueExtraMoveSpeed) / Constant.Constant.FrameRate));
        }

        public override void Move(Direction direction, int durationMilliseconds)
        {
            this.facingDirection = direction;
            this.Velocity = new Vector(0, 0);
            this.Velocity = new Vector(((double)(int)direction) * Math.PI / 4, moveSpeed + GlueExtraMoveSpeed);
            this.status = CommandType.Move;

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

                    foreach (var item in WorldMap.Grid[(int)xypos.x, (int)xypos.y].GetLayer((int)MapLayer.ItemLayer))
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
                Tool toolToThrow = new Tool(Position.x, Position.y, tool);
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
                moveSpeed += Convert.ToDouble(ConfigurationManager.AppSettings["SpeedBuffExtraMoveSpeed"]);
                SpeedBuffTimer.Change(Convert.ToInt32(ConfigurationManager.AppSettings["SpeedBuffDuration"]), 0);
                this.Velocity = new Vector(Velocity.angle, moveSpeed + GlueExtraMoveSpeed);
                tool = ToolType.Empty;
            }
            else if (tool == ToolType.StrenthBuff)
            {
                MaxThrowDistance += Convert.ToInt32(ConfigurationManager.AppSettings["StrenthBuffExtraThrowDistance"]);
                StrenthBuffTimer.Change(Convert.ToInt32(ConfigurationManager.AppSettings["StrenthBuffDuration"]), 0);
                tool = ToolType.Empty;
            }
            else if (tool == ToolType.Fertilizer)
            {
                XYPosition xyPosition1 = Position.GetMid() + 2 * EightCornerVector[facingDirection];
                if (WorldMap.Grid[(int)xyPosition1.x, (int)xyPosition1.y].ContainsType(typeof(Block)))
                    foreach (Block block in WorldMap.Grid[(int)xyPosition1.x, (int)xyPosition1.y].GetType(typeof(Block)))
                    {
                        if (block.blockType == BlockType.FoodPoint)
                            block.RefreshTime /= 2;
                        else
                        { Console.WriteLine("物品使用失败（未检测到施肥对象）！"); }
                    }
                tool = ToolType.Empty;
            }
            else if (tool == ToolType.WaveGlue)
            {
                XYPosition xyPosition1 = Position.GetMid() + 2 * EightCornerVector[facingDirection];
                new Trigger(xyPosition1.x, xyPosition1.y, TriggerType.WaveGlue, team).Parent = WorldMap;
                tool = ToolType.Empty;
            }
            else if (tool == ToolType.LandMine)
            {
                XYPosition xyPosition1 = Position.GetMid() + 2 * EightCornerVector[facingDirection];
                new Trigger(xyPosition1.x, xyPosition1.y, TriggerType.Mine, team).Parent = WorldMap;
                tool = ToolType.Empty;
            }
            else if (tool == ToolType.Trap)
            {
                XYPosition xyPosition1 = Position.GetMid() + 2 * EightCornerVector[facingDirection];
                new Trigger(xyPosition1.x, xyPosition1.y, TriggerType.Trap, -1).Parent = WorldMap;
                tool = ToolType.Empty;
            }
        }
        public override bool TouchTrigger(GameObject gameObject)
        {
            if (((Trigger)gameObject).triggerType == TriggerType.WaveGlue && ((Trigger)gameObject).OwnerTeam != team)
            {
                GlueExtraMoveSpeed = Convert.ToDouble(ConfigurationManager.AppSettings["WaveGlueExtraMoveSpeed"]);
                Console.WriteLine(GlueExtraMoveSpeed);
                this.Velocity = new Vector(Velocity.angle, moveSpeed + GlueExtraMoveSpeed);
            }
            else if (((Trigger)gameObject).triggerType == TriggerType.Mine && ((Trigger)gameObject).OwnerTeam != team)
            {
                score += Convert.ToInt32(ConfigurationManager.AppSettings["MineScore"]);
                return true;
            }
            else if (((Trigger)gameObject).triggerType == TriggerType.Trap && ((Trigger)gameObject).OwnerTeam != team)
            {
                IsStun = true;
                Velocity = new Vector(0, 0);
                return true;
            }
            return false;
        }
    }
}
