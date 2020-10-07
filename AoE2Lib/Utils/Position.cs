using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace AoE2Lib.Utils
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
