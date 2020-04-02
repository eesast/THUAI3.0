using Communication.Proto;
using Logic.Constant;
using THUnity2D;
using static Logic.Constant.MapInfo;

namespace Logic.Server
{
    public class Obj : THUnity2D.GameObject
    {
        public ObjType objType;
        protected DishType _dish;
        public DishType Dish
        {
            get { return _dish; }
            set
            {
                _dish = value;
                //lock (Program.MessageToClientLock)
                Program.MessageToClient.GameObjectList[ID].DishType = _dish;
            }
        }
        protected ToolType _tool;
        public ToolType Tool
        {
            get { return _tool; }
            set
            {
                _tool = value;
                //lock (Program.MessageToClientLock)
                Program.MessageToClient.GameObjectList[ID].ToolType = _tool;
            }
        }

        public delegate void StopMovingHandler(object o);
        public event StopMovingHandler? StopMoving = null;
        protected void StopMovingMethod(object o)
        {
            Velocity = new THUnity2D.Vector(Velocity.angle, 0);
            StopMoving?.Invoke(o);
        }
        protected System.Threading.Timer _stopMovingTimer = null;
        public System.Threading.Timer StopMovingTimer
        {
            get
            {
                if (_stopMovingTimer == null)
                    _stopMovingTimer = new System.Threading.Timer(StopMovingMethod);
                return _stopMovingTimer;
            }
        }

        public Obj(double x_t, double y_t, ObjType objType) : base(new THUnity2D.XYPosition(x_t, y_t))
        {
            this.objType = objType;
        }
        public virtual DishType GetDish(DishType t) { return DishType.DishEmpty; }
        public virtual ToolType GetTool(ToolType t) { return ToolType.ToolEmpty; }
        public virtual void UseCooker(int TeamNumber, Talent t) { }
        public virtual int HandIn(DishType dish_t) { return 0; }
        protected void AddToMessage()
        {
            lock (Program.MessageToClientLock)
            {
                Program.MessageToClient.GameObjectList.Add(
                    this.ID,
                    new Communication.Proto.GameObject
                    {
                        ObjType = objType,
                        PositionX = Position.x,
                        PositionY = Position.y
                    });
            }
        }
        protected void DeleteFromMessage()
        {
            lock (Program.MessageToClientLock)
            {
                Program.MessageToClient.GameObjectList.Remove(ID);
                //Server.ServerDebug("Delete " + objType + " From Message List");
            }
        }
        protected void ChangePositionInMessage(THUnity2D.GameObject thisGameObject)
        {
            //lock (Program.MessageToClientLock)
            //{
            Program.MessageToClient.GameObjectList[thisGameObject.ID].PositionX = thisGameObject.Position.x;
            Program.MessageToClient.GameObjectList[thisGameObject.ID].PositionY = thisGameObject.Position.y;
            //}
        }
    }
}
