using System;
using System.Collections.Generic;
using System.Text;

namespace AoE2Lib.Bots
{
    public class GameElement
    {
        public TimeSpan TimeSinceLastUpdate => DateTime.UtcNow - LastUpdate;
        public DateTime LastUpdate { get; protected set; } = DateTime.MinValue;

        protected void ElementUpdated()
        {
            LastUpdate = DateTime.UtcNow;
        }
    }
}
