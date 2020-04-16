using Communication.Proto;
using Communication.Server;
using Logic.Constant;
using System;
using THUnity2D;
using static Logic.Constant.Constant;
using static Logic.Constant.MapInfo;
using static THUnity2D.Tools;
using Direction = Communication.Proto.Direction;

namespace Logic.Server
{
    class Player : Character
    {
        protected System.Threading.Timer MoveStopTimer;
        protected System.Threading.Timer SpeedBuffTimer;
        protected System.Threading.Timer StrengthBuffTimer;
        protected System.Threading.Timer LuckTalentTimer;
        public CommandType status = CommandType.Stop;
        protected bool tempIsStepOnGlue = false;
        protected bool isStepOnGlue = false;

        protected int _isStun = 0;
        protected int IsStun
        {
            get => _isStun;
            private set
            {
                _isStun = value;
                if (value > 0)
                    StunTimer.Change(value, 0);
            }
        }
        protected System.Threading.Timer StunTimer;

        protected DishType Dish
        {
            get => dish;
            private set => Program.MessageToClient.GameObjectList[this.ID].DishType = dish = value;
        }

        protected ToolType Tool
        {
            get { return tool; }
            private set
            {
                DeFunction(tool);
                tool = value;
                Function(tool);
                Program.MessageToClient.GameObjectList[this.ID].ToolType = tool;
            }
        }
        public Talent Talent
        {
            get { return _talent; }
            internal set
            {
                _talent = value;
                switch (_talent)
                {
                    case Talent.Runner: MoveSpeed += (double)Configs["Talent"]["Runner"]["ExtraMoveSpeed"]; break;
                    case Talent.StrongMan: MaxThrowDistance += (int)Configs["Talent"]["StrongMan"]["ExtraThrowDistance"]; break;
                    case Talent.LuckyBoy:
                        LuckTalentTimer = new System.Threading.Timer(Item, null, 30000, (int)Configs["Talent"]["LuckyBoy"]["RefreshTime"]);
                        void Item(object i)
                        {
                            if (Tool == ToolType.ToolEmpty) Tool = (ToolType)Program.Random.Next(1, (int)ToolType.ToolSize - 1);
                            else new Tool(Position.x, Position.y, (ToolType)Program.Random.Next(1, (int)ToolType.ToolSize - 1)).Parent = WorldMap;
                        }
                        break;
                }
            }
        }
        public new int SightRange
        {
            get => _sightRange;
            private set => Program.MessageToClient.GameObjectList[this.ID].SightRange = _sightRange = value;
        }
        protected int Score
        {
            get => Program.MessageToClient.Scores[CommunicationID.Item1];
            private set => Program.MessageToClient.Scores[CommunicationID.Item1] = value;
        }
        protected new double MoveSpeed
        {
            get => _moveSpeed;
            private set => Program.MessageToClient.GameObjectList[this.ID].MoveSpeed = _moveSpeed = value;

        }
        protected new THUnity2D.Direction FacingDirection
        {
            get => _facingDirection;
            private set => Program.MessageToClient.GameObjectList[this.ID].Direction = (Direction)(_facingDirection = value);
        }

