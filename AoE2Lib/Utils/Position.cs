using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Numerics;
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

        public static Position operator +(Position a, Position b)
        {
            return new Position(a.X + b.X, a.Y + b.Y);
        }

        public static Position operator -(Position a, Position b)
        {
            return new Position(a.X - b.X, a.Y - b.Y);
        }

        public static Position operator *(Position v, double a)
        {
            return new Position(v.X * a, v.Y * a);
        }

        public static Position operator *(double a, Position v)
        {
            return v * a;
        }

        public static Position operator /(Position v, double a)
        {
            return new Position(v.X / a, v.Y / a);
        }

        public static Position operator /(double a, Position v)
        {
            return v / a;
        }

        public double Norm()
        {
            return Math.Sqrt((X * X) + (Y * Y));
        }

        public double DistanceTo(Position other)
        {
            var dx = X - other.X;
            var dy = Y - other.Y;

            return Math.Sqrt((dx * dx) + (dy * dy));
        }
    }
}
