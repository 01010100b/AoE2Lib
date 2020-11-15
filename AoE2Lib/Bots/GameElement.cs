using AoE2Lib.Bots.Modules;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace AoE2Lib.Bots
{
    public abstract class GameElement
    {
        public TimeSpan LastUpdate { get; private set; } = TimeSpan.MinValue;
        public TimeSpan FirstUpdate { get; private set; } = TimeSpan.MinValue;
        public int TimesUpdated { get; private set; } = 0;

        protected internal readonly Bot Bot;
        protected internal readonly Command Command = new Command();

        protected internal GameElement(Bot bot)
        {
            Bot = bot;
        }

        public void RequestUpdate()
        {
            Command.Messages.Clear();
            Command.Responses.Clear();

            Command.Messages.AddRange(RequestElementUpdate());
        }

        internal void Update()
        {
            if (Command.Responses.Count == 0)
            {
                return;
            }

            Debug.Assert(Command.Responses.Count == Command.Messages.Count);

            UpdateElement(Command.Responses);

            var gametime = Bot.GetModule<InfoModule>().GameTime;

            LastUpdate = gametime;

            if (FirstUpdate == TimeSpan.MinValue)
            {
                FirstUpdate = gametime;
            }

            TimesUpdated++;

            Command.Messages.Clear();
            Command.Responses.Clear();
        }

        protected abstract IEnumerable<IMessage> RequestElementUpdate();
        protected abstract void UpdateElement(IReadOnlyList<Any> responses);
    }
}
