using AoE2Lib.Bots;
using AoE2Lib.Games;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Unary.Learning.Scenarios
{
    internal static class Scenarios
    {
        public static List<Tuple<Settings, Scenario, Game>> RunScenarios(string exe, List<Settings> settings, List<Scenario> scenarios, int max_concurrent = int.MaxValue)
        {
            const int GAMES_PER_SCENARIO = 50;

            var queue = new ConcurrentQueue<KeyValuePair<Game, Dictionary<int, Bot>>>();
            var games = new Dictionary<Game, Scenario>();
            var results = new List<Tuple<Settings, Scenario, Game>>();
            var runners = new List<InstanceRunner>();
            var count = Math.Max(1, Math.Min(max_concurrent, Environment.ProcessorCount - 1));

            for (int i = 0; i < count; i++)
            {
                var runner = new InstanceRunner(exe);
                runner.Start(queue);
                runners.Add(runner);
            }

            Program.Log.Info("Started runners");

            foreach (var setting in settings)
            {
                for (int i = 0; i < GAMES_PER_SCENARIO; i++)
                {
                    foreach (var scenario in scenarios)
                    {
                        var game = scenario.CreateGame("Unary");
                        var unary = new Unary(setting.Copy());
                        var dict = new Dictionary<int, Bot>() { { 1, unary } };
                        queue.Enqueue(new KeyValuePair<Game, Dictionary<int, Bot>>(game, dict));
                        games.Add(game, scenario);
                        results.Add(new Tuple<Settings, Scenario, Game>(setting, scenario, game));
                    }
                }
            }
            

            Program.Log.Info($"Total game count {games.Count}");

            foreach (var result in games)
            {
                while (!result.Key.Finished)
                {
                    Thread.Sleep(1000);
                }

                Program.Log.Info($"Ran game {result.Value} score {result.Value.GetScore(result.Key):P0}.");
            }

            Program.Log.Info("All games finished");

            foreach (var runner in runners)
            {
                runner.Stop();
            }

            var scores = new Dictionary<Scenario, double>();

            foreach (var game in games)
            {
                var score = game.Value.GetScore(game.Key);

                if (!scores.ContainsKey(game.Value))
                {
                    scores.Add(game.Value, 0);
                }

                scores[game.Value] += score / (GAMES_PER_SCENARIO * settings.Count);
            }

            foreach (var score in scores)
            {
                Program.Log.Info($"Test {score.Key}: {score.Value:P0}");
            }

            return results;
        }

        public static List<Scenario> GetCombatRangedTests()
        {
            var scenarios = new List<Scenario>()
            {
                new Scenario()
                {
                    Name = "TCR 4v4 no ballistics",
                    PerfectScore = 336,
                    Civilization = Civilization.BYZANTINES
                },
                new Scenario()
                {
                    Name = "TCR 5v5 no ballistics",
                    PerfectScore = 336,
                    Civilization = Civilization.BYZANTINES
                },
                new Scenario()
                {
                    Name = "TCR 6v6 no ballistics",
                    PerfectScore = 336,
                    Civilization = Civilization.BRITONS
                },
                new Scenario()
                {
                    Name = "TCR 8v8 ballistics",
                    PerfectScore = 336,
                    Civilization = Civilization.BYZANTINES
                },
                new Scenario()
                {
                    Name = "TCR 10v10 ballistics",
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
    }
}
