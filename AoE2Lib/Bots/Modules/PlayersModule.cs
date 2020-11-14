using Protos.Expert.Action;
using Protos.Expert.Fact;
using System;
using System.Collections.Generic;
using System.Text;

namespace AoE2Lib.Bots.Modules
{
    public class PlayersModule : Module
    {
        public class Player
        {
            public readonly int PlayerNumber;
            public bool InGame { get; internal set; } = false;
            public int Civilization { get; internal set; } = -1;
            public int Score { get; internal set; } = -1;
            public int Age { get; internal set; } = -1;
            public int Population => CivilianPopulation + MilitaryPopulation;
            public int CivilianPopulation { get; internal set; } = -1;
            public int MilitaryPopulation { get; internal set; } = -1;
            public int WoodAmount { get; internal set; } = -1;
            public int FoodAmount { get; internal set; } = -1;
            public int GoldAmount { get; internal set; } = -1;
            public int StoneAmount { get; internal set; } = -1;
            public PlayerStance Stance { get; internal set; } = PlayerStance.NEUTRAL;

            internal readonly Command Command = new Command();

            internal Player(int player)
            {
                PlayerNumber = player;
            }
        }

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
                var command = player.Command;
                command.Reset();

                command.Add(new PlayerInGame() { PlayerNumber = player.PlayerNumber });
                command.Add(new UpGetPlayerFact() { Player = player.PlayerNumber, FactId = (int)FactId.CIVILIZATION, Param = 0, GoalData = 100 });
                command.Add(new Goal() { GoalId = 100 });
                command.Add(new UpGetPlayerFact() { Player = player.PlayerNumber, FactId = (int)FactId.CURRENT_SCORE, Param = 0, GoalData = 100 });
                command.Add(new Goal() { GoalId = 100 });
                command.Add(new UpGetPlayerFact() { Player = player.PlayerNumber, FactId = (int)FactId.CURRENT_AGE, Param = 0, GoalData = 100 });
                command.Add(new Goal() { GoalId = 100 });
                command.Add(new UpGetPlayerFact() { Player = player.PlayerNumber, FactId = (int)FactId.CIVILIAN_POPULATION, Param = 0, GoalData = 100 });
                command.Add(new Goal() { GoalId = 100 });
                command.Add(new UpGetPlayerFact() { Player = player.PlayerNumber, FactId = (int)FactId.MILITARY_POPULATION, Param = 0, GoalData = 100 });
                command.Add(new Goal() { GoalId = 100 });
                command.Add(new UpGetPlayerFact() { Player = player.PlayerNumber, FactId = (int)FactId.WOOD_AMOUNT, Param = 0, GoalData = 100 });
                command.Add(new Goal() { GoalId = 100 });
                command.Add(new UpGetPlayerFact() { Player = player.PlayerNumber, FactId = (int)FactId.FOOD_AMOUNT, Param = 0, GoalData = 100 });
                command.Add(new Goal() { GoalId = 100 });
                command.Add(new UpGetPlayerFact() { Player = player.PlayerNumber, FactId = (int)FactId.GOLD_AMOUNT, Param = 0, GoalData = 100 });
                command.Add(new Goal() { GoalId = 100 });
                command.Add(new UpGetPlayerFact() { Player = player.PlayerNumber, FactId = (int)FactId.STONE_AMOUNT, Param = 0, GoalData = 100 });
                command.Add(new Goal() { GoalId = 100 });
                command.Add(new StanceToward() { PlayerNumber = player.PlayerNumber, Stance = (int)PlayerStance.ALLY });
                command.Add(new StanceToward() { PlayerNumber = player.PlayerNumber, Stance = (int)PlayerStance.ENEMY });
                command.Add(new StanceToward() { PlayerNumber = player.PlayerNumber, Stance = (int)PlayerStance.NEUTRAL });

                yield return command;
            }
        }

        protected internal override void Update()
        {
            foreach (var player in Players.Values)
            {
                var resp = player.Command.GetResponses();
                
                if (resp.Count > 0)
                {
                    player.InGame = resp[0].Unpack<PlayerInGameResult>().Result;
                    player.Civilization = resp[2].Unpack<GoalResult>().Result;
                    player.Score = resp[4].Unpack<GoalResult>().Result;
                    player.Age = resp[6].Unpack<GoalResult>().Result;
                    player.CivilianPopulation = resp[8].Unpack<GoalResult>().Result;
                    player.MilitaryPopulation = resp[10].Unpack<GoalResult>().Result;
                    player.WoodAmount = resp[12].Unpack<GoalResult>().Result;
                    player.FoodAmount = resp[14].Unpack<GoalResult>().Result;
                    player.GoldAmount = resp[16].Unpack<GoalResult>().Result;
                    player.StoneAmount = resp[18].Unpack<GoalResult>().Result;

                    if (resp[19].Unpack<StanceTowardResult>().Result)
                    {
                        player.Stance = PlayerStance.ALLY;
                    }
                    else if (resp[20].Unpack<StanceTowardResult>().Result)
                    {
                        player.Stance = PlayerStance.ENEMY;
                    }
                    else if (resp[21].Unpack<StanceTowardResult>().Result)
                    {
                        player.Stance = PlayerStance.NEUTRAL;
                    }
                }
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
                        _Players.Add(player, new Player(player));
                    }
                }
            }
        }
    }
}
