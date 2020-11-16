using AoE2Lib.Utils;
using Protos.Expert.Action;
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
        public Position MyPosition { get; private set; }
        public double GameSecondsPerTick { get; private set; } = 1;

        private readonly Command Command = new Command();

        protected override IEnumerable<Command> RequestUpdate()
        {
            Command.Reset();
            Command.Add(new GameTime());
            Command.Add(new UpGetPoint() { GoalPoint = 50, PositionType = (int)PositionType.SELF });
            Command.Add(new Goal() { GoalId = 50 });
            Command.Add(new Goal() { GoalId = 51 });

            yield return Command;
        }

        protected override void Update()
        {
            var responses = Command.GetResponses();
            if (responses.Count > 0)
            {
                var current_time = GameTime;

                GameTime = TimeSpan.FromSeconds(responses[0].Unpack<GameTimeResult>().Result);
                var x = responses[2].Unpack<GoalResult>().Result;
                var y = responses[3].Unpack<GoalResult>().Result;
                MyPosition = new Position(x, y);

                GameSecondsPerTick *= 99;
                GameSecondsPerTick += (GameTime - current_time).TotalSeconds;
                GameSecondsPerTick /= 100;
            }

            Command.Reset();
        }
    }
}
