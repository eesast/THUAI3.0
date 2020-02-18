using System;
using System.Collections.Generic;
using System.Text;
using THUnity2D;
using Logic.Constant;
using Communication.Proto;

namespace Logic.Server
{
    public class Obj : GameObject
    {
        public ObjType type;
        protected DishType _dish;
        public DishType Dish
        {
            get { return _dish; }
            set
            {
                _dish = value;
                lock (Program.MessageToClientLock)
                {
                    Program.MessageToClient.GameObjectMessageList[ID].DishType = (DishTypeMessage)_dish;
                }
            }
        }
        protected ToolType _tool;
        public ToolType Tool
        {
            get { return _tool; }
            set
            {
                _tool = value;
                lock (Program.MessageToClientLock)
                {
                    Program.MessageToClient.GameObjectMessageList[ID].ToolType = (ToolTypeMessage)_tool;
                }
            }
        }

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
