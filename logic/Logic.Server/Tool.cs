using Logic.Constant;
using static Logic.Constant.MapInfo;
using Communication.Proto;

namespace Logic.Server
{
    public class Tool : Obj
    {

        public Tool(double x_t, double y_t, ToolType type_t) : base(x_t, y_t, ObjType.Tool)
        {
            Layer = ItemLayer;
            Movable = true;
            Bouncable = true;

            AddToMessage();
            Tool = type_t;
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
        public override ToolType GetTool(ToolType t)
        {
            ToolType temp = Tool;
            if (t == ToolType.ToolEmpty) this.Parent = null;
            else
            {
                Tool = t;
            }
            return temp;
        }

        public override string ToString()
        {
            return objType + ":" + Tool + ", " + ID + ", " + Position.ToString() + " ";
        }

    }
}
