using System;
using THUnity2D;
using static Logic.Constant.Constant;
using static Logic.Constant.MapInfo;
using static THUnity2D.Tools;
using Communication;
using Communication.Proto;

namespace Logic.Constant
{
    public class Character : THUnity2D.GameObject
    {
        public Tuple<int, int> CommunicationID = new Tuple<int, int>(0, 0);//第一个数表示Agent，第二个数表示Client
        public int team = 0;
        public double GlueExtraMoveSpeed = 0;
        public double SpeedBuffExtraMoveSpeed = 0;
        public double moveSpeed = (double)Configs["PlayerInitMoveSpeed"];
        public double MoveSpeed { get { return moveSpeed + GlueExtraMoveSpeed + SpeedBuffExtraMoveSpeed; } }
        public THUnity2D.Direction facingDirection;
        public int StrenthBuffThrowDistance = 0;
        public int MaxThrowDistance = (int)Configs["PlayerInitThrowDistance"];
        public int SightRange = (int)(Configs["PlayerInitSightRange"]);
        protected Talent _talent = Talent.None;

        protected int _score = 0;

        public DishType dish = DishType.DishEmpty;
        public ToolType tool = ToolType.ToolEmpty;
        public Character(double x, double y) : base(new THUnity2D.XYPosition(x, y))
        {
            Layer = PlayerLayer;
            Movable = true;
        }
        public virtual void Move(THUnity2D.Direction direction_t, int duration = 50)
        { }
        public virtual void Put(double distance, double angle, bool isThrowDish)
        { }
        public virtual void Pick()
        { }
        public virtual void Use(int type, int parameter_1, int parameter_2)
        { }
    }
}
