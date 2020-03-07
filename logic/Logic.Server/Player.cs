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
        protected System.Threading.Timer LuckTalentTimer;
        public CommandType status = CommandType.Stop;
        protected bool isStepOnGlue = false;

        protected int _isStun = 0;
        protected int IsStun
        {
            get { return _isStun; }
            set
            {
                _isStun = value;
                if (value > 0)
                {
                    StunTimer.Change(value, 0);
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
        public TALENT Talent
        {
            get { return _talent; }
            set
            {
                _talent = value;
                if (_talent == TALENT.Runner) moveSpeed += (double)(Configs["RunnerTalentExtraMoveSpeed"]);
                else if (_talent == TALENT.StrongMan) MaxThrowDistance += (int)(Configs["StrenthTalentExtraMoveSpeed"]);
                else if (_talent == TALENT.LuckyBoy)
                {
                    LuckTalentTimer = new System.Threading.Timer(Item, null, 30000, (int)Configs["LuckTalentRefreshTime"]);
                    void Item(object i)
                    {
                        if (Tool == ToolType.Empty) Tool = (ToolType)Program.Random.Next(0, (int)ToolType.Size - 1);
                        else new Tool(Position.x, Position.y, (ToolType)Program.Random.Next(0, (int)ToolType.Size - 1)).Parent = WorldMap;
                    }
                }
            }
        }
        protected int Score
        {
            get { return base._score; }
            set
            {
                base._score = value;
                lock (Program.MessageToClientLock)
                    Program.MessageToClient.GameObjectMessageList[this.ID].Score = base._score;
            }
        }

        public Player(double x, double y) :
            base(x, y)
        {
            Parent = WorldMap;
            MoveStopTimer = new System.Threading.Timer((i) => { Velocity = new Vector(Velocity.angle, 0); status = CommandType.Stop; Program.MessageToClient.GameObjectMessageList[this.ID].IsMoving = false; });
            StunTimer = new System.Threading.Timer((i) => { _isStun = 0; });
            SpeedBuffTimer = new System.Threading.Timer((i) => { SpeedBuffExtraMoveSpeed = 0; });
            StrengthBuffTimer = new System.Threading.Timer((i) => { StrenthBuffThrowDistance = 0; });
            PositionChangeComplete += new PositionChangeCompleteHandler(ChangePositionInMessage);
            void ChangePositionInMessage(GameObject thisGameObject)
            {
                lock (Program.MessageToClientLock)
                {
                    Program.MessageToClient.GameObjectMessageList[thisGameObject.ID].Position.X = thisGameObject.Position.x;
                    Program.MessageToClient.GameObjectMessageList[thisGameObject.ID].Position.Y = thisGameObject.Position.y;
                }
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
            this.MoveStart += new MoveStartHandler(
                (thisGameObject) =>
                {
                    if (isStepOnGlue)
                    {
                        GlueExtraMoveSpeed = (int)Configs["WaveGlueExtraMoveSpeed"];
                        if (Velocity.length > 3)
                            this.Velocity = new Vector(Velocity.angle, MoveSpeed);
                    }
                    else
                    {
                        GlueExtraMoveSpeed = 0;
                        if (Velocity.length > 0 && Velocity.length < 4)
                            this.Velocity = new Vector(Velocity.angle, MoveSpeed);
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
            if (IsStun > 0) return;
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
                    Use(msg.UseType, msg.Parameter);
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
            Move(new MoveEventArgs((int)direction * Math.PI / 4, MoveSpeed / Constant.Constant.FrameRate));
        }

        public override void Move(Direction direction, int durationMilliseconds)
        {
            this.facingDirection = direction;
            this.Velocity = new Vector(((double)(int)direction) * Math.PI / 4, MoveSpeed);
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
                        if ((block.blockType == BlockType.FoodPoint || (block.blockType == BlockType.Cooker && (block.ProtectedTeam == team || block.ProtectedTeam < 0)))
                            && block.Dish != DishType.Empty)
                        {
                            DishType temp = Dish;
                            Dish = block.GetDish(Dish);
                            if (temp != DishType.Empty) { new Dish(Position.x, Position.y, temp).Parent = WorldMap;Console.WriteLine(111); }
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
                        tool = ((Tool)item).GetTool(tool);
                        lock (Program.MessageToClientLock)
                            Program.MessageToClient.GameObjectMessageList[this.ID].ToolType = (ToolTypeMessage)tool;
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
                toolToThrow.Parent = WorldMap;
                toolToThrow.Layer = (int)MapLayer.FlyingLayer;
                toolToThrow.Velocity = new Vector((double)(int)facingDirection * Math.PI / 4, (int)Configs["ItemMoveSpeed"]);
                toolToThrow.StopMovingTimer.Change(dueTime, 0);
                Tool = ToolType.Empty;
            }
            else Console.WriteLine("没有可以扔的东西");
            status = CommandType.Stop;
            Velocity = new Vector(0, 0);
        }
        public override void Use(int type, string parameter_1, string parameter_2 = null)
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
                            if (block.Cooking == false) block.UseCooker(team, Talent);
                        }
                        else if (block.blockType == BlockType.TaskPoint)
                        {
                            int temp = block.HandIn(Dish);
                            if(temp>0){ Score += temp; Dish = DishType.Empty; }
                            else Console.WriteLine("提交任务失败！");
                        }
                        break;
                    }
                }
            }
            else//否则为使用手中道具
            {
                UseTool(parameter_1);
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
            Talent = t;
        }

        public void UseTool(string parameter)
        {
            switch (tool)
            {
                case ToolType.TigerShoes:
                case ToolType.TeleScope:
                case ToolType.BreastPlate:
                case ToolType.Empty: Console.WriteLine("物品使用失败（为空或无需使用）！"); break;
                case ToolType.Condiment:
                    {
                        XYPosition xyPosition1 = Position + 2 * EightCornerVector[facingDirection];
                        if (WorldMap.Grid[(int)xyPosition1.x, (int)xyPosition1.y].ContainsType(typeof(Block)))
                        {
                            foreach (Block block in WorldMap.Grid[(int)xyPosition1.x, (int)xyPosition1.y].GetType(typeof(Block)))
                            {
                                if (block.blockType == BlockType.TaskPoint)
                                {
                                    int temp = block.HandIn(Dish);
                                    if (temp > 0)
                                    {
                                        if(Talent==TALENT.Cook) Score += (int)(temp * (1 + (double)(Configs["CookCondimentScoreParameter"])));
                                        Score += (int)(temp*(1+ (double)(Configs["CondimentScoreParameter"]))); 
                                        Dish = DishType.Empty; Tool = ToolType.Empty; 
                                    }
                                    else Console.WriteLine("物品使用失败（提交任务失败）！");
                                }
                                break;
                            }
                        }
                    }
                    break;
                case ToolType.SpeedBuff:
                    {
                        SpeedBuffExtraMoveSpeed = (double)(Configs["SpeedBuffExtraMoveSpeed"]);
                        if(Talent!=TALENT.Technician)SpeedBuffTimer.Change((int)(Configs["SpeedBuffDuration"]), 0);
                        else SpeedBuffTimer.Change((int)(Configs["TechnicianSpeedBuffDuration"]), 0);
                        if (Velocity.length > 0)
                            Velocity = new Vector(Velocity.angle, MoveSpeed);
                        tool = ToolType.Empty;
                    }
                    break;
                case ToolType.StrenthBuff:
                    {
                        StrenthBuffThrowDistance = (int)(Configs["StrenthBuffExtraThrowDistance"]);
                        if (Talent != TALENT.Technician) SpeedBuffTimer.Change((int)(Configs["StrenthBuffDuration"]), 0);
                        else SpeedBuffTimer.Change((int)(Configs["TechnicianStrenthBuffDuration"]), 0);
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
                        XYPosition xyPosition1 = Position.GetMid();
                        if (this.Talent == TALENT.Technician)
                        {
                            for (int i = -2; i <= 2; i++)
                            {
                                for (int j = -2; j <= 2; j++)
                                {
                                    if (!WorldMap.Grid[(int)xyPosition1.x + i, (int)xyPosition1.y + j].ContainsType(typeof(Block)))
                                        new Trigger(xyPosition1.x + i, xyPosition1.y + j, TriggerType.WaveGlue, team).Parent = WorldMap;
                                }
                            }
                        }
                        else
                        {
                            for (int i = -1; i <= 1; i++)
                            {
                                for (int j = -1; j <= 1; j++)
                                {
                                    if (!WorldMap.Grid[(int)xyPosition1.x + i, (int)xyPosition1.y + j].ContainsType(typeof(Block)))
                                        new Trigger(xyPosition1.x + i, xyPosition1.y + j, TriggerType.WaveGlue, team).Parent = WorldMap;
                                }
                            }
                        }
                        tool = ToolType.Empty;
                    }
                    break;
                case ToolType.LandMine:
                    {
                        XYPosition xyPosition1 = Position.GetMid();
                        if (WorldMap.Grid[(int)xyPosition1.x, (int)xyPosition1.y].ContainsType(typeof(Block)))
                        { Console.WriteLine("物品使用失败（无效的陷阱放置地点）！"); break; }
                        new Trigger(xyPosition1.x, xyPosition1.y, TriggerType.Mine, team).Parent = WorldMap;
                        tool = ToolType.Empty;
                    }
                    break;
                case ToolType.Trap:
                    {
                        XYPosition xyPosition1 = Position.GetMid();
                        if (WorldMap.Grid[(int)xyPosition1.x, (int)xyPosition1.y].ContainsType(typeof(Block)))
                        { Console.WriteLine("物品使用失败（无效的陷阱放置地点）！"); break; }
                        new Trigger(xyPosition1.x, xyPosition1.y, TriggerType.Trap, -1).Parent = WorldMap;
                        tool = ToolType.Empty;
                    }
                    break;
                case ToolType.SpaceGate:
                    {

                        string[] i = parameter.Split(',');
                        Console.WriteLine(i[0] + " " + i[1]);
                        int dx = int.Parse(i[0]), dy = int.Parse(i[1]);
                        if(Talent==TALENT.Technician)
                        {
                            if (Math.Abs(dx) > (int)Configs["TechnicianSpaceGateMaxDistance"]) dx = (int)Configs["TechnicianSpaceGateMaxDistance"] * (dx / Math.Abs(dx));
                            if (Math.Abs(dy) > (int)Configs["TechnicianSpaceGateMaxDistance"]) dy = (int)Configs["TechnicianSpaceGateMaxDistance"] * (dy / Math.Abs(dy));
                        }
                        else
                        {
                            if (Math.Abs(dx) > (int)Configs["SpaceGateMaxDistance"]) dx = (int)Configs["SpaceGateMaxDistance"] * (dx / Math.Abs(dx));
                            if (Math.Abs(dy) > (int)Configs["SpaceGateMaxDistance"]) dy = (int)Configs["SpaceGateMaxDistance"] * (dy / Math.Abs(dy));
                        }
                        XYPosition previous = Position.GetMid();
                        XYPosition aim = Position + new XYPosition(dx, dy);
                        Position = aim;
                        if (Math.Abs(Position.x - aim.x) > 0.001 || Math.Abs(Position.y - aim.y) > 0.001)
                            Position = previous;
                    }
                    break;
            }
            lock (Program.MessageToClientLock)
                Program.MessageToClient.GameObjectMessageList[this.ID].ToolType = (ToolTypeMessage)tool;
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
                        Score += trigger.parameter;
                    }
                    break;
                case TriggerType.Trap:
                    {
                        IsStun = trigger.parameter;
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
