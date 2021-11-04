using Protos.Expert.Fact;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Drawing;
using System.Numerics;
using System.Text;

namespace AoE2Lib.Bots
{
    public struct Position
    {
        public static readonly Position Zero = new Position(0, 0);
        public static readonly Position One = new Position(1, 0);

        public static Position FromPoint(int x, int y)
        {
            return new Position(x, y);
        }

        public static Position FromPrecise(int x, int y)
        {
            return new Position(x / 100d, y / 100d);
        }

        public static Position FromPolar(double angle, double norm)
        {
            return new Position(norm * Math.Cos(angle), norm * Math.Sin(angle));
        }

        public readonly double X;
        public readonly double Y;
        public int PointX => (int)Math.Floor(X);
        public int PointY => (int)Math.Floor(Y);
        public int PreciseX => (int)Math.Floor(X * 100);
        public int PreciseY => (int)Math.Floor(Y * 100);
        public double Norm => DistanceTo(Zero);
        public double Angle => AngleFrom(One);

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

        public static bool operator ==(Position a, Position b)
        {
            return a.PreciseX == b.PreciseX && a.PreciseY == b.PreciseY;
        }

        public static bool operator !=(Position a, Position b)
        {
            return !(a == b);
        }

        public double DistanceTo(Position other)
        {
            var dx = X - other.X;
            var dy = Y - other.Y;

            return Math.Sqrt((dx * dx) + (dy * dy));
        }

        public double AngleFrom(Position other)
        {
            // CCW [0..2*pi]

            var a1 = Math.Atan2(other.Y, other.X);
            var a2 = Math.Atan2(Y, X);

            if (double.IsNaN(a1) || double.IsNaN(a2))
            {
                throw new Exception("Position has NaN");
            }

            var a = a2 - a1;
            while (a < 0)
            {
                a += 2 * Math.PI;
            }

            return a;
        }

        public Position Rotate(double angle)
        {
            // angle CCW

            return FromPolar(Angle + angle, Norm);
        }

        public override int GetHashCode()
        {
            return PreciseX.GetHashCode() ^ PreciseY.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is Position pos)
            {
                return pos == this;
            }
            else
            {
                return false;
            }
        }

        public override string ToString()
        {
            if (PreciseX % 100 == 0 && PreciseY % 100 == 0)
            {
                return $"{X:N0},{Y:N0}";
            }
            else
            {
                return $"{X:N2},{Y:N2}";
            }
        }
    }
}
