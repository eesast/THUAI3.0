using System;
using System.Collections.Generic;
using System.Text;
using Logic.Constant;

namespace Logic.Server
{
    public class Trigger : Obj
    {
        public TriggerType trigger;
        public Trigger(double x_t, double y_t, TriggerType type_t) : base(x_t, y_t)
        {
            Blockable = false;
            Movable = false;
            trigger = type_t;
        }
    }
}
