using AoE2Lib.Games;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unary.Optimization
{
    internal class Scenario
    {
        public static List<Scenario> GetDefaultScenarios()
        {
            var scenarios = new List<Scenario>()
            {
                new Scenario()
                {
                    ScenarioName = "TRM range 4v4 no ballistics",
                    PerfectScore = 250,
                    Civilization = Civilization.FRANKS
                },
                new Scenario()
                {
                    ScenarioName = "TRM range 5v5 no ballistics",
                    PerfectScore = 250,
                    Civilization = Civilization.FRANKS
                },
                new Scenario()
                {
                    ScenarioName = "TRM range 6v6 no ballistics",
                    PerfectScore = 250,
                    Civilization = Civilization.BRITONS
                },
                new Scenario()
                {
                    ScenarioName = "TRM range 7v7 ballistics",
                    PerfectScore = 250,
                    Civilization = Civilization.FRANKS
                },
                new Scenario()
                {
                    ScenarioName = "TRM range 8v8 ballistics",
                    PerfectScore = 250,
                    Civilization = Civilization.BYZANTINES
                },
                new Scenario()
                {
                    ScenarioName = "TRM range 10v10 ballistics",
                    PerfectScore = 250,
                    Civilization = Civilization.BRITONS
                }
            };

            return scenarios;
        }

        public string ScenarioName { get; set; }
        public double PerfectScore { get; set; }
        public Civilization Civilization { get; set; }
        public string OpponentAiFile { get; set; }

        public Game CreateGame(string my_ai_file)
        {
            var game = new Game()
            {
                GameType = GameType.SCENARIO,
                ScenarioName = ScenarioName,
                MapType = MapType.RANDOM_MAP,
                MapSize = MapSize.TINY,
                Difficulty = Difficulty.HARD,
                StartingResources = StartingResources.STANDARD,
                PopulationLimit = 200,
                RevealMap = RevealMap.NORMAL,
                StartingAge = StartingAge.STANDARD,
                VictoryType = VictoryType.CONQUEST,
                VictoryValue = 0,
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
    }
}