        public Player(double x, double y) :
            base(x, y)
        {
            Parent = WorldMap;
            MoveStopTimer = new System.Threading.Timer((i) => { Velocity = new Vector(Velocity.angle, 0); status = CommandType.Stop; Program.MessageToClient.GameObjectList[this.ID].IsMoving = false; });
            StunTimer = new System.Threading.Timer((i) => { _isStun = 0; });
            SpeedBuffTimer = new System.Threading.Timer((i) => { MoveSpeed -= (double)Configs["Tool"]["SpeedBuff"]["ExtraMoveSpeed"]; });
            StrengthBuffTimer = new System.Threading.Timer((i) => { StrenthBuffThrowDistance = 0; });
            PositionChangeComplete += new PositionChangeCompleteHandler(ChangePositionInMessage);

            lock (Program.MessageToClientLock)
            {
                Program.MessageToClient.GameObjectList.Add(
                    this.ID,
                    new Communication.Proto.GameObject
                    {
                        ObjType = ObjType.People,
                        IsMoving = false,
                        PositionX = this.Position.x,
                        PositionY = this.Position.y,
                        SightRange = this.SightRange,
                        Direction = (Direction)this.FacingDirection,
                        MoveSpeed = this.MoveSpeed,
                        DishType = dish,
                        ToolType = tool,
                        Team = CommunicationID.Item1
                    });
            }
            MoveStart += new MoveStartHandler(
                (thisGameObject) =>
                {
                    tempIsStepOnGlue = false;
                });
            MoveComplete += new MoveCompleteHandler(ChangePositionInMessage);
            MoveComplete += new MoveCompleteHandler(
                (thisGameObject) =>
                {
                    if (!isStepOnGlue && tempIsStepOnGlue)
                    {
                        MoveSpeed += (double)Configs["Trigger"]["WaveGlue"]["ExtraMoveSpeed"];
                        Velocity = new Vector(Velocity.angle, MoveSpeed);
                        isStepOnGlue = true;
                    }
                    else if (isStepOnGlue && !tempIsStepOnGlue)
                    {
                        MoveSpeed -= (double)Configs["Trigger"]["WaveGlue"]["ExtraMoveSpeed"];
                        Velocity = new Vector(Velocity.angle, MoveSpeed);
                        isStepOnGlue = false;
                    }
                }
                );
            OnTrigger += new TriggerHandler(
                (triggerGameObjects) =>
                {
                    foreach (Trigger trigger in triggerGameObjects)
                        this.TouchTrigger(trigger);
                });
        }
        public void ExecuteMessage(MessageToServer msg)
        {
            if (IsStun > 0) return;
            if (msg.CommandType < 0 || msg.CommandType >= CommandType.Size)
                return;
            switch (msg.CommandType)
            {
                case CommandType.Move:
                    if (msg.MoveDirection < 0)
                        msg.MoveDirection = 0;
                    else if (msg.MoveDirection >= Direction.Size)
                        msg.MoveDirection = Direction.Size - 1;
                    if (msg.MoveDuration < 0)
                        msg.MoveDuration = 0;
                    else if (msg.MoveDuration > 10000)
                        msg.MoveDuration = 10000;
                    Move((THUnity2D.Direction)msg.MoveDirection, msg.MoveDuration);
                    break;
                case CommandType.Pick:
                    Pick();
                    break;
                case CommandType.Put:
                    if (msg.ThrowDistance < 0)
                        msg.ThrowDistance = 0;
                    else if (msg.ThrowDistance >= MaxThrowDistance)
                        msg.ThrowDistance = MaxThrowDistance;
                    Put(msg.ThrowDistance, msg.ThrowAngle, msg.IsThrowDish);
                    break;
                case CommandType.Use:
                    if (msg.Parameter1 < 0)
                        msg.Parameter1 = 0;
                    else if (msg.Parameter1 > 100)
                        msg.Parameter1 = 100;
                    Use(msg.UseType, msg.Parameter1, msg.Parameter2);
                    break;
                case CommandType.Speak:
                    SpeakToFriend(msg.SpeakText.Substring(0, 15));
                    break;
                default:
                    break;
            }
        }

        public void Move(THUnity2D.Direction direction)
        {
            this.FacingDirection = direction;
            Move(new MoveEventArgs((int)direction * Math.PI / 4, MoveSpeed / Constant.Constant.FrameRate));
        }

