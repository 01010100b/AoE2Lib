using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Unary
{
    public abstract class GameElement
    {
        public TimeSpan TimeSinceLastUpdate => DateTime.UtcNow - LastUpdate;
        public DateTime LastUpdate { get; private set; } = DateTime.MinValue;
        public DateTime FirstUpdate { get; private set; } = DateTime.MinValue;
        public int TimesUpdated { get; private set; } = 0;

        internal readonly List<IMessage> Messages = new List<IMessage>();

        public void Update(Dictionary<IMessage, Any> responses)
        {
            if (Messages.Count == 0)
            {
                return;
            }

            UpdateElement(Messages.Select(m => responses[m]).ToList());

            LastUpdate = DateTime.UtcNow;

            if (FirstUpdate == DateTime.MinValue)
            {
                FirstUpdate = DateTime.UtcNow;
            }

            TimesUpdated++;

            Messages.Clear();
        }

        public void RequestUpdate()
        {
            Messages.Clear();
            Messages.AddRange(RequestElementUpdate());
        }

        protected abstract void UpdateElement(List<Any> responses);
        protected abstract IEnumerable<IMessage> RequestElementUpdate();
    }
}
