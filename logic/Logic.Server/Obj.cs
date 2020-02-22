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
        public ObjType objType;
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

        public Obj(double x_t, double y_t, ObjType objType) : base(new XYPosition(x_t, y_t))
        {
            this.objType = objType;
        }
        public virtual DishType GetDish(DishType t) { return DishType.Empty; }
        public virtual ToolType GetTool(ToolType t) { return ToolType.Empty; }
        public virtual void UseCooker() { }
        public virtual int HandIn(DishType dish_t) { return 0; }
        public void AddToMessage()
        {
            lock (Program.MessageToClientLock)
            {
                Program.MessageToClient.GameObjectMessageList.Add(
                    this.ID,
                    new GameObjectMessage
                    {
                        ObjType = (ObjTypeMessage)objType,
                        Position = new XYPositionMessage { X = Position.x, Y = Position.y }
                    });
            }
        }
    }
}
