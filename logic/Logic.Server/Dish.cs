using System;
using System.Collections.Generic;
using System.Text;
using static THUnity2D.Tools;
using Logic.Constant;

namespace Logic.Server
{
    public class Dish : Obj //包括食材和做好的菜
    {

        public int distance;
        public Direction direction;
        public TimeSpan LastActTime;

        public Dish(double x_t, double y_t, DishType type_t) : base(x_t, y_t)
        {
            Blockable = false;
            Movable = false;
            dish = type_t;
        }
        public override DishType GetDish(DishType t)
        {
            DishType temp = dish;
            if (t == DishType.Empty) this.Parent = null;
            else dish = t;
            return temp;
        }
    }
}
