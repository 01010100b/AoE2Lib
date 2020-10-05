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
        public int UnitId { get; private set; } = -1; // 4000
        public int Elevation { get; private set; } = -1; // 64
        public int TerrainId { get; private set; } = -1; // 64
        public bool Explored { get; private set; } = false; // 2

        public void Update(int goal0, int goal1)
        {
            var data = goal0;

            var y = (data % 500) - 1;
            data /= 500;
            var x = (data % 500) - 1;
            Position = new Position(x, y);

            data = goal1;

            UnitId = (data % 4000) - 1;
            data /= 4000;
            Elevation = (data % 64) - 1;
            data /= 64;
            TerrainId = (data % 64) - 1;
            data /= 64;
            Explored = (data % 2) == 1;
        }
    }
}
