using System;
using THUnity2D;
using static Logic.Constant.Constant;
using static Logic.Constant.MapInfo;
using static THUnity2D.Tools;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq; 

namespace Logic.Constant
{
    public class Character : GameObject
    {
        public Tuple<int, int> CommunicationID = new Tuple<int, int>(0, 0);//第一个数表示Agent，第二个数表示Client
        public int team = 0;
        public double GlueExtraMoveSpeed = 0;
        public double moveSpeed = (double)Configs["PlayerInitMoveSpeed"];
        public Direction facingDirection;
        public int MaxThrowDistance = (int)Configs["PlayerInitThrowDistance"];
        public int SightRange = (int)(Configs["PlayerInitSightRange"]);
        public TALENT talent;
        protected int score = 0;
        public DishType dish = DishType.Banana;
        public ToolType tool = ToolType.Trap;
        public Character(double x, double y) : base(new XYPosition(x, y))
        {
            Layer = (int)MapLayer.PlayerLayer;
            Movable = true;
        }
        public virtual void Move(Direction direction_t, int duration = 50)
        { }
        public virtual void Put(double distance, bool isThrowDish)
        { }
        public virtual void Pick()
        { }
        public virtual void Use(int type, int parameter)
        { }
    }
}
