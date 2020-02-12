using System;
using System.Collections.Generic;
using System.Text;
using static THUnity2D.Tools;
using Logic.Constant;
using Communication.Proto;

namespace Logic.Server
{
    public class Dish : Obj //包括食材和做好的菜
    {

        public int distance;
        public Direction direction;
        //public TimeSpan LastActTime;
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
                        });
                return _stopMovingTimer;
            }
        }
        public Dish(double x_t, double y_t, DishType type_t) : base(x_t, y_t)
        {
            Layer = (int)Logic.Constant.Map.MapLayer.ItemLayer;
            Movable = true;
            Bouncable = true;
            dish = type_t;
            lock (Program.MessageToClientLock)
            {
                Program.MessageToClient.GameObjectMessageList.Add(
                    this.ID,
                    new GameObjectMessage
                    {
                        ObjType = (ObjTypeMessage)ObjType.Dish,
                        DishType = (DishTypeMessage)dish,
                        Position = new XYPositionMessage { X = Position.x, Y = Position.y }
                    });
            }
            this.MoveComplete += new MoveCompleteHandler(
                (thisGameObject) =>
                {
                    lock (Program.MessageToClientLock)
                    {
                        Program.MessageToClient.GameObjectMessageList[thisGameObject.ID].Position.X = thisGameObject.Position.x;
                        Program.MessageToClient.GameObjectMessageList[thisGameObject.ID].Position.Y = thisGameObject.Position.y;
                        Program.MessageToClient.GameObjectMessageList[thisGameObject.ID].Direction = (DirectionMessage)((Player)thisGameObject).facingDirection;
                    }
                });
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
