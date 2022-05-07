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
        public static List<Scenario> GetDefaultScenarios()
        {
            var scenarios = new List<Scenario>()
            {
                new Scenario()
                {
                    ScenarioName = "TRM range 4v4 no ballistics",
                    PerfectScore = 336,
                    Civilization = Civilization.FRANKS
                },
                new Scenario()
                {
                    ScenarioName = "TRM range 5v5 no ballistics",
                    PerfectScore = 336,
                    Civilization = Civilization.FRANKS
                },
                new Scenario()
                {
                    ScenarioName = "TRM range 6v6 no ballistics",
                    PerfectScore = 336,
                    Civilization = Civilization.BRITONS
                },
                new Scenario()
                {
                    ScenarioName = "TRM range 7v7 ballistics",
                    PerfectScore = 336,
                    Civilization = Civilization.FRANKS
                },
                new Scenario()
                {
                    ScenarioName = "TRM range 8v8 ballistics",
                    PerfectScore = 336,
                    Civilization = Civilization.BYZANTINES
                },
                new Scenario()
                {
                    ScenarioName = "TRM range 10v10 ballistics",
                    PerfectScore = 336,
                    Civilization = Civilization.BRITONS
                }
            };

            foreach (var scenario in scenarios)
            {
                scenario.HighResources = true;
                scenario.MapExplored = true;
                scenario.TimeLimit = 1200;
            }

            return scenarios;
        }

        public static void RunScenarios(string exe, double speed, List<Scenario> scenarios, List<string> opponents)
        {
            const int GAMES_PER_SCENARIO = 1;

            var queue = new ConcurrentQueue<KeyValuePair<Game, Dictionary<int, Bot>>>();
            var results = new Dictionary<Game, Scenario>();
            var runners = new List<InstanceRunner>();

            for (int i = 0; i < 3; i++)
            {
                var runner = new InstanceRunner(exe, null, speed);
                runner.Start(queue);
                runners.Add(runner);
            }

            Program.Log.Info("Started runners");

            for (int i = 0; i < GAMES_PER_SCENARIO; i++)
            {
                foreach (var opponent in opponents)
                {
                    foreach (var scenario in scenarios)
                    {
                        scenario.OpponentAiFile = opponent;

                        var game = scenario.CreateGame("Unary");
                        var unary = new Unary(Program.DefaultSettings);
                        var dict = new Dictionary<int, Bot>() { { 1, unary } };
                        queue.Enqueue(new KeyValuePair<Game, Dictionary<int, Bot>>(game, dict));
                        results.Add(game, scenario);
                    }
                }
            }

            Program.Log.Info($"Total game count {results.Count}");

            foreach (var result in results)
            {
                while (!result.Key.Finished)
                {
                    Thread.Sleep(1000);
                }

                Program.Log.Info($"Ran game {result.Value.ScenarioName} against {result.Value.OpponentAiFile} score {result.Value.GetScore(result.Key):P0}.");
            }

            Program.Log.Info("All games finished");
            
            foreach (var runner in runners)
            {
                runner.Stop();
            }

            var scores = new Dictionary<KeyValuePair<string, string>, double>();

            foreach (var result in results)
            {
                var kvp = new KeyValuePair<string, string>(result.Value.ScenarioName, result.Value.OpponentAiFile);
                var score = result.Value.GetScore(result.Key);

                if (!scores.ContainsKey(kvp))
                {
                    scores.Add(kvp, 0);
                }

                scores[kvp] += score / GAMES_PER_SCENARIO;
            }

            foreach (var score in scores)
            {
                Program.Log.Info($"Test {score.Key.Key} against {score.Key.Value}: {score.Value:P0}");
            }
        }

        public string ScenarioName { get; set; }
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
                ScenarioName = ScenarioName,
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
    }
}
