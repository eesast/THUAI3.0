using System;
using System.Collections.Generic;
using System.Text;

namespace THUnity2D
{
    public class XYPosition
    {
        public double x { get; set; }
        public double y { get; set; }
        public XYPosition(double x_t = 0, double y_t = 0)
        {
            x = x_t;
            y = y_t;
        }
        public static XYPosition operator +(XYPosition a, XYPosition b)
        {
            XYPosition result = new XYPosition();
            result.x = a.x + b.x;
            result.y = a.y + b.y;
            return result;
        }
        public static XYPosition operator -(XYPosition a, XYPosition b)
        {
            XYPosition result = new XYPosition();
            result.x = a.x - b.x;
            result.y = a.y - b.y;
            return result;
        }
        public static XYPosition operator *(XYPosition a, double b)
        {
            XYPosition result = new XYPosition();
            result.x = a.x * b;
            result.y = a.y * b;
            return result;
        }
        public static XYPosition operator *(double b, XYPosition a)
        {
            XYPosition result = new XYPosition();
            result.x = a.x * b;
            result.y = a.y * b;
            return result;
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