        public override void Move(THUnity2D.Direction direction, int durationMilliseconds)
        {
            this.FacingDirection = direction;
            this.status = CommandType.Move;
            //lock (Program.MessageToClientLock)
            Program.MessageToClient.GameObjectList[this.ID].IsMoving = true;
            int dueTime = durationMilliseconds - (int)HalfTimeIntervalInMillisecond;
            if (dueTime > 0)
            {
                this.Velocity = new Vector((int)direction * Math.PI / 4, MoveSpeed);
                MoveStopTimer.Change(dueTime, 0);
            }
        }

        void ChangePositionInMessage(THUnity2D.GameObject thisGameObject)
        {
            //lock (Program.MessageToClientLock)
            //{
            Program.MessageToClient.GameObjectList[thisGameObject.ID].PositionX = thisGameObject.Position.x;
            Program.MessageToClient.GameObjectList[thisGameObject.ID].PositionY = thisGameObject.Position.y;
            Program.MessageToClient.GameObjectList[thisGameObject.ID].Direction = (Direction)((Player)thisGameObject).FacingDirection;
            //}
        }

        public override void Pick()
        {
            XYPosition[] toCheckPositions = new XYPosition[] { Position, Position + 2 * EightCornerVector[FacingDirection] };
            foreach (var xypos in toCheckPositions)
            {
                Block? block = null;
                if (WorldMap.Grid[(int)xypos.x, (int)xypos.y].ContainsType(typeof(FoodPoint)))
                    block = (Block)WorldMap.Grid[(int)xypos.x, (int)xypos.y].GetFirstObject(typeof(FoodPoint));
                else if (WorldMap.Grid[(int)xypos.x, (int)xypos.y].ContainsType(typeof(Cooker)))
                {
                    block = (Cooker)WorldMap.Grid[(int)xypos.x, (int)xypos.y].GetFirstObject(typeof(Cooker));
                    if (((Cooker)block).ProtectedTeam != CommunicationID.Item1 && ((Cooker)block).ProtectedTeam >= 0)
                        block = null;
                }
                if (block != null && block.Dish != DishType.DishEmpty)
                {
                    DishType temp = Dish;
                    Dish = block.GetDish(Dish);
                    if (temp != DishType.DishEmpty)
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
                            Tool = ((Tool)item).GetTool(tool);
                            Server.ServerDebug("Player : " + ID + " Get Tool " + tool.ToString());
                            return;
                        }
                    }
            }
            //Server.ServerDebug("没东西捡");
            status = CommandType.Stop;
            Velocity = new Vector(0, 0);
        }
        public override void Put(double distance, double angle, bool isThrowDish)
        {
            if (distance > MaxThrowDistance) distance = MaxThrowDistance;
            int dueTime = (int)((double)1000 * distance / (double)Configs["ItemMoveSpeed"]) - (int)HalfTimeIntervalInMillisecond;//注意到如果直接用 distance / ItemMoveSpeed 会在最后多走一步，所以必须做一些数值处理

            Obj ItemToThrow = null;
            if (Dish != DishType.DishEmpty && isThrowDish)
            {
                ItemToThrow = new Dish(Position.x, Position.y, Dish);
                Dish = DishType.DishEmpty;
            }
            else if (tool != ToolType.ToolEmpty && !isThrowDish)
            {
                ItemToThrow = new Tool(Position.x, Position.y, tool);
                Tool = ToolType.ToolEmpty;
            }
            else
            {
                Server.ServerDebug("没有可以扔的东西");
                return;
            }
            if (dueTime > 0)
            {
                ItemToThrow.Layer = FlyingLayer;
                ItemToThrow.Parent = WorldMap;
                ItemToThrow.Velocity = new Vector(angle, (int)Configs["ItemMoveSpeed"]);
                ItemToThrow.StopMovingTimer.Change(dueTime, 0);
            }
            else
                ItemToThrow.Parent = WorldMap;

            status = CommandType.Stop;
            Velocity = new Vector(Velocity.angle, 0);
        }
        public override void Use(int type, double parameter_1, double parameter_2)
        {
            if (type == 0)//type为0表示使用厨具做菜和提交菜品
            {
                XYPosition xyPosition1 = Position + 2 * EightCornerVector[FacingDirection];
                if (WorldMap.Grid[(int)xyPosition1.x, (int)xyPosition1.y].ContainsType(typeof(Cooker)))
                {
                    Cooker cooker = (Cooker)WorldMap.Grid[(int)xyPosition1.x, (int)xyPosition1.y].GetFirstObject(typeof(Cooker));
                    if (cooker.isCooking == false)
                        cooker.UseCooker(CommunicationID.Item1, Talent);
                }
                else if (WorldMap.Grid[(int)xyPosition1.x, (int)xyPosition1.y].ContainsType(typeof(TaskPoint)))
                {
                    lock (Program.ScoreLocks[CommunicationID.Item1])
                        Score += ((TaskPoint)WorldMap.Grid[(int)xyPosition1.x, (int)xyPosition1.y].GetFirstObject(typeof(TaskPoint))).HandIn(Dish);
                    Dish = DishType.DishEmpty;
                }
            }
            else//否则为使用手中道具
            {
                UseTool(parameter_1, parameter_2);
            }
            status = CommandType.Stop;
            Velocity = new Vector(0, 0);
        }

