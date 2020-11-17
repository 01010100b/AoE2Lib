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
        public bool Updated => TimesUpdated > 0;
        public TimeSpan LastUpdateGameTime { get; private set; } = TimeSpan.MinValue;
        public TimeSpan FirstUpdateGameTime { get; private set; } = TimeSpan.MinValue;
        public int TimesUpdated { get; private set; } = 0;
        public int LastUpdateTick { get; private set; } = -1;

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

            LastUpdateGameTime = gametime;

            if (FirstUpdateGameTime == TimeSpan.MinValue)
            {
                FirstUpdateGameTime = gametime;
            }

            TimesUpdated++;
            LastUpdateTick = Bot.Tick;

            Command.Reset();
        }

        protected abstract IEnumerable<IMessage> RequestElementUpdate();
        protected abstract void UpdateElement(IReadOnlyList<Any> responses);
    }
}
