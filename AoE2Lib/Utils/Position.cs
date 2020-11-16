using System;
using System.Collections.Generic;
using System.Text;

namespace AoE2Lib.Utils
{
    public struct Position
    {
        public static Position FromPoint(int x, int y)
        {
            return new Position(x, y);
        }

        public static Position FromPrecise(int x, int y)
        {
            return new Position(x / 100d, y / 100d);
        }

        public readonly double X;
        public readonly double Y;
        public int PointX => (int)Math.Floor(X);
        public int PointY => (int)Math.Floor(Y);
        public int PreciseX => (int)Math.Floor(X * 100);
        public int PreciseY => (int)Math.Floor(Y * 100);

        public Position(double x, double y)
        {
            X = x;
            Y = y;
        }

        public double DistanceTo(Position other)
        {
            var dx = X - other.X;
            var dy = Y - other.Y;

            return Math.Sqrt((dx * dx) + (dy * dy));
        }
    }
}
