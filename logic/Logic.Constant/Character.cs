using System;
using System.Collections.Generic;
using System.Text;
using THUnity2D;
using static THUnity2D.Tools;

namespace Logic.Constant
{
    public class Character : GameObject
    {
        public double moveSpeed = Convert.ToDouble(System.Configuration.ConfigurationManager.AppSettings["PlayerInitMoveSpeed"]);
        public Direction facingDirection;
        public int MaxThrowDistance = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["PlayerInitThrowDistance"]);
        public int SightRange = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["PlayerInitSightRange"]);
        public TALENT talent;
        public int score = 0;
        public DishType dish = DishType.Empty;
        public ToolType tool = ToolType.Empty;
        //public Tuple<int, int> id = new Tuple<int, int>(-1, -1);  //first:Agent, second:Client
        public Character(double x, double y) : base(new XYPosition(x, y))
        {
            Blockable = true;
            Movable = true;
            //score = 0;
            //dish = DishType.Empty;
            //tool = ToolType.Empty;
            //moveSpeed = Convert.ToDouble(System.Configuration.ConfigurationManager.AppSettings["PlayerInitMoveSpeed"]);
            //MaxThrowDistance = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["PlayerInitThrowDistance"]);
            //SightRange = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["PlayerInitSightRange"]);
        }
        public virtual void Move(Direction direction_t, int duration = 50)
        { }
        public virtual void Put(int distance, int ThrowDish)
        { }
        public virtual void Pick()
        { }
        public virtual void Use(int type, int parameter)
        { }
    }
}
