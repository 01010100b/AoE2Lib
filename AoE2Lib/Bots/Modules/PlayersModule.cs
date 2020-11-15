﻿using AoE2Lib.Bots.GameElements;
using Protos.Expert.Action;
using Protos.Expert.Fact;
using System;
using System.Collections.Generic;
using System.Text;

namespace AoE2Lib.Bots.Modules
{
    public class PlayersModule : Module
    {
        public IReadOnlyDictionary<int, Player> Players => _Players;
        private Dictionary<int, Player> _Players = new Dictionary<int, Player>();

        private readonly Command Command = new Command();

        protected internal override IEnumerable<Command> RequestUpdate()
        {
            Command.Reset();

            for (int i = 1; i <= 8; i++)
            {
                Command.Add(new PlayerValid() { PlayerNumber = i });
            }

            yield return Command;

            foreach (var player in Players.Values)
            {
                player.RequestUpdate();

                yield return player.Command;
            }
        }

        protected internal override void Update()
        {
            foreach (var player in Players.Values)
            {
                player.Update();
            }

            var responses = Command.GetResponses();
            for (int i = 0; i < responses.Count; i++)
            {
                var valid = responses[i].Unpack<PlayerValidResult>().Result;

                if (valid)
                {
                    var player = i + 1;

                    if (!Players.ContainsKey(player))
                    {
                        _Players.Add(player, new Player(Bot, player));
                    }
                }
            }
        }
    }
}
