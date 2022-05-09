using AoE2Lib.Bots;
using AoE2Lib.Games;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Unary.Learning
{
    internal class Scenario
    {
        public string Name { get; set; }
        public double PerfectScore { get; set; }
        public Civilization Civilization { get; set; }
        public bool HighResources { get; set; }
        public bool MapExplored { get; set; }
        public int TimeLimit { get; set; }
        public string OpponentAiFile { get; set; }

        public Game CreateGame(string my_ai_file)
        {
            var game = new Game()
            {
                GameType = GameType.SCENARIO,
                ScenarioName = Name,
                MapType = MapType.RANDOM_MAP,
                MapSize = MapSize.TINY,
                Difficulty = Difficulty.HARD,
                StartingResources = HighResources ? StartingResources.HIGH : StartingResources.STANDARD,
                PopulationLimit = 200,
                RevealMap = MapExplored ? RevealMap.EXPLORED : RevealMap.NORMAL,
                StartingAge = StartingAge.STANDARD,
                VictoryType = TimeLimit > 0 ? VictoryType.TIME_LIMIT : VictoryType.CONQUEST,
                VictoryValue = TimeLimit,
                TeamsTogether = true,
                LockTeams = true,
                AllTechs = false,
                Recorded = false
            };

            var me = new Player()
            {
                PlayerNumber = 1,
                IsHuman = false,
                AiFile = my_ai_file,
                Civilization = (int)Civilization,
                Color = Color.COLOR_1,
                Team = Team.NO_TEAM
            };

            var opponent = new Player()
            {
                PlayerNumber = 2,
                IsHuman = false,
                AiFile = OpponentAiFile,
                Civilization = (int)Civilization,
                Color = Color.COLOR_2,
                Team = Team.NO_TEAM
            };

            game.AddPlayer(me);
            game.AddPlayer(opponent);

            return game;
        }

        public double GetScore(Game game)
        {
            if (!game.Finished)
            {
                throw new ArgumentException("Game is not finished.");
            }

            var my_score = 0;
            var opponent_score = 0;

            foreach (var player in game.GetPlayers())
            {
                if (player.PlayerNumber == 1)
                {
                    my_score = player.Score;
                }
                else if (player.PlayerNumber == 2)
                {
                    opponent_score = player.Score;
                }
            }

            return (my_score - opponent_score) / PerfectScore;
        }

        public override string ToString() => $"{Name} against {OpponentAiFile}";
    }
}
