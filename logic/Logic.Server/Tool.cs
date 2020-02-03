using System;
using System.Collections.Generic;
using System.Text;
using Logic.Constant;

namespace Logic.Server
{
    public class Tool : Obj
    {
        public Tool(double x_t, double y_t, ToolType type_t) : base(x_t, y_t)
        {
            Blockable = false;
            Movable = false;
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
