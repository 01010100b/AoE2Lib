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

        protected readonly Bot Bot;
        private readonly Command Command = new Command();

        protected internal GameElement(Bot bot)
        {
            Bot = bot;
        }

        public void RequestUpdate()
        {
            if (Command.HasMessages)
            {
                Bot.GameState.AddCommand(Command);

                return;
            }

            Command.Reset();

            foreach (var message in RequestElementUpdate())
            {
                Command.Add(message);
            }

            if (Command.HasMessages)
            {
                Bot.GameState.AddCommand(Command);
            }
        }

        internal void Update()
        {
            if (!Command.HasResponses)
            {
                Command.Reset();

                return;
            }

            var responses = Command.GetResponses();
            UpdateElement(responses);

            var gametime = Bot.GameState.GameTime;
            LastUpdateGameTime = gametime;
            if (TimesUpdated == 0)
            {
                FirstUpdateGameTime = gametime;
            }
            TimesUpdated++;
            LastUpdateTick = Bot.GameState.Tick;

            Command.Reset();
        }

        protected abstract IEnumerable<IMessage> RequestElementUpdate();
        protected abstract void UpdateElement(IReadOnlyList<Any> responses);
    }
}
