﻿using System;
using System.Collections.Generic;
using System.Text;
using THUnity2D;
using static THUnity2D.Tools;

namespace Logic.Constant
{
    public class Character : GameObject
    {
        public double moveSpeed;
        public Direction facingDirection;
        public int MaxThrowDistance;
        public int SightRange;
        public TALENT talent;
        public int score;
        public DishType dish;
        public ToolType tool;
        //public Tuple<int, int> id = new Tuple<int, int>(-1, -1);  //first:Agent, second:Client
        public Character(double x, double y) : base(new XYPosition(x, y))
        {
            Blockable = true;
            Movable = true;
            score = 0;
            dish = DishType.Empty;
            tool = ToolType.Empty;
            moveSpeed = Convert.ToDouble(System.Configuration.ConfigurationManager.AppSettings["PlayerInitMoveSpeed"]);
            MaxThrowDistance = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["PlayerInitThrowDistance"]);
            SightRange = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["PlayerInitSightRange"]);
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
