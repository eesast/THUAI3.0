using System;
using System.Collections.Generic;
using System.Text;

namespace THUnity2D
{
    public class Vector
    {
        public readonly double angle;
        public readonly double length;
        public Vector(double angle = 0, double length = 0)
        {
            this.angle = Tools.CorrectAngle(angle);
            if (double.IsNaN(length))
                length = 0;
            this.length = length;
        }
        public static Vector operator *(Vector vector, double number)
        {
            return new Vector(vector.angle, vector.length * number);
        }
        public static Vector operator *(double number, Vector vector)
        {
            return new Vector(vector.angle, vector.length * number);
        }
    }
}
