using AoE2Lib.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace AoE2Lib.Bots
{
    public class Unit
    {
        public enum Order
        {
            NONE = -1, ATTACK, DEFEND, BUILD, HEAL, CONVERT, EXPLORE, STOP,
            RUNAWAY, RETREAT, GATHER, MOVE, PATROL, FOLLOW, HUNT, TRANSPORT,
            TRADE, EVADE, ENTER, REPAIR, TRAIN, RESEARCH, UNLOAD, RELIC = 31
        }

        public TimeSpan TimeSinceLastUpdate => DateTime.UtcNow - LastUpdate;
        public DateTime LastUpdate { get; private set; } = DateTime.MinValue;

        public int Id { get; private set; } = -1; // 4000
        public Position Position { get; private set; } = new Position(-1, -1); // 500 x 500
        public int PlayerNumber { get; private set; } = -1; // 10
        public int TypeId { get; private set; } = -1; // 2000
        public int Hitpoints { get; private set; } = -1; // 1000
        public Order CurrentOrder { get; private set; } = Order.NONE; // 40
        public int TargetId { get; private set; } = -1; // 4000
    }
}
