using AoE2Lib.Bots.GameElements;
using AoE2Lib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Unary.Utils
{
    class Region
    {
        public Position Position => GetPosition();
        public readonly HashSet<Tile> Tiles = new HashSet<Tile>();
        public TimeSpan LastScouted { get; set; } = TimeSpan.MinValue;
        public TimeSpan LastAccessFailure { get; set; } = TimeSpan.MinValue;
        public double ExploredFraction => Tiles.Count(t => t.Explored) / (double)Tiles.Count;

        private Position GetPosition()
        {
            if (Tiles.Count == 0)
            {
                return new Position(-1, -1);
            }

            var pos = Position.Zero;
            foreach (var tile in Tiles)
            {
                pos += tile.Position;
            }

            pos /= Tiles.Count;

            return pos;
        }
    }
}