        public void Function(ToolType type)//在捡起装备时生效，仅对捡起即生效的装备有用
        {
            switch (type)
            {
                case ToolType.TigerShoes: MoveSpeed += (double)Configs["Tool"]["TigerShoe"]["ExtraMoveSpeed"]; break;
                case ToolType.TeleScope: SightRange += (int)Configs["Tool"]["TeleScope"]["ExtraSightRange"]; break;
            }
        }
        public void DeFunction(ToolType type)//在丢弃装备时生效
        {
            switch (type)
            {
                case ToolType.TigerShoes: MoveSpeed -= (double)Configs["Tool"]["TigerShoe"]["ExtraMoveSpeed"]; break;
                case ToolType.TeleScope: SightRange -= (int)Configs["Tool"]["TeleScope"]["ExtraSightRange"]; break;
            }
        }

        public void GetTalent(Talent t)
        {
            Talent = t;
        }

        public void UseTool(double parameter1, double parameter2)
        {
            switch (tool)
            {
                case ToolType.TigerShoes:
                case ToolType.TeleScope:
                case ToolType.BreastPlate:
                case ToolType.ToolEmpty: Server.ServerDebug("物品使用失败（为空或无需使用）！"); break;
                case ToolType.Condiment:
                    {
                        XYPosition xyPosition1 = Position + 2 * EightCornerVector[FacingDirection];
                        if (WorldMap.Grid[(int)xyPosition1.x, (int)xyPosition1.y].ContainsType(typeof(TaskPoint)))
                        {
                            int temp = ((TaskPoint)WorldMap.Grid[(int)xyPosition1.x, (int)xyPosition1.y].GetFirstObject(typeof(TaskPoint))).HandIn(Dish);
                            if (temp > 0)
                            {
                                lock (Program.ScoreLocks[CommunicationID.Item1])
                                    Score += (int)(temp * (1 + (double)((Talent == Talent.Cook) ?
                                                                        Configs["Talent"]["Cook"]["Condiment"]["ScoreParameter"] : Configs["Tool"]["Condiment"]["ScoreParameter"])));
                                Dish = DishType.DishEmpty;
                            }
                            else Server.ServerDebug("物品使用失败（提交任务失败）！");
                        }
                        Tool = ToolType.ToolEmpty;
                    }
                    break;
                case ToolType.SpeedBuff:
                    {
                        MoveSpeed += (double)Configs["Tool"]["SpeedBuff"]["ExtraMoveSpeed"];
                        if (Talent != Talent.Technician) SpeedBuffTimer.Change((int)Configs["Tool"]["SpeedBuff"]["Duration"], 0);
                        else SpeedBuffTimer.Change((int)Configs["Talent"]["Technician"]["SpeedBuff"]["Duration"], 0);
                        if (Velocity.length > 0)
                            Velocity = new Vector(Velocity.angle, MoveSpeed);
                        Tool = ToolType.ToolEmpty;
                    }
                    break;
                case ToolType.StrengthBuff:
                    {
                        StrenthBuffThrowDistance = (int)Configs["Tool"]["StrengthBuff"]["ExtraThrowDistance"];
                        if (Talent != Talent.Technician) StrengthBuffTimer.Change((int)Configs["Tool"]["StrengthBuff"]["Duration"], 0);
                        else StrengthBuffTimer.Change((int)Configs["Talent"]["Technician"]["StrengthBuff"]["Duration"], 0);
                        Tool = ToolType.ToolEmpty;
                    }
                    break;
                case ToolType.Fertilizer:
                    {
                        XYPosition xyPosition1 = Position.GetMid() + 2 * EightCornerVector[FacingDirection];
                        if (WorldMap.Grid[(int)xyPosition1.x, (int)xyPosition1.y].ContainsType(typeof(FoodPoint)))
                        {
                            ((FoodPoint)WorldMap.Grid[(int)xyPosition1.x, (int)xyPosition1.y].GetFirstObject(typeof(FoodPoint))).RefreshTime /= 2;
                        }
                        Tool = ToolType.ToolEmpty;
                    }
                    break;
                case ToolType.WaveGlueBottle:
                    {
                        XYPosition xyPosition1 = Position.GetMid();
                        int radius = (this.Talent == Talent.Technician) ? 2 : 1;
                        for (int i = -radius; i <= radius; i++)
                        {
                            for (int j = -radius; j <= radius; j++)
                            {
                                if (!WorldMap.Grid[(int)xyPosition1.x + i, (int)xyPosition1.y + j].ContainsType(typeof(Block)))
                                    new Trigger(xyPosition1.x + i, xyPosition1.y + j, TriggerType.WaveGlue, CommunicationID.Item1, Talent).Parent = WorldMap;
                            }
                        }
                        Tool = ToolType.ToolEmpty;
                    }
                    break;
                case ToolType.LandMine:
                    {
                        XYPosition xyPosition1 = Position.GetMid();
                        if (WorldMap.Grid[(int)xyPosition1.x, (int)xyPosition1.y].ContainsType(typeof(Block)))
                        { Server.ServerDebug("物品使用失败（无效的地雷放置地点）！"); break; }
                        new Trigger(xyPosition1.x, xyPosition1.y, TriggerType.Mine, CommunicationID.Item1, Talent).Parent = WorldMap;
                        Tool = ToolType.ToolEmpty;
                    }
                    break;
                case ToolType.TrapTool:
                    {
                        XYPosition xyPosition1 = Position.GetMid();
                        if (WorldMap.Grid[(int)xyPosition1.x, (int)xyPosition1.y].ContainsType(typeof(Block)))
                        { Server.ServerDebug("物品使用失败（无效的陷阱放置地点）！"); break; }
                        new Trigger(xyPosition1.x, xyPosition1.y, TriggerType.Trap, CommunicationID.Item1, Talent).Parent = WorldMap;
                        Tool = ToolType.ToolEmpty;
                    }
                    break;
                case ToolType.SpaceGate:
                    {
                        //parameter1是距离，paameter2是角度
                        double maxDistance = (Talent == Talent.Technician) ? (double)Configs["Talent"]["Technician"]["SpaceGate"]["MaxDistance"] : (double)Configs["Tool"]["SpaceGate"]["MaxDistance"];
                        if (parameter1 < 0) parameter1 = 0;
                        else if (parameter1 > maxDistance) parameter1 = maxDistance;
                        XYPosition aim = Position + new XYPosition(parameter1 * Math.Cos(parameter2), parameter1 * Math.Sin(parameter2));
                        if (WorldMap.XYPositionIsLegal(aim, 1, 1, Layer))
                            Position = aim;
                        Tool = ToolType.ToolEmpty;
                    }
                    break;
                case ToolType.FlashBomb:
                    {
                        XYPosition xyPosition1 = Position.GetMid();
                        if (WorldMap.Grid[(int)xyPosition1.x, (int)xyPosition1.y].ContainsType(typeof(Block)))
                        { Server.ServerDebug("物品使用失败（无效的炸弹放置地点）！"); break; }
                        new Trigger(xyPosition1.x, xyPosition1.y, TriggerType.Bomb, CommunicationID.Item1, Talent).Parent = WorldMap;
                        Tool = ToolType.ToolEmpty;
                    }
                    break;
                case ToolType.ThrowHammer:
                    {
                        //parameter1是距离，paameter2是角度
                        double maxDistance = (Talent == Talent.StrongMan) ? (double)Configs["Talent"]["StrongMan"]["ThrowHammer"]["MaxDistance"] : (double)Configs["Tool"]["ThrowHammer"]["MaxDistance"];
                        if (parameter1 < 0) parameter1 = 0;
                        else if (parameter1 > maxDistance) parameter1 = maxDistance;
                        int dueTime = (int)((double)1000 * parameter1 / (double)Configs["ItemMoveSpeed"]) - (int)HalfTimeIntervalInMillisecond;
                        //XYPosition aim = Position + new XYPosition(parameter1 * Math.Cos(parameter2), parameter1 * Math.Sin(parameter2));
                        Trigger triggerToThrow = new Trigger(Position.x, Position.y, TriggerType.Hammer, CommunicationID.Item1, Talent);
                        triggerToThrow.Parent = WorldMap;
                        triggerToThrow.Velocity = new Vector(parameter2, (int)Configs["ItemMoveSpeed"]);
                        if (dueTime > 0)
                            triggerToThrow.StopMovingTimer.Change(dueTime, 0);
                        Tool = ToolType.ToolEmpty;
                    }
                    break;
                case ToolType.Bow:
                    {
                        //parameter1是距离，parameter2是角度
                        double maxDistance = (Talent == Talent.StrongMan) ? (double)Configs["Talent"]["StrongMan"]["Bow"]["MaxDistance"] : (double)Configs["Tool"]["Bow"]["MaxDistance"];
                        if (parameter1 < 0) parameter1 = 0;
                        else if (parameter1 > maxDistance) parameter1 = maxDistance;
                        int dueTime = (int)((double)1000 * parameter1 / (double)Configs["Trigger"]["Arrow"]["Speed"]) - (int)HalfTimeIntervalInMillisecond;
                        //XYPosition aim = Position + new XYPosition(parameter1 * Math.Cos(parameter2), parameter1 * Math.Sin(parameter2));
                        Trigger triggerToThrow = new Trigger(Position.x, Position.y, TriggerType.Arrow, CommunicationID.Item1, Talent);
                        triggerToThrow.Parent = WorldMap;
                        triggerToThrow.Velocity = new Vector(parameter2, (int)Configs["Trigger"]["Arrow"]["Speed"]);
                        if (dueTime > 0)
                            triggerToThrow.StopMovingTimer.Change(dueTime, 0);
                        Tool = ToolType.ToolEmpty;
                    }
                    break;
                case ToolType.Stealer:
                    {
                        XYPosition xyPositionToSteal = Position + 2 * EightCornerVector[FacingDirection];
                        if (WorldMap.Grid[(int)xyPositionToSteal.x, (int)xyPositionToSteal.y].ContainsType(typeof(Player)))
                        {
                            Player playerToSteal = (Player)WorldMap.Grid[(int)xyPositionToSteal.x, (int)xyPositionToSteal.y].GetFirstObject(typeof(Player));
                            new Dish(Position.x, Position.y, Dish).Parent = WorldMap;
                            Dish = playerToSteal.Dish;
                            playerToSteal.Dish = DishType.DishEmpty;
                        }
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
                    tempIsStepOnGlue = true;
                    break;
                case TriggerType.Mine:
                    if (Tool != ToolType.BreastPlate)
                    {
                        lock (Program.ScoreLocks[CommunicationID.Item1])
                            Score += trigger.hitScore;
                        IsStun = trigger.stunTime;
                        Velocity = new Vector(Velocity.angle, Velocity.length);
                    }
                    else Tool = ToolType.ToolEmpty;
                    trigger.Parent = null;
                    break;
                case TriggerType.Trap:
                    if (Tool != ToolType.BreastPlate)
                    {
                        IsStun = trigger.stunTime;
                        Velocity = new Vector(Velocity.angle, 0);
                    }
                    else Tool = ToolType.ToolEmpty;
                    trigger.Parent = null;
                    break;
                case TriggerType.Bomb:
                    if (Tool != ToolType.BreastPlate)
                    {
                        IsStun = trigger.stunTime;
                        Velocity = new Vector(Velocity.angle, 0);
                        if (Dish != DishType.DishEmpty)
                        {
                            new Dish(3 * Math.Cos(Program.Random.NextDouble() * 2 * Math.PI), 3 * Math.Sin(Program.Random.NextDouble() * 2 * Math.PI), Dish).Parent = WorldMap;
                            Dish = DishType.DishEmpty;
                        }
                        if (Tool != ToolType.ToolEmpty)
                        {
                            new Tool(3 * Math.Cos(Program.Random.NextDouble() * 2 * Math.PI), 3 * Math.Sin(Program.Random.NextDouble() * 2 * Math.PI), Tool).Parent = WorldMap;
                            Tool = ToolType.ToolEmpty;
                        }
                    }
                    else Tool = ToolType.ToolEmpty;
                    trigger.Parent = null;
                    break;
                case TriggerType.Arrow:
                    if (Tool != ToolType.BreastPlate)
                    {
                        lock (Program.ScoreLocks[CommunicationID.Item1])
                            Score += trigger.hitScore;
                        IsStun = trigger.stunTime;
                        Velocity = new Vector(Velocity.angle, 0);
                    }
                    else Tool = ToolType.ToolEmpty;
                    trigger.Parent = null;
                    break;
                case TriggerType.Hammer:
                    if (Tool != ToolType.BreastPlate)
                    {
                        IsStun = trigger.stunTime;
                        Velocity = new Vector(Velocity.angle, 0);
                        if (Dish != DishType.DishEmpty)
                        {
                            new Dish(1 * Math.Cos(Program.Random.NextDouble() * 2 * Math.PI), 1 * Math.Sin(Program.Random.NextDouble() * 2 * Math.PI), Dish).Parent = WorldMap;
                            Dish = DishType.DishEmpty;
                        }
                        if (Tool != ToolType.ToolEmpty)
                        {
                            new Tool(1 * Math.Cos(Program.Random.NextDouble() * 2 * Math.PI), 1 * Math.Sin(Program.Random.NextDouble() * 2 * Math.PI), Tool).Parent = WorldMap;
                            Tool = ToolType.ToolEmpty;
                        }
                    }
                    else Tool = ToolType.ToolEmpty;
                    trigger.Parent = null;
                    break;
                default:
                    return false;
            }
            return true;
        }

        protected void SpeakToFriend(string speakText)
        {
            //Server.ServerDebug("Recieve text : " + speakText);
            for (int i = 0; i < Communication.Proto.Constants.PlayerCount; i++)
            {
                if (i == CommunicationID.Item2)
                    continue;
                //lock (Program.MessageToClientLock)
                Program.MessageToClient.GameObjectList[Program.PlayerList[new Tuple<int, int>(CommunicationID.Item1, i)].ID].SpeakText = speakText;
            }
        }
    }
}
