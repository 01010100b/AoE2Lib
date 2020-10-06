using System;
using System.Collections.Generic;
using System.Text;

namespace AoE2Lib.Bots
{
    public abstract class GameElement
    {
        public TimeSpan TimeSinceLastUpdate => DateTime.UtcNow - LastUpdate;
        public DateTime LastUpdate { get; private set; } = DateTime.MinValue;
        public DateTime FirstUpdate { get; private set; } = DateTime.MinValue;
        public int TimesUpdated { get; private set; } = 0;

        protected void ElementUpdated()
        {
            LastUpdate = DateTime.UtcNow;

            if (FirstUpdate == DateTime.MinValue)
            {
                FirstUpdate = DateTime.UtcNow;
            }

            TimesUpdated++;
        }
    }
}
