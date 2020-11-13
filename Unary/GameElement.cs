using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Unary
{
    public abstract class GameElement
    {
        public TimeSpan LastUpdate { get; private set; } = TimeSpan.MinValue;
        public TimeSpan FirstUpdate { get; private set; } = TimeSpan.MinValue;
        public int TimesUpdated { get; private set; } = 0;

        internal readonly Command Command = new Command();

        public void RequestUpdate()
        {
            Command.Messages.Clear();
            Command.Responses.Clear();
            Command.Messages.AddRange(RequestElementUpdate());
        }

        public void Update(TimeSpan gametime)
        {
            if (Command.Responses.Count == 0)
            {
                return;
            }

            Debug.Assert(Command.Responses.Count == Command.Messages.Count);

            UpdateElement(Command.Responses);

            LastUpdate = gametime;

            if (FirstUpdate == TimeSpan.MinValue)
            {
                FirstUpdate = gametime;
            }

            TimesUpdated++;

            Command.Messages.Clear();
            Command.Responses.Clear();
        }

        protected abstract void UpdateElement(List<Any> responses);
        protected abstract IEnumerable<IMessage> RequestElementUpdate();
    }
}
