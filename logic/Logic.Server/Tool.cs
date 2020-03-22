using Logic.Constant;
using static Logic.Constant.MapInfo;
using Communication.Proto;

namespace Logic.Server
{
    public class Tool : Obj
    {

        public Tool(double x_t, double y_t, ToolType type_t) : base(x_t, y_t, ObjType.Tool)
        {
            Server.ServerDebug("Create Tool : " + type_t);
            Layer = ItemLayer;
            Movable = true;
            Bouncable = true;

            AddToMessage();
            Tool = type_t;

            this.MoveComplete += new MoveCompleteHandler(ChangePositionInMessage);
            this.OnParentDelete += new ParentDeleteHandler(DeleteFromMessage);

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

    }
}
