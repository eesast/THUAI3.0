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
        //public int team = 0;
        //protected double GlueExtraMoveSpeed = 0;
        //protected double SpeedBuffExtraMoveSpeed = 0;
        protected double _moveSpeed = (double)Configs("Player", "InitMoveSpeed");
        protected double MoveSpeed { get => _moveSpeed; }
        protected new THUnity2D.Direction _facingDirection;
        public new THUnity2D.Direction FacingDirection { get => _facingDirection; }
        protected int StrenthBuffThrowDistance = 0;
        protected int MaxThrowDistance = (int)Configs("Player", "InitThrowDistance");
        protected int _sightRange = (int)Configs("Player", "InitSightRange");
        public int SightRange { get => _sightRange; }
        protected Talent _talent = Talent.None;

        //protected int _score = 0;

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
        public virtual void Pick(bool isSelfPosition, ObjType pickType, int dishOrToolType)
        { }
        public virtual void Use(int type, double parameter_1, double parameter_2)
        { }
    }
}
