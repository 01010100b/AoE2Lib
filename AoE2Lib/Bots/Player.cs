using AoE2Lib.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace AoE2Lib.Bots
{
    public class Player
    {
        public TimeSpan TimeSinceLastUpdate => DateTime.UtcNow - LastUpdate;
        public DateTime LastUpdate { get; internal set; } = DateTime.MinValue;

        public int PlayerNumber { get; internal set; } = -1;
        public int Civilization { get; internal set; } = -1;
        public int Score { get; internal set; } = -1;
        public int Age { get; internal set; } = -1;
        public int CivilianPopulation { get; internal set; } = -1;
        public int MilitaryPopulation { get; internal set; } = -1;
        public int Stance { get; internal set; } = -1;
        public Position Position { get; internal set; } = new Position(-1, -1);
    }
}
