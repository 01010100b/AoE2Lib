using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace Unary
{
    public abstract class GameElement
    {
        public TimeSpan TimeSinceLastUpdate => DateTime.UtcNow - LastUpdate;
        public DateTime LastUpdate { get; private set; } = DateTime.MinValue;
        public DateTime FirstUpdate { get; private set; } = DateTime.MinValue;
        public int TimesUpdated { get; private set; } = 0;

        public void Update(IEnumerable<IMessage> responses)
        {
            UpdateElement(responses);

            LastUpdate = DateTime.UtcNow;

            if (FirstUpdate == DateTime.MinValue)
            {
                FirstUpdate = DateTime.UtcNow;
            }

            TimesUpdated++;
        }

        protected abstract void UpdateElement(IEnumerable<IMessage> responses);
    }
}
