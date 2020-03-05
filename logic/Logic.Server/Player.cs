using Communication.Proto;
using Communication.Server;
using Logic.Constant;
using System;
using THUnity2D;
using static Logic.Constant.Constant;
using static Logic.Constant.MapInfo;
using static THUnity2D.Tools;

namespace Logic.Server
{
    class Player : Character
    {
        protected System.Threading.Timer MoveStopTimer;
        protected System.Threading.Timer SpeedBuffTimer;
        protected System.Threading.Timer StrengthBuffTimer;
        public CommandType status = CommandType.Stop;
        protected bool isStepOnGlue = false;

        protected bool _isStun = false;
        protected bool IsStun
        {
            get { return _isStun; }
            set
            {
                _isStun = value;
                if (value)
                {
                    StunTimer.Change((int)Configs["TrapStunDuration"], 0);
                }
            }
        }
        protected System.Threading.Timer StunTimer;

        protected DishType Dish
        {
            get { return dish; }
            set
            {
                dish = value;
                lock (Program.MessageToClientLock)
                    Program.MessageToClient.GameObjectMessageList[this.ID].DishType = (DishTypeMessage)dish;
            }
        }

        protected ToolType Tool
        {
            get { return tool; }
            set
            {
                tool = value;
                lock (Program.MessageToClientLock)
                    Program.MessageToClient.GameObjectMessageList[this.ID].ToolType = (ToolTypeMessage)tool;
            }
        }

        protected int Score
        {
            get { return score; }
            set
            {
                score = value;
                lock (Program.MessageToClientLock)
                    Program.MessageToClient.GameObjectMessageList[this.ID].Score = score;
            }
        }

