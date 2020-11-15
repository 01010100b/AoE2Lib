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
            Command.Reset();

            foreach (var message in RequestElementUpdate())
            {
                Command.Add(message);
            }
        }

        public void Update()
        {
            var responses = Command.GetResponses();

            if (responses.Count == 0)
            {
                return;
            }

            UpdateElement(responses);

            var gametime = Bot.GetModule<InfoModule>().GameTime;

            LastUpdate = gametime;

            if (FirstUpdate == TimeSpan.MinValue)
            {
                FirstUpdate = gametime;
            }

            TimesUpdated++;

            Command.Reset();
        }

        protected abstract IEnumerable<IMessage> RequestElementUpdate();
        protected abstract void UpdateElement(IReadOnlyList<Any> responses);
    }
}
