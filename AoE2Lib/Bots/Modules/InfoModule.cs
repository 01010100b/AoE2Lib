using AoE2Lib.Utils;
using Protos.Expert.Fact;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace AoE2Lib.Bots.Modules
{
    public class InfoModule : Module
    {
        public TimeSpan GameTime { get; private set; } = TimeSpan.Zero;

        private readonly Command Command = new Command();

        protected internal override IEnumerable<Command> RequestUpdate()
        {
            Command.Reset();
            Command.Add(new GameTime());

            yield return Command;
        }

        protected internal override void Update()
        {
            var responses = Command.GetResponses();
            if (responses.Count > 0)
            {
                GameTime = TimeSpan.FromSeconds(responses[0].Unpack<GameTimeResult>().Result);
                Log.Write($"game-time {GameTime}");
            }

            Command.Reset();
        }
    }
}
