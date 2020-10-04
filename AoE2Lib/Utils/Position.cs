using System;
using System.Collections.Generic;
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

        public double DistanceTo(Position other)
        {
            var dx = (double)(X - other.X);
            var dy = (double)(Y - other.Y);

            return Math.Sqrt((dx * dx) + (dy * dy));
        }
    }
}
