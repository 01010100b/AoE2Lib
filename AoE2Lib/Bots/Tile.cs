using AoE2Lib.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Text;

namespace AoE2Lib.Bots
{
    public class Tile
    {
        public TimeSpan TimeSinceLastUpdate => DateTime.UtcNow - LastUpdate;
        public DateTime LastUpdate { get; private set; } = DateTime.MinValue;

        public Position Position { get; private set; } = new Position(-1, -1); // 500 x 500
        public int UnitId { get; private set; } = -1; // 45000
        public int Elevation { get; private set; } = -1; // 64
        public int TerrainId { get; private set; } = -1; // 64
        public bool Explored { get; private set; } = false; // 2

        public void Update(int goal0, int goal1)
        {
            var x = (goal0 % 500) - 1;
            goal0 /= 500;
            var y = (goal0 % 500) - 1;
            var position = new Position(x, y);

            if (position != Position)
            {
                throw new ArgumentException("Incorrect tile position: " + position);
            }

            UnitId = (goal1 % 45000) - 1;
            goal1 /= 45000;
            Elevation = (goal1 % 64) - 1;
            goal1 /= 64;
            TerrainId = (goal1 % 64) - 1;
            goal1 /= 64;
            Explored = (goal1 % 2) == 1;

            LastUpdate = DateTime.UtcNow;
        }
    }
}
