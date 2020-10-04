using System;
using System.Collections.Generic;
using System.Text;

namespace AoE2Lib.Utils
{
    public struct Point
    {
        public readonly int X;
        public readonly int Y;

        public Point(int x, int y)
        {
            X = x;
            Y = y;
        }

        public double DistanceTo(Point other)
        {
            var dx = (double)(X - other.X);
            var dy = (double)(Y - other.Y);

            return Math.Sqrt((dx * dx) + (dy * dy));
        }
    }
}