        public Player(double x, double y) :
            base(x, y)
        {
            Parent = WorldMap;
            MoveStopTimer = new System.Threading.Timer((i) => { Velocity = new Vector(Velocity.angle, 0); status = CommandType.Stop; Program.MessageToClient.GameObjectMessageList[this.ID].IsMoving = false; });
            StunTimer = new System.Threading.Timer((i) => { _isStun = false; });
            SpeedBuffTimer = new System.Threading.Timer((i) => { moveSpeed -= (double)Configs["SpeedBuffExtraMoveSpeed"]; });
            StrengthBuffTimer = new System.Threading.Timer((i) => { MaxThrowDistance -= (int)Configs["StrenthBuffExtraThrowDistance"]; });

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
            this.MoveStart += new MoveStartHandler(
                (thisGameObject) =>
                {
                    if (isStepOnGlue)
                    {
                        GlueExtraMoveSpeed = (int)Configs["WaveGlueExtraMoveSpeed"];
                        if (Velocity.length > 3)
                            this.Velocity = new Vector(Velocity.angle, moveSpeed + GlueExtraMoveSpeed);
                    }
                    else
                    {
                        GlueExtraMoveSpeed = 0;
                        if (Velocity.length > 0 && Velocity.length < 4)
                            this.Velocity = new Vector(Velocity.angle, moveSpeed);
                    }

                    isStepOnGlue = false;
                });
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
            this.OnTrigger += new TriggerHandler(
                (triggerGameObjects) =>
                {
                    foreach (Trigger trigger in triggerGameObjects)
                        this.TouchTrigger(trigger);
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
                    Move((Direction)msg.MoveDirection, msg.MoveDuration);
                    break;
                case CommandTypeMessage.Pick:
                    Pick();
                    break;
                case CommandTypeMessage.Put:
                    Put(msg.ThrowDistance, msg.IsThrowDish);
                    break;
                case CommandTypeMessage.Use:
                    Use(msg.UseType, 0);
                    break;
                case CommandTypeMessage.Speak:
                    SpeakToFriend(msg.SpeakText);
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
            this.Velocity = new Vector(((double)(int)direction) * Math.PI / 4, moveSpeed + GlueExtraMoveSpeed);
            this.status = CommandType.Move;
            lock (Program.MessageToClientLock)
                Program.MessageToClient.GameObjectMessageList[this.ID].IsMoving = true;
            MoveStopTimer.Change(durationMilliseconds, 0);
        }
        public override void Pick()
        {
            XYPosition[] toCheckPositions = new XYPosition[] { Position, Position + 2 * EightCornerVector[facingDirection] };
            foreach (var xypos in toCheckPositions)
            {
                if (WorldMap.Grid[(int)xypos.x, (int)xypos.y].ContainsType(typeof(Block)))
                {
                    foreach (Block block in WorldMap.Grid[(int)xypos.x, (int)xypos.y].GetType(typeof(Block)))
                    {
                        if ((block.blockType == BlockType.FoodPoint || block.blockType == BlockType.Cooker)
                            && block.Dish != DishType.Empty)
                        {
                            Dish = block.GetDish(Dish);
                            Server.ServerDebug("Player : " + ID + " Get Dish " + Dish.ToString());
                            return;
                        }
                        break;
                    }
                }

                foreach (var item in WorldMap.Grid[(int)xypos.x, (int)xypos.y].GetLayer((int)MapLayer.ItemLayer))
                {
                    if (item is Dish)
                    {
                        Dish = ((Dish)item).GetDish(Dish);
                        Server.ServerDebug("Player : " + ID + " Get Dish " + Dish.ToString());
                        return;
                    }
                    else if (item is Tool)
                    {
                        DeFunction(tool);
                        Tool = ((Tool)item).GetTool(tool);
                        Server.ServerDebug("Player : " + ID + " Get Tool " + tool.ToString());
                        Function(tool);
                        return;
                    }
                }
            }
            Console.WriteLine("没东西捡");
            status = CommandType.Stop;
            Velocity = new Vector(0, 0);
        }
        public override void Put(double distance, bool isThrowDish)
        {
            if (distance > MaxThrowDistance) distance = MaxThrowDistance;
            int dueTime = (int)(1000 * distance / (int)Configs["ItemMoveSpeed"]);

            if (Dish != DishType.Empty && isThrowDish)
            {
                Dish dishToThrow = new Dish(Position.x, Position.y, Dish);
                dishToThrow.Layer = (int)MapLayer.FlyingLayer;
                dishToThrow.Parent = WorldMap;
                dishToThrow.Velocity = new Vector((double)(int)facingDirection * Math.PI / 4, (int)Configs["ItemMoveSpeed"]);
                dishToThrow.StopMovingTimer.Change(dueTime, 0);
                Dish = DishType.Empty;
            }
            else if (tool != ToolType.Empty && !isThrowDish)
            {
                Tool toolToThrow = new Tool(Position.x, Position.y, tool);
                toolToThrow.Layer = (int)MapLayer.FlyingLayer;
                toolToThrow.Parent = WorldMap;
                toolToThrow.Velocity = new Vector((double)(int)facingDirection * Math.PI / 4, (int)Configs["ItemMoveSpeed"]);
                toolToThrow.StopMovingTimer.Change(dueTime, 0);
                Tool = ToolType.Empty;
            }
            else Console.WriteLine("没有可以扔的东西");
            status = CommandType.Stop;
            Velocity = new Vector(0, 0);
        }
        public override void Use(int type, int parameter)
        {
            if (type == 0)//type为0表示使用厨具做菜和提交菜品
            {
                XYPosition xyPosition1 = Position + 2 * EightCornerVector[facingDirection];
                if (WorldMap.Grid[(int)xyPosition1.x, (int)xyPosition1.y].ContainsType(typeof(Block)))
                {
                    foreach (Block block in WorldMap.Grid[(int)xyPosition1.x, (int)xyPosition1.y].GetType(typeof(Block)))
                    {
                        if (block.blockType == BlockType.Cooker)
                        {
                            if (block.Cooking == false) block.UseCooker();
                        }
                        else if (block.blockType == BlockType.TaskPoint)
                        {
                            int temp = block.HandIn(Dish);
                            { Score += temp; Dish = DishType.Empty; }
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
            Velocity = new Vector(0, 0);
        }

        public void Function(ToolType type)//在捡起装备时生效，仅对捡起即生效的装备有用
        {
            if (type == ToolType.TigerShoes) moveSpeed += (double)(Configs["TigerShoeExtraMoveSpeed"]);
            else if (type == ToolType.TeleScope) SightRange += (int)(Configs["TeleScopeExtraSightRange"]);
        }
        public void DeFunction(ToolType type)//在丢弃装备时生效
        {
            if (type == ToolType.TigerShoes) moveSpeed -= (double)(Configs["TigerShoeExtraMoveSpeed"]);
            else if (type == ToolType.TeleScope) SightRange -= (int)(Configs["TeleScopeExtraSightRange"]);
        }

        public void GetTalent(TALENT t)
        {
            talent = t;
            if (talent == TALENT.Run) moveSpeed += (double)(Configs["RunnerTalentExtraMoveSpeed"]);
            else if (talent == TALENT.Strenth) MaxThrowDistance += (int)(Configs["StrenthTalentExtraMoveSpeed"]);
        }

        public void UseTool(int parameter)
        {
            switch (tool)
            {
                case ToolType.TigerShoes:
                case ToolType.TeleScope:
                case ToolType.BreastPlate:
                case ToolType.Condiment:
                case ToolType.Empty: Console.WriteLine("物品使用失败（为空或无需使用）！"); break;
                case ToolType.SpeedBuff:
                    {
                        moveSpeed += (double)(Configs["SpeedBuffExtraMoveSpeed"]);
                        SpeedBuffTimer.Change((int)(Configs["SpeedBuffDuration"]), 0);
                        if (Velocity.length > 0)
                            Velocity = new Vector(Velocity.angle, moveSpeed + GlueExtraMoveSpeed);
                        tool = ToolType.Empty;
                    }
                    break;
                case ToolType.StrenthBuff:
                    {
                        MaxThrowDistance += (int)(Configs["StrenthBuffExtraThrowDistance"]);
                        StrengthBuffTimer.Change((int)(Configs["StrenthBuffDuration"]), 0);
                        tool = ToolType.Empty;
                    }
                    break;
                case ToolType.Fertilizer:
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
                    break;
                case ToolType.WaveGlue:
                    {
                        XYPosition xyPosition1 = Position.GetMid() + 2 * EightCornerVector[facingDirection];
                        if (WorldMap.Grid[(int)xyPosition1.x, (int)xyPosition1.y].ContainsType(typeof(Block)))
                        { Console.WriteLine("物品使用失败（无效的陷阱放置地点）！"); break; }
                        new Trigger(xyPosition1.x, xyPosition1.y, TriggerType.WaveGlue, team).Parent = WorldMap;
                        tool = ToolType.Empty;
                    }
                    break;
                case ToolType.LandMine:
                    {
                        XYPosition xyPosition1 = Position.GetMid() + 2 * EightCornerVector[facingDirection];
                        if (WorldMap.Grid[(int)xyPosition1.x, (int)xyPosition1.y].ContainsType(typeof(Block)))
                        { Console.WriteLine("物品使用失败（无效的陷阱放置地点）！"); break; }
                        new Trigger(xyPosition1.x, xyPosition1.y, TriggerType.Mine, team).Parent = WorldMap;
                        tool = ToolType.Empty;
                    }
                    break;
                case ToolType.Trap:
                    {
                        XYPosition xyPosition1 = Position.GetMid() + 2 * EightCornerVector[facingDirection];
                        if (WorldMap.Grid[(int)xyPosition1.x, (int)xyPosition1.y].ContainsType(typeof(Block)))
                        { Console.WriteLine("物品使用失败（无效的陷阱放置地点）！"); break; }
                        new Trigger(xyPosition1.x, xyPosition1.y, TriggerType.Trap, -1).Parent = WorldMap;
                        tool = ToolType.Empty;
                    }
                    break;
            }
        }
        public bool TouchTrigger(Trigger trigger)
        {
            if (trigger.OwnerTeam == CommunicationID.Item1)
                return false;
            switch (trigger.triggerType)
            {
                case TriggerType.WaveGlue:
                    {
                        isStepOnGlue = true;
                    }
                    break;
                case TriggerType.Mine:
                    {
                        Score += (int)(Configs["MineScore"]);
                    }
                    break;
                case TriggerType.Trap:
                    {
                        IsStun = true;
                        Velocity = new Vector(Velocity.angle, 0);
                    }
                    break;
                default:
                    return false;
            }
            return true;
        }

        protected void SpeakToFriend(string speakText)
        {
            Server.ServerDebug("Recieve text : " + speakText);
            for (int i = 0; i < Communication.Proto.Constants.PlayerCount; i++)
            {
                if (i == CommunicationID.Item2)
                    continue;
                lock (Program.MessageToClientLock)
                    Program.MessageToClient.GameObjectMessageList[Program.PlayerList[new Tuple<int, int>(CommunicationID.Item1, i)].ID].SpeakText = speakText;
            }
        }
    }
}
