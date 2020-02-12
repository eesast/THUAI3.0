using System;
using System.Collections.Generic;
using System.Text;
using Logic.Constant;
using static THUnity2D._Map;

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
                            this.Layer = (int)MapLayer.ItemLayer;
                        });
                return _stopMovingTimer;
            }
        }

        public Tool(double x_t, double y_t, ToolType type_t) : base(x_t, y_t)
        {
            Layer = (int)MapLayer.ItemLayer;
            Movable = true;
            Bouncable = true;
            tool = type_t;
        }
        public override ToolType GetTool(ToolType t)
        {
            ToolType temp = tool;
            if (t == ToolType.Empty) this.Parent = null;
            else tool = t;
            return temp;
        }

    }
}
