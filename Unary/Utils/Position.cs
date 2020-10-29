using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Drawing;
using System.Text;

namespace Unary.Utils
{
    public struct Position
    {
        public readonly int X;
        public readonly int Y;

        public Position(int x, int y)
        {
            X = x;
            Y = y;
        }

        public bool OnMap(int map_size)
        {
            return (X >= 0) && (X < map_size) && (Y >= 0) && (Y < map_size);
        }

        public double DistanceTo(Position other)
        {
            var dx = (double)(X - other.X);
            var dy = (double)(Y - other.Y);

            return Math.Sqrt((dx * dx) + (dy * dy));
        }

        public Point ProjectOnLine(Point a, Point b)
        {
            // get dot product of e1, e2
            Point e1 = new Point(b.X - a.X, b.Y - a.Y);
            Point e2 = new Point(X - a.X, Y - a.Y);
            double dp = (e1.X * e2.X) + (e1.Y * e2.Y);
            // get squared length of e1
            double len2 = e1.X * e1.X + e1.Y * e1.Y;
            Point p = new Point((int)(a.X + (dp * e1.X) / len2), (int)(a.Y + (dp * e1.Y) / len2));

            return p;
        }

        public override bool Equals(object obj)
        {
            if (obj is Position pos)
            {
                return X == pos.X && Y == pos.Y;
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return X + "," + Y;
        }

        public static bool operator ==(Position a, Position b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(Position a, Position b)
        {
            return !a.Equals(b);
        }
    }
}
