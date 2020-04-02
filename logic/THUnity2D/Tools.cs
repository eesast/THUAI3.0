using System;
using System.Collections.Generic;

namespace THUnity2D
{
    public enum Direction
    {
        Right = 0,
        RightUp,
        Up,
        LeftUp,
        Left,
        LeftDown,
        Down,
        RightDown,
        Size
    }

    public static class Tools
    {
        //长度为1的向量。
        public static readonly Dictionary<Direction, XYPosition> EightUnitVector = new Dictionary<Direction, XYPosition> {
            { Direction.Right, new XYPosition (1, 0) },
            { Direction.RightUp, new XYPosition(1 / Math.Sqrt(2),1 / Math.Sqrt(2)) },
            { Direction.Up, new XYPosition(0, 1) },
            { Direction.LeftUp, new XYPosition(-1 / Math.Sqrt(2),1 / Math.Sqrt(2)) },
            { Direction.Left, new XYPosition(-1, 0) },
            { Direction.LeftDown, new XYPosition(-1 / Math.Sqrt(2),-1 / Math.Sqrt(2)) },
            { Direction.Down, new XYPosition(0, -1) },
            { Direction.RightDown, new XYPosition(1 / Math.Sqrt(2),-1 / Math.Sqrt(2)) }
        };
        //指向八个角的向量
        public static readonly Dictionary<Direction, XYPosition> EightCornerVector = new Dictionary<Direction, XYPosition> {
            { Direction.Right, new XYPosition (0.5, 0) },
            { Direction.RightUp, new XYPosition(0.5,0.5) },
            { Direction.Up, new XYPosition(0, 0.5) },
            { Direction.LeftUp, new XYPosition(-0.5,0.5) },
            { Direction.Left, new XYPosition(-0.5, 0) },
            { Direction.LeftDown,new XYPosition(-0.5,-0.5) },
            { Direction.Down, new XYPosition(0, -0.5) },
            { Direction.RightDown, new XYPosition(0.5,-0.5) }
        };

        public static double CorrectAngle(double angle)
        {
            if (double.IsNaN(angle))
            {
                angle = 0;
                return angle;
            }
            while (angle < 0)
                angle += 2 * Math.PI;
            while (angle >= 2 * Math.PI)
                angle -= 2 * Math.PI;
            return angle;
        }

        public static double DivisionWithoutNaN(double d1, double d2)
        {
            if (d1 == 0)
                return 0;
            return d1 / d2;
        }

        public static bool IsInOpenInterval(double toCheckNumber, double IntervalA, double IntervalB)
        {
            if (IntervalA < IntervalB)
            {
                return toCheckNumber > IntervalA && toCheckNumber < IntervalB;
            }
            else if (IntervalA > IntervalB)
            {
                return toCheckNumber > IntervalB && toCheckNumber < IntervalA;
            }
            return false;
        }

        public static bool IsInCloseInterval(double toCheckNumber, double IntervalA, double IntervalB)
        {
            if (IntervalA <= IntervalB)
            {
                return toCheckNumber >= IntervalA && toCheckNumber <= IntervalB;
            }
            else if (IntervalA >= IntervalB)
            {
                return toCheckNumber >= IntervalB && toCheckNumber <= IntervalA;
            }
            return false;
        }

        public static bool IsInOpenCloseInterval(double toCheckNumber, double IntervalA, double IntervalB)
        {
            if (IntervalA < IntervalB)
            {
                return toCheckNumber > IntervalA && toCheckNumber <= IntervalB;
            }
            else if (IntervalA > IntervalB)
            {
                return toCheckNumber >= IntervalB && toCheckNumber < IntervalA;
            }
            return false;
        }

        public static bool IsInCloseOpenInterval(double toCheckNumber, double IntervalA, double IntervalB)
        {
            if (IntervalA < IntervalB)
            {
                return toCheckNumber >= IntervalA && toCheckNumber < IntervalB;
            }
            else if (IntervalA > IntervalB)
            {
                return toCheckNumber > IntervalB && toCheckNumber <= IntervalA;
            }
            return false;
        }
    }
}
