using System;
using System.Collections.Generic;
using System.Text;
using THUnity2D;
using static THUnity2D.Tools;
using static Logic.Constant.MapInfo;

namespace Logic.Constant
{
    public class Character : GameObject
    {
        public int team = 0;
        public double GlueExtraMoveSpeed = 0;
        public double moveSpeed = Convert.ToDouble(System.Configuration.ConfigurationManager.AppSettings["PlayerInitMoveSpeed"]);
        public Direction facingDirection;
        public int MaxThrowDistance = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["PlayerInitThrowDistance"]);
        public int SightRange = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["PlayerInitSightRange"]);
        public TALENT talent;
        protected int _score = 0;
        public DishType dish = DishType.Banana;
        public ToolType tool = ToolType.Fertilizer;
        //public Tuple<int, int> id = new Tuple<int, int>(-1, -1);  //first:Agent, second:Client
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
