using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace AoE2Lib.Utils
{
    public struct Vector2
    {
        public static Vector2 FromPoint(int x, int y)
        {
            return new Vector2(x, y);
        }

        public static Vector2 FromPrecise(int x, int y)
        {
            return new Vector2(x / 100d, y / 100d);
        }

        public readonly double X;
        public readonly double Y;
        public int PointX => (int)Math.Floor(X);
        public int PointY => (int)Math.Floor(Y);
        public int PreciseX => (int)Math.Floor(X * 100);
        public int PreciseY => (int)Math.Floor(Y * 100);

        public Vector2(double x, double y)
        {
            X = x;
            Y = y;
        }

        public static Vector2 operator +(Vector2 a, Vector2 b)
        {
            return new Vector2(a.X + b.X, a.Y + b.Y);
        }

        public static Vector2 operator -(Vector2 a, Vector2 b)
        {
            return new Vector2(a.X - b.X, a.Y - b.Y);
        }

        public static Vector2 operator *(Vector2 v, double a)
        {
            return new Vector2(v.X * a, v.Y * a);
        }

        public static Vector2 operator *(double a, Vector2 v)
        {
            return v * a;
        }

        public static Vector2 operator /(Vector2 v, double a)
        {
            return new Vector2(v.X / a, v.Y / a);
        }

        public static Vector2 operator /(double a, Vector2 v)
        {
            return v / a;
        }

        public double Norm()
        {
            return Math.Sqrt((X * X) + (Y * Y));
        }

        public double DistanceTo(Vector2 other)
        {
            var dx = X - other.X;
            var dy = Y - other.Y;

            return Math.Sqrt((dx * dx) + (dy * dy));
        }
    }
}
