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

        public Tuple<int, int> CommunicationID
        {
            get => _communicationID;
            set => Program.MessageToClient.GameObjectList[this.ID].Team = (_communicationID = value).Item1;
        }


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
            get => _dish;
            private set => Program.MessageToClient.GameObjectList[this.ID].DishType = _dish = value;
        }

        protected ToolType Tool
        {
            get { return _tool; }
            private set
            {
                DeFunction(_tool);
                _tool = value;
                Function(_tool);
                Program.MessageToClient.GameObjectList[this.ID].ToolType = _tool;
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
                    case Talent.Runner: MoveSpeed += (double)Configs("Talent", "Runner", "ExtraMoveSpeed"); break;
                    case Talent.StrongMan: MaxThrowDistance += (int)Configs("Talent", "StrongMan", "ExtraThrowDistance"); break;
                    case Talent.LuckyBoy:
                        LuckTalentTimer = new System.Threading.Timer(Item, null, 30000, (int)Configs("Talent", "LuckyBoy", "RefreshTime"));
                        void Item(object? i)
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
        protected int MaxThrowDistance
        {
            get => _maxThrowDistance;
            private set => Program.MessageToClient.GameObjectList[this.ID].MaxThrowDistance = _maxThrowDistance = value;

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
            SpeedBuffTimer = new System.Threading.Timer((i) => { MoveSpeed -= (double)Configs("Tool", "SpeedBuff", "ExtraMoveSpeed"); });
            StrengthBuffTimer = new System.Threading.Timer((i) => { MaxThrowDistance -= (int)Configs("Tool", "StrengthBuff", "ExtraThrowDistance"); });
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
                        MaxThrowDistance = this.MaxThrowDistance,
                        DishType = Dish,
                        ToolType = Tool,
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
                        MoveSpeed += (double)Configs("Trigger", "WaveGlue", "ExtraMoveSpeed");
                        Velocity = new Vector(Velocity.angle, MoveSpeed);
                        isStepOnGlue = true;
                    }
                    else if (isStepOnGlue && !tempIsStepOnGlue)
                    {
                        MoveSpeed -= (double)Configs("Trigger", "WaveGlue", "ExtraMoveSpeed");
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
                    Pick(msg.IsPickSelfPosition, msg.PickType, msg.PickDishOrToolType);
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
                    SpeakToFriend(msg.SpeakText.Length > 16 ? msg.SpeakText.Substring(0, 16) : msg.SpeakText);
                    break;
                default:
                    break;
            }
        }

        public void Move(THUnity2D.Direction direction)
        {
            this.FacingDirection = direction;
            Move((int)direction * Math.PI / 4, MoveSpeed / Constant.Constant.FrameRate);
        }

        public override void Move(THUnity2D.Direction direction, int durationMilliseconds)
        {
            this.FacingDirection = direction;
            this.status = CommandType.Move;
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
            Program.MessageToClient.GameObjectList[thisGameObject.ID].PositionX = thisGameObject.Position.x;
            Program.MessageToClient.GameObjectList[thisGameObject.ID].PositionY = thisGameObject.Position.y;
            Program.MessageToClient.GameObjectList[thisGameObject.ID].Direction = (Direction)((Player)thisGameObject).FacingDirection;
        }

        //isSelfPosition参数表示是不是捡起自己所在方格的物品
        //pickType表示捡起类型
        //dishOrToolType表示捡起的Dish或Tool类型，-1为随缘
        public override void Pick(bool isSelfPosition, ObjType pickType, int dishOrToolType)
        {
            XYPosition toCheckPosition = isSelfPosition ? Position : Position + 2 * EightCornerVector[FacingDirection];

            switch (pickType)
            {
                case ObjType.Block:
                    var block = WorldMap.Grid[(int)toCheckPosition.x, (int)toCheckPosition.y].GetFirstObject(typeof(FoodPoint));
                    if (block == null)
                    {
                        block = WorldMap.Grid[(int)toCheckPosition.x, (int)toCheckPosition.y].GetFirstObject(typeof(Cooker));
                        if (block != null)
                        {
                            if (((Cooker)block).ProtectedTeam != CommunicationID.Item1 && ((Cooker)block).ProtectedTeam >= 0)
                                block = null;
                        }
                    }
                    if (block != null && ((Block)block).Dish != DishType.DishEmpty)
                    {
                        DishType temp = Dish;
                        Dish = ((Block)block).GetDish(Dish);
                        if (temp != DishType.DishEmpty)
                        {
                            new Dish(Position.x, Position.y, temp).Parent = WorldMap;
                        }
                        Server.ServerDebug(this + " Get Dish " + Dish + " from Block");
                        break;
                    }
                    break;
                case ObjType.Dish:
                    if (dishOrToolType == -1)
                    {
                        Dish? dishToPick = (Dish?)WorldMap.Grid[(int)toCheckPosition.x, (int)toCheckPosition.y].GetFirstObject(typeof(Dish));
                        if (dishToPick != null)
                        {
                            Dish = dishToPick.GetDish(Dish);
                            Server.ServerDebug(this + " Get " + dishToPick);
                        }
                        break;
                    }
                    if (dishOrToolType <= (int)DishType.DishEmpty || dishOrToolType >= (int)DishType.DishSize3
                        || dishOrToolType == (int)DishType.DishSize1 || dishOrToolType == (int)DishType.DishSize2)
                        break;
                    DishType toPickDishType = (DishType)dishOrToolType;
                    var dishlist = WorldMap.Grid[(int)toCheckPosition.x, (int)toCheckPosition.y].GetObjects(typeof(Dish));
                    if (dishlist != null)
                    {
                        foreach (Dish dish in dishlist)
                        {
                            if (dish.Dish == toPickDishType)
                            {
                                Dish = dish.GetDish(Dish);
                                Server.ServerDebug(this + " Get " + dish);
                                break;
                            }
                        }
                    }
                    break;
                case ObjType.Tool:
                    if (dishOrToolType == -1)
                    {
                        Tool? toolToPick = (Tool?)WorldMap.Grid[(int)toCheckPosition.x, (int)toCheckPosition.y].GetFirstObject(typeof(Tool));
                        if (toolToPick != null)
                        {
                            Tool = toolToPick.GetTool(Tool);
                            Server.ServerDebug(this + " Get " + toolToPick);
                        }
                        break;
                    }
                    if (dishOrToolType <= (int)ToolType.ToolEmpty || dishOrToolType >= (int)ToolType.ToolSize)
                        break;
                    ToolType toPickToolType = (ToolType)dishOrToolType;
                    var toolist = WorldMap.Grid[(int)toCheckPosition.x, (int)toCheckPosition.y].GetObjects(typeof(Tool));
                    if (toolist != null)
                    {
                        foreach (Tool tool in toolist)
                        {
                            if (tool.Tool == toPickToolType)
                            {
                                Tool = tool.GetTool(Tool);
                                Server.ServerDebug(this + " Get " + tool);
                                break;
                            }
                        }
                    }
                    break;
            }

            //Server.ServerDebug("没东西捡");
            status = CommandType.Stop;
            Velocity = new Vector(0, 0);
        }
        public override void Put(double distance, double angle, bool isThrowDish)
        {
            distance = distance < MaxThrowDistance ? distance : MaxThrowDistance;
            int dueTime = (int)((double)1000 * distance / (double)Configs("ItemMoveSpeed")) - (int)HalfTimeIntervalInMillisecond;//注意到如果直接用 distance / ItemMoveSpeed 会在最后多走一步，所以必须做一些数值处理

            Obj ItemToThrow;
            if (Dish != DishType.DishEmpty && isThrowDish)
            {
                Server.ServerDebug(this + " throw Dish " + Dish + " distance:" + distance + " , angle:" + angle);
                ItemToThrow = new Dish(Position.x, Position.y, Dish);
                Dish = DishType.DishEmpty;
            }
            else if (Tool != ToolType.ToolEmpty && !isThrowDish)
            {
                Server.ServerDebug(this + " throw Tool " + Tool + " distance:" + distance + " , angle:" + angle);
                ItemToThrow = new Tool(Position.x, Position.y, Tool);
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
                ItemToThrow.Velocity = new Vector(angle, (int)Configs("ItemMoveSpeed"));
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
                var cooker = WorldMap.Grid[(int)xyPosition1.x, (int)xyPosition1.y].GetFirstObject(typeof(Cooker));
                if (cooker != null && ((Cooker)cooker).isCooking == false)
                {
                    Server.ServerDebug(this + " use cooker at " + cooker.Position);
                    ((Cooker)cooker).UseCooker(CommunicationID.Item1, Talent);
                }
                else
                {
                    var taskPoint = WorldMap.Grid[(int)xyPosition1.x, (int)xyPosition1.y].GetFirstObject(typeof(TaskPoint));
                    if (taskPoint != null)
                    {
                        Server.ServerDebug(this + " hand in task: " + Dish);
                        lock (Program.ScoreLocks[CommunicationID.Item1])
                            Score += ((TaskPoint)taskPoint).HandIn(Dish);
                        Dish = DishType.DishEmpty;
                    }
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
                case ToolType.TigerShoes: MoveSpeed += (double)Configs("Tool", "TigerShoe", "ExtraMoveSpeed"); break;
                case ToolType.TeleScope: SightRange += (int)Configs("Tool", "TeleScope", "ExtraSightRange"); break;
            }
        }
        public void DeFunction(ToolType type)//在丢弃装备时生效
        {
            switch (type)
            {
                case ToolType.TigerShoes: MoveSpeed -= (double)Configs("Tool", "TigerShoe", "ExtraMoveSpeed"); break;
                case ToolType.TeleScope: SightRange -= (int)Configs("Tool", "TeleScope", "ExtraSightRange"); break;
            }
        }

        public void GetTalent(Talent t)
        {
            Talent = t;
        }

        public void UseTool(double parameter1, double parameter2)
        {
            switch (Tool)
            {
                case ToolType.TigerShoes:
                case ToolType.TeleScope:
                case ToolType.BreastPlate:
                case ToolType.ToolEmpty: Server.ServerDebug("物品使用失败（为空或无需使用）！"); break;
                case ToolType.Condiment:
                    {
                        XYPosition xyPosition1 = Position + 2 * EightCornerVector[FacingDirection];
                        TaskPoint? taskPoint = (TaskPoint?)WorldMap.Grid[(int)xyPosition1.x, (int)xyPosition1.y].GetFirstObject(typeof(TaskPoint));
                        if (taskPoint != null)
                        {
                            int temp = taskPoint.HandIn(Dish);
                            if (temp > 0)
                            {
                                Server.ServerDebug(this + " use Condiment to hand in");
                                lock (Program.ScoreLocks[CommunicationID.Item1])
                                    Score += (int)(temp * (1 + (double)((Talent == Talent.Cook) ?
                                                                        Configs("Talent", "Cook", "Condiment", "ScoreParameter") : Configs("Tool", "Condiment", "ScoreParameter"))));
                                Dish = DishType.DishEmpty;
                            }
                            else Server.ServerDebug("物品使用失败（提交任务失败）！");
                        }
                        Tool = ToolType.ToolEmpty;
                    }
                    break;
                case ToolType.SpeedBuff:
                    {
                        Server.ServerDebug(this + " use Tool " + Tool);
                        MoveSpeed += (double)Configs("Tool", "SpeedBuff", "ExtraMoveSpeed");
                        if (Talent != Talent.Technician) SpeedBuffTimer.Change((int)Configs("Tool", "SpeedBuff", "Duration"), 0);
                        else SpeedBuffTimer.Change((int)Configs("Talent", "Technician", "SpeedBuff", "Duration"), 0);
                        if (Velocity.length > 0)
                            Velocity = new Vector(Velocity.angle, MoveSpeed);
                        Tool = ToolType.ToolEmpty;
                    }
                    break;
                case ToolType.StrengthBuff:
                    {
                        Server.ServerDebug(this + " use Tool " + Tool);
                        MaxThrowDistance += (int)Configs("Tool", "StrengthBuff", "ExtraThrowDistance");
                        if (Talent != Talent.Technician) StrengthBuffTimer.Change((int)Configs("Tool", "StrengthBuff", "Duration"), 0);
                        else StrengthBuffTimer.Change((int)Configs("Talent", "Technician", "StrengthBuff", "Duration"), 0);
                        Tool = ToolType.ToolEmpty;
                    }
                    break;
                case ToolType.Fertilizer:
                    {
                        Server.ServerDebug(this + " use Tool " + Tool);
                        XYPosition xyPosition1 = Position.GetMid() + 2 * EightCornerVector[FacingDirection];
                        FoodPoint? foodPoint = (FoodPoint?)WorldMap.Grid[(int)xyPosition1.x, (int)xyPosition1.y].GetFirstObject(typeof(FoodPoint));
                        if (foodPoint != null)
                            foodPoint.RefreshTime /= 2;
                        Tool = ToolType.ToolEmpty;
                    }
                    break;
                case ToolType.WaveGlueBottle:
                    {
                        Server.ServerDebug(this + " use Tool " + Tool);
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
                        Server.ServerDebug(this + " use Tool " + Tool);
                        new Trigger(xyPosition1.x, xyPosition1.y, TriggerType.Mine, CommunicationID.Item1, Talent).Parent = WorldMap;
                        Tool = ToolType.ToolEmpty;
                    }
                    break;
                case ToolType.TrapTool:
                    {
                        XYPosition xyPosition1 = Position.GetMid();
                        if (WorldMap.Grid[(int)xyPosition1.x, (int)xyPosition1.y].ContainsType(typeof(Block)))
                        { Server.ServerDebug("物品使用失败（无效的陷阱放置地点）！"); break; }
                        Server.ServerDebug(this + " use Tool " + Tool);
                        new Trigger(xyPosition1.x, xyPosition1.y, TriggerType.Trap, CommunicationID.Item1, Talent).Parent = WorldMap;
                        Tool = ToolType.ToolEmpty;
                    }
                    break;
                case ToolType.SpaceGate:
                    {
                        //parameter1是距离，paameter2是角度
                        double maxDistance = (Talent == Talent.Technician) ? (double)Configs("Talent", "Technician", "SpaceGate", "MaxDistance") : (double)Configs("Tool", "SpaceGate", "MaxDistance");
                        if (parameter1 < 0) parameter1 = 0;
                        else if (parameter1 > maxDistance) parameter1 = maxDistance;
                        XYPosition aim = Position + new XYPosition(parameter1 * Math.Cos(parameter2), parameter1 * Math.Sin(parameter2));
                        if (!WorldMap.XYPositionIsLegal(aim, 1, 1, Layer))
                        { Server.ServerDebug("物品使用失败（无效的传送门地点）！"); break; }
                        Server.ServerDebug(this + " use Tool " + Tool);
                        Tool = ToolType.ToolEmpty;
                    }
                    break;
                case ToolType.FlashBomb:
                    {
                        XYPosition xyPosition1 = Position.GetMid();
                        if (WorldMap.Grid[(int)xyPosition1.x, (int)xyPosition1.y].ContainsType(typeof(Block)))
                        { Server.ServerDebug("物品使用失败（无效的炸弹放置地点）！"); break; }
                        Server.ServerDebug(this + " use Tool " + Tool);
                        new Trigger(xyPosition1.x, xyPosition1.y, TriggerType.Bomb, CommunicationID.Item1, Talent).Parent = WorldMap;
                        Tool = ToolType.ToolEmpty;
                    }
                    break;
                case ToolType.ThrowHammer:
                    {
                        //parameter1是距离，paameter2是角度
                        double maxDistance = (Talent == Talent.StrongMan) ? (double)Configs("Talent", "StrongMan", "ThrowHammer", "MaxDistance") : (double)Configs("Tool", "ThrowHammer", "MaxDistance");
                        if (parameter1 < 0) parameter1 = 0;
                        else if (parameter1 > maxDistance) parameter1 = maxDistance;
                        int dueTime = (int)((double)1000 * parameter1 / (double)Configs("ItemMoveSpeed")) - (int)HalfTimeIntervalInMillisecond;
                        Server.ServerDebug(this + " use Tool " + Tool + " distance:" + parameter1 + " angle:" + parameter2);
                        Trigger triggerToThrow = new Trigger(Position.x, Position.y, TriggerType.Hammer, CommunicationID.Item1, Talent);
                        triggerToThrow.Parent = WorldMap;
                        if (dueTime > 0)
                        {
                            triggerToThrow.Velocity = new Vector(parameter2, (int)Configs("ItemMoveSpeed"));
                            triggerToThrow.StopMovingTimer.Change(dueTime, 0);
                        }
                        Tool = ToolType.ToolEmpty;
                    }
                    break;
                case ToolType.Bow:
                    {
                        //parameter1是距离，parameter2是角度
                        double maxDistance = (Talent == Talent.StrongMan) ? (double)Configs(nameof(Talent), Talent.ToString(), Tool.ToString(), "MaxDistance") : (double)Configs(nameof(Tool), Tool.ToString(), "MaxDistance");
                        if (parameter1 < 0) parameter1 = 0;
                        else if (parameter1 > maxDistance) parameter1 = maxDistance;
                        int dueTime = (int)((double)1000 * parameter1 / (double)Configs("Trigger", "Arrow", "Speed")) - (int)HalfTimeIntervalInMillisecond;
                        Server.ServerDebug(this + " use Tool " + Tool + " distance:" + parameter1 + " angle:" + parameter2);
                        Trigger triggerToThrow = new Trigger(Position.x, Position.y, TriggerType.Arrow, CommunicationID.Item1, Talent);
                        triggerToThrow.Parent = WorldMap;
                        if (dueTime > 0)
                        {
                            triggerToThrow.Velocity = new Vector(parameter2, (int)Configs("Trigger", "Arrow", "Speed"));
                            triggerToThrow.StopMovingTimer.Change(dueTime, 0);
                        }
                        Tool = ToolType.ToolEmpty;
                    }
                    break;
                case ToolType.Stealer:
                    {
                        XYPosition xyPositionToSteal = Position + 2 * EightCornerVector[FacingDirection];
                        Player? playerToSteal = (Player?)WorldMap.Grid[(int)xyPositionToSteal.x, (int)xyPositionToSteal.y].GetFirstObject(typeof(Player));
                        if (playerToSteal != null && playerToSteal.Dish != DishType.DishEmpty)
                        {
                            Server.ServerDebug(this + " use Tool " + Tool);
                            if (Dish != DishType.DishEmpty)
                                new Dish(Position.x, Position.y, Dish).Parent = WorldMap;
                            Dish = playerToSteal.Dish;
                            playerToSteal.Dish = DishType.DishEmpty;
                            Tool = ToolType.ToolEmpty;
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
                            new Dish(Position.x + 3 * Math.Cos(Program.Random.NextDouble() * 2 * Math.PI), Position.y + 3 * Math.Sin(Program.Random.NextDouble() * 2 * Math.PI), Dish).Parent = WorldMap;
                            Dish = DishType.DishEmpty;
                        }
                        if (Tool != ToolType.ToolEmpty)
                        {
                            new Tool(Position.x + 3 * Math.Cos(Program.Random.NextDouble() * 2 * Math.PI), Position.y + 3 * Math.Sin(Program.Random.NextDouble() * 2 * Math.PI), Tool).Parent = WorldMap;
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
                            new Dish(Position.x + 1 * Math.Cos(Program.Random.NextDouble() * 2 * Math.PI), Position.y + 1 * Math.Sin(Program.Random.NextDouble() * 2 * Math.PI), Dish).Parent = WorldMap;
                            Dish = DishType.DishEmpty;
                        }
                        if (Tool != ToolType.ToolEmpty)
                        {
                            new Tool(Position.x + 1 * Math.Cos(Program.Random.NextDouble() * 2 * Math.PI), Position.y + 1 * Math.Sin(Program.Random.NextDouble() * 2 * Math.PI), Tool).Parent = WorldMap;
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
                Program.MessageToClient.GameObjectList[Program.PlayerList[new Tuple<int, int>(CommunicationID.Item1, i)].ID].RecieveText = speakText;
            }
        }

        public override string ToString()
        {
            return "Player:" + ID + ", " + CommunicationID.Item1 + "." + CommunicationID.Item2 + ", " + Position.ToString() + " ";
        }
    }
}
