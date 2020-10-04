using AoE2Lib.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace AoE2Lib.Bots
{
    public class Unit
    {
        public TimeSpan TimeSinceLastUpdate => DateTime.UtcNow - LastUpdate;
        public DateTime LastUpdate { get; internal set; } = DateTime.MinValue;

        public int Id { get; internal set; } = -1;
        public int PlayerNumber { get; internal set; } = -1;
        public int TypeId { get; internal set; } = -1;
        public Position Position { get; internal set; } = new Position(-1, -1);
    }
}
