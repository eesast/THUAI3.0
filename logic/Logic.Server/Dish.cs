using Logic.Constant;
using static Logic.Constant.MapInfo;
using static THUnity2D.Tools;
using Communication.Proto;
using Direction = Communication.Proto.Direction;

namespace Logic.Server
{
    public class Dish : Obj //包括食材和做好的菜
    {

        public int distance;
        public Direction direction;

        public Dish(double x_t, double y_t, DishType type_t) : base(x_t, y_t, ObjType.Dish)
        {
            Layer = ItemLayer;
            Movable = true;
            Bouncable = true;
            AddToMessage();
            Dish = type_t;
            this.StopMoving += new StopMovingHandler((o) =>
            {
                Server.ServerDebug(this + "has stopped moving");
                Layer = ItemLayer;
                if (WorldMap.Grid[(int)Position.x, (int)Position.y].ContainsType(typeof(RubbishBin)))
                {
                    Parent = null;
                }
            });
            MoveComplete += ChangePositionInMessage;
            PositionChangeComplete += ChangePositionInMessage;
            OnParentDelete += DeleteFromMessage;
            OnParentDelete += () =>
            {
                MoveComplete -= ChangePositionInMessage;
                PositionChangeComplete -= ChangePositionInMessage;

            };
            Server.ServerDebug("Create " + this);
        }

        public override DishType GetDish(DishType t)
        {
            DishType temp = Dish;
            if (t == DishType.DishEmpty) this.Parent = null;
            else
            {
                Dish = t;
            }
            return temp;
        }

        public override string ToString()
        {
            return objType + ":" + Dish + ", " + ID + ", " + Position.ToString() + " ";
        }
    }
}
