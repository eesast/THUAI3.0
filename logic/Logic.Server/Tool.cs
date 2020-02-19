using System;
using System.Collections.Generic;
using System.Text;
using Logic.Constant;
using static Logic.Constant.MapInfo;
using Communication.Proto;

namespace Logic.Server
{
    public class Tool : Obj
    {
        protected System.Threading.Timer _stopMovingTimer = null;
        public System.Threading.Timer StopMovingTimer
        {
            get
            {
                if (_stopMovingTimer == null)
                    _stopMovingTimer = new System.Threading.Timer(
                        (o) =>
                        {
                            Velocity = new THUnity2D.Vector(Velocity.angle, 0);
                            Layer = (int)MapLayer.ItemLayer;
                        });
                return _stopMovingTimer;
            }
        }

        public Tool(double x_t, double y_t, ToolType type_t) : base(x_t, y_t, ObjType.Tools)
        {
            Server.ServerDebug("Create Tool : " + type_t);
            Layer = (int)MapLayer.ItemLayer;
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
            if (t == ToolType.Empty) this.Parent = null;
            else
            {
                Tool = t;
            }
            return temp;
        }

    }
}
