using System;
using System.Collections.Generic;
using System.Text;

namespace THUnity2D
{
    public class XYPosition
    {
        public readonly double x;
        public readonly double y;
        public XYPosition(double x_t = 0, double y_t = 0)
        {
            x = x_t;
            y = y_t;
        }
        public static XYPosition operator +(XYPosition a, XYPosition b)
        {
            return new XYPosition(a.x + b.x, a.y + b.y);
        }
        public static XYPosition operator -(XYPosition a, XYPosition b)
        {
            return new XYPosition(a.x - b.x, a.y - b.y);
        }
        public static XYPosition operator *(XYPosition a, double b)
        {
            return new XYPosition(a.x * b, a.y * b);
        }
        public static XYPosition operator *(double b, XYPosition a)
        {
            return new XYPosition(a.x * b, a.y * b);
        }
        public double get(bool flag)
        {
            if (flag)
                return y;
            else
                return x;
        }
        public static double Distance(XYPosition position1, XYPosition position2)
        {
            return Math.Sqrt(Math.Pow(Math.Abs(position1.x - position2.x), 2) + Math.Pow(Math.Abs(position1.y - position2.y), 2));
        }
        public static double ManhattanDistance(XYPosition position1, XYPosition position2)
        {
            return Math.Abs(position1.x - position2.x) + Math.Abs(position1.y - position2.y);
        }
        public override string ToString()
        {
            return "(" + x.ToString() + "," + y.ToString() + ")";
        }
    }
}
