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
        public Talent Talent
        {
            get { return _talent; }
            set
            {
                _talent = value;
                if (_talent == Talent.Runner) moveSpeed += (double)(Configs["RunnerTalentExtraMoveSpeed"]);
                else if (_talent == Talent.StrongMan) MaxThrowDistance += (int)(Configs["StrenthTalentExtraMoveSpeed"]);
                else if (_talent == Talent.LuckyBoy)
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

            lock (Program.MessageToClientLock)
            {
                Program.MessageToClient.GameObjectMessageList.Add(
                    this.ID,
                    new GameObjectMessage
                    {
                        ObjType = ObjTypeMessage.People,
                        IsMoving = false,
                        Position = new XYPositionMessage { X = this.Position.x, Y = this.Position.y },
                        Direction = (DirectionMessage)(int)this.facingDirection,
                        DishType = (DishTypeMessage)dish,
                        ToolType = (ToolTypeMessage)tool
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
            this.MoveComplete += new MoveCompleteHandler(ChangePositionInMessage);
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
                    if (msg.MoveDirection < 0)
                        msg.MoveDirection = 0;
                    else if (msg.MoveDirection >= DirectionMessage.DirectionSize)
                        msg.MoveDirection = DirectionMessage.DirectionSize - 1;
                    if (msg.MoveDuration < 0)
                        msg.MoveDuration = 0;
                    else if (msg.MoveDuration > 10000)
                        msg.MoveDuration = 10000;
                    Move((Direction)msg.MoveDirection, msg.MoveDuration);
                    break;
                case CommandTypeMessage.Pick:
                    Pick();
                    break;
                case CommandTypeMessage.Put:
                    if (msg.ThrowDistance < 0)
                        msg.ThrowDistance = 0;
                    else if (msg.ThrowDistance >= MaxThrowDistance)
                        msg.ThrowDistance = MaxThrowDistance;
                    Put(msg.ThrowDistance, msg.IsThrowDish);
                    break;
                case CommandTypeMessage.Use:
                    if (msg.Parameter1 < -99)
                        msg.Parameter1 = -99;
                    else if (msg.Parameter1 > 99)
                        msg.Parameter1 = 99;
                    if (msg.Parameter2 < -99)
                        msg.Parameter2 = -99;
                    else if (msg.Parameter2 > 99)
                        msg.Parameter2 = 99;
                    Use(msg.UseType, msg.Parameter1, msg.Parameter2);
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

        void ChangePositionInMessage(GameObject thisGameObject)
        {
            lock (Program.MessageToClientLock)
            {
                Program.MessageToClient.GameObjectMessageList[thisGameObject.ID].Position.X = thisGameObject.Position.x;
                Program.MessageToClient.GameObjectMessageList[thisGameObject.ID].Position.Y = thisGameObject.Position.y;
                Program.MessageToClient.GameObjectMessageList[thisGameObject.ID].Direction = (DirectionMessage)((Player)thisGameObject).facingDirection;
            }
        }

        public override void Pick()
        {
            XYPosition[] toCheckPositions = new XYPosition[] { Position, Position + 2 * EightCornerVector[facingDirection] };
            foreach (var xypos in toCheckPositions)
            {
                Block? block = null;
                if (WorldMap.Grid[(int)xypos.x, (int)xypos.y].ContainsType(typeof(FoodPoint)))
                    block = (Block)WorldMap.Grid[(int)xypos.x, (int)xypos.y].GetFirstObject(typeof(FoodPoint));
                else if (WorldMap.Grid[(int)xypos.x, (int)xypos.y].ContainsType(typeof(Cooker)))
                {
                    block = (Cooker)WorldMap.Grid[(int)xypos.x, (int)xypos.y].GetFirstObject(typeof(Cooker));
                    if (((Cooker)block).ProtectedTeam != team && ((Cooker)block).ProtectedTeam >= 0)
                        block = null;
                }
                if (block != null && block.Dish != DishType.Empty)
                {
                    DishType temp = Dish;
                    Dish = block.GetDish(Dish);
                    if (temp != DishType.Empty)
                    {
                        new Dish(Position.x, Position.y, temp).Parent = WorldMap;
                        //Server.ServerDebug(111.ToString());
                    }
                    Server.ServerDebug("Player : " + ID + " Get Dish " + Dish.ToString());
                    return;
                }

                if (WorldMap.Grid[(int)xypos.x, (int)xypos.y].ContainsLayer(ItemLayer))
                    foreach (var item in WorldMap.Grid[(int)xypos.x, (int)xypos.y].GetObjects(ItemLayer))
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
            Server.ServerDebug("没东西捡");
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
                dishToThrow.Layer = FlyingLayer;
                dishToThrow.Parent = WorldMap;
                dishToThrow.Velocity = new Vector((double)(int)facingDirection * Math.PI / 4, (int)Configs["ItemMoveSpeed"]);
                dishToThrow.StopMovingTimer.Change(dueTime, 0);
                Dish = DishType.Empty;
            }
            else if (tool != ToolType.Empty && !isThrowDish)
            {
                Tool toolToThrow = new Tool(Position.x, Position.y, tool);
                toolToThrow.Layer = FlyingLayer;
                toolToThrow.Parent = WorldMap;
                toolToThrow.Velocity = new Vector((double)(int)facingDirection * Math.PI / 4, (int)Configs["ItemMoveSpeed"]);
                toolToThrow.StopMovingTimer.Change(dueTime, 0);
                Tool = ToolType.Empty;
            }
            else Server.ServerDebug("没有可以扔的东西");
            status = CommandType.Stop;
            Velocity = new Vector(0, 0);
        }
        public override void Use(int type, int parameter_1, int parameter_2 = 0)
        {
            if (type == 0)//type为0表示使用厨具做菜和提交菜品
            {
                XYPosition xyPosition1 = Position + 2 * EightCornerVector[facingDirection];
                if (WorldMap.Grid[(int)xyPosition1.x, (int)xyPosition1.y].ContainsType(typeof(Cooker)))
                {
                    Cooker cooker = (Cooker)WorldMap.Grid[(int)xyPosition1.x, (int)xyPosition1.y].GetFirstObject(typeof(Cooker));
                    if (cooker.isCooking == false)
                        cooker.UseCooker(team, Talent);
                }
                else if (WorldMap.Grid[(int)xyPosition1.x, (int)xyPosition1.y].ContainsType(typeof(TaskPoint)))
                {
                    int tempScore = ((TaskPoint)WorldMap.Grid[(int)xyPosition1.x, (int)xyPosition1.y].GetFirstObject(typeof(TaskPoint))).HandIn(Dish);
                    Score += tempScore;
                    Dish = DishType.Empty;
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

        public void GetTalent(Talent t)
        {
            Talent = t;
        }

        public void UseTool(int parameter)
        {
            switch (tool)
            {
                case ToolType.TigerShoes:
                case ToolType.TeleScope:
                case ToolType.BreastPlate:
                case ToolType.Empty: Server.ServerDebug("物品使用失败（为空或无需使用）！"); break;
                case ToolType.Condiment:
                    {
                        XYPosition xyPosition1 = Position + 2 * EightCornerVector[facingDirection];
                        if (WorldMap.Grid[(int)xyPosition1.x, (int)xyPosition1.y].ContainsType(typeof(TaskPoint)))
                        {
                            int temp = ((TaskPoint)WorldMap.Grid[(int)xyPosition1.x, (int)xyPosition1.y].GetFirstObject(typeof(TaskPoint))).HandIn(Dish);
                            if (temp > 0)
                            {
                                if (Talent == Talent.Cook) Score += (int)(temp * (1 + (double)(Configs["CookCondimentScoreParameter"])));
                                Score += (int)(temp * (1 + (double)(Configs["CondimentScoreParameter"])));
                                Dish = DishType.Empty;
                            }
                            else Server.ServerDebug("物品使用失败（提交任务失败）！");
                        }
                        Tool = ToolType.Empty;
                    }
                    break;
                case ToolType.SpeedBuff:
                    {
                        SpeedBuffExtraMoveSpeed = (double)(Configs["SpeedBuffExtraMoveSpeed"]);
                        if (Talent != Talent.Technician) SpeedBuffTimer.Change((int)(Configs["SpeedBuffDuration"]), 0);
                        else SpeedBuffTimer.Change((int)(Configs["TechnicianSpeedBuffDuration"]), 0);
                        if (Velocity.length > 0)
                            Velocity = new Vector(Velocity.angle, MoveSpeed);
                        Tool = ToolType.Empty;
                    }
                    break;
                case ToolType.StrenthBuff:
                    {
                        StrenthBuffThrowDistance = (int)(Configs["StrenthBuffExtraThrowDistance"]);
                        if (Talent != Talent.Technician) SpeedBuffTimer.Change((int)(Configs["StrenthBuffDuration"]), 0);
                        else SpeedBuffTimer.Change((int)(Configs["TechnicianStrenthBuffDuration"]), 0);
                        Tool = ToolType.Empty;
                    }
                    break;
                case ToolType.Fertilizer:
                    {
                        XYPosition xyPosition1 = Position.GetMid() + 2 * EightCornerVector[facingDirection];
                        if (WorldMap.Grid[(int)xyPosition1.x, (int)xyPosition1.y].ContainsType(typeof(FoodPoint)))
                        {
                            ((FoodPoint)WorldMap.Grid[(int)xyPosition1.x, (int)xyPosition1.y].GetFirstObject(typeof(FoodPoint))).RefreshTime /= 2;
                        }
                        Tool = ToolType.Empty;
                    }
                    break;
                case ToolType.WaveGlue:
                    {
                        XYPosition xyPosition1 = Position.GetMid();
                        if (this.Talent == Talent.Technician)
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
                        Tool = ToolType.Empty;
                    }
                    break;
                case ToolType.LandMine:
                    {
                        XYPosition xyPosition1 = Position.GetMid();
                        if (WorldMap.Grid[(int)xyPosition1.x, (int)xyPosition1.y].ContainsType(typeof(Block)))
                        { Server.ServerDebug("物品使用失败（无效的陷阱放置地点）！"); break; }
                        new Trigger(xyPosition1.x, xyPosition1.y, TriggerType.Mine, team).Parent = WorldMap;
                        Tool = ToolType.Empty;
                    }
                    break;
                case ToolType.Trap:
                    {
                        XYPosition xyPosition1 = Position.GetMid();
                        if (WorldMap.Grid[(int)xyPosition1.x, (int)xyPosition1.y].ContainsType(typeof(Block)))
                        { Server.ServerDebug("物品使用失败（无效的陷阱放置地点）！"); break; }
                        new Trigger(xyPosition1.x, xyPosition1.y, TriggerType.Trap, -1).Parent = WorldMap;
                        Tool = ToolType.Empty;
                    }
                    break;
                case ToolType.SpaceGate:
                    {
                        int dx = (parameter / 100) % 100, dy = parameter % 100;
                        if (parameter >= 100000) { parameter -= 100000; dx = -dx; }
                        if (parameter >= 10000) { dy = -dy; }
                        if (Talent == Talent.Technician)
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
                        { Position = previous; }
                        Tool = ToolType.Empty;
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
                        if (Tool != ToolType.BreastPlate) Score += trigger.parameter;
                        else Tool = ToolType.Empty;
                    }
                    break;
                case TriggerType.Trap:
                    {
                        if (Tool != ToolType.BreastPlate)
                        {
                            IsStun = trigger.parameter;
                            Velocity = new Vector(Velocity.angle, 0);
                        }
                        else Tool = ToolType.Empty;
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
