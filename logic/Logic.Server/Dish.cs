using System;
using System.Collections.Generic;
using System.Text;
using static THUnity2D.Tools;
using static Logic.Constant.MapInfo;
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
                            Layer = (int)MapLayer.ItemLayer;
                            foreach (Block block in WorldMap.Grid[(int)Position.x, (int)Position.y].GetType(typeof(Block)))
                            {
                                if (block.blockType == BlockType.RubbishBin) Parent = null;
                            }
                        });
                return _stopMovingTimer;
            }
        }

        public Dish(double x_t, double y_t, DishType type_t) : base(x_t, y_t)
        {
            Server.ServerDebug("Create Dish : " + type_t);
            Layer = (int)MapLayer.ItemLayer;
            Movable = true;
            Bouncable = true;
            _dish = type_t;
            lock (Program.MessageToClientLock)
            {
                Program.MessageToClient.GameObjectMessageList.Add(
                    this.ID,
                    new GameObjectMessage
                    {
                        ObjType = (ObjTypeMessage)ObjType.Dish,
                        DishType = (DishTypeMessage)Dish,
                        Position = new XYPositionMessage { X = Position.x, Y = Position.y }
                    });
                Server.ServerDebug("Add Dish to Message list : " + type_t);
            }
            this.MoveComplete += new MoveCompleteHandler(
                (thisGameObject) =>
                {
                    lock (Program.MessageToClientLock)
                    {
                        Program.MessageToClient.GameObjectMessageList[thisGameObject.ID].Position.X = thisGameObject.Position.x;
                        Program.MessageToClient.GameObjectMessageList[thisGameObject.ID].Position.Y = thisGameObject.Position.y;
                    }
                    //Server.ServerDebug(this.Position.ToString());
                });
            this.OnParentDelete += new ParentDeleteHandler(
                () =>
                {
                    lock (Program.MessageToClientLock)
                    {
                        Program.MessageToClient.GameObjectMessageList.Remove(ID);
                    }
                });
        }

        public override DishType GetDish(DishType t)
        {
            DishType temp = Dish;
            if (t == DishType.Empty) this.Parent = null;
            else
            {
                Dish = t;
            }
            return temp;
        }
    }
}
