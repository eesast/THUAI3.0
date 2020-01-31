using System;
using System.Collections.Generic;
using System.Text;
using THUnity2D;
using Logic.Constant;

namespace Logic.Server
{
    public class Obj : GameObject
    {
        public ObjType type;
        public DishType dish;
        public ToolType tool;
        public BlockType blockType;

        public Obj(double x_t, double y_t) : base(new XYPosition(x_t, y_t))
        {
        }
        public virtual DishType GetDish(DishType t) { return DishType.Empty; }
        public virtual ToolType GetTool(ToolType t) { return ToolType.Empty; }
        public virtual void UseCooker() { }
        public virtual int HandIn(DishType dish_t) { return 0; }
    }
}
