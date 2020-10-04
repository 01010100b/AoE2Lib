using AoE2Lib.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace AoE2Lib.Bots
{
    public class Tile
    {
        public TimeSpan TimeSinceLastUpdate => DateTime.UtcNow - LastUpdate;
        public DateTime LastUpdate { get; internal set; } = DateTime.MinValue;

        public Position Position { get; internal set; } = new Position(-1, -1);
        public int Elevation { get; internal set; } = -1;
        public int TerrainId { get; internal set; } = -1;
        public bool Explored { get; internal set; } = false;
    }
}
