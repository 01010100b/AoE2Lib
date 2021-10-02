using System;
using System.Collections.Generic;
using System.Text;

namespace AoE2Lib.Bots.GameElements
{
    public class Tile
    {
        public readonly int X;
        public readonly int Y;
        public int Height { get; internal set; }
        public Position Position => Position.FromPoint(X, Y);
        public bool IsOnLand => Terrain != 1 && Terrain != 2 && Terrain != 4 && Terrain != 15 && Terrain != 22 && Terrain != 23 && Terrain != 28 && Terrain != 37;
        public bool Explored => Visibility != 0;
        public bool Visible => Visibility == 15;
        public readonly List<Unit> Units = new List<Unit>();

        internal int Terrain { get; set; }
        internal int Visibility { get; set; }

        internal Tile(int x, int y)
        {
            X = x;
            Y = y;
        }
    }
}
