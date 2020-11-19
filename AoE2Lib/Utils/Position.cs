﻿using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Numerics;
using System.Text;

namespace AoE2Lib.Utils
{
    public struct Position
    {
        public static readonly Position Zero = new Position(0, 0);

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
        public double Norm => Math.Sqrt((X * X) + (Y * Y));
        public double Angle => AngleFrom(Zero);

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

        public double DistanceTo(Position other)
        {
            var dx = X - other.X;
            var dy = Y - other.Y;

            return Math.Sqrt((dx * dx) + (dy * dy));
        }

        public double AngleFrom(Position other)
        {
            var a1 = Math.Atan2(other.Y, other.X);
            var a2 = Math.Atan2(Y, X);

            if (a1 < 0)
            {
                a1 += 2 * Math.PI;
            }
            if (a2 < 0)
            {
                a2 += 2 * Math.PI;
            }

            var a = a2 - a1;
            if (a < 0)
            {
                a += 2 * Math.PI;
            }

            return a;
        }
    }
}