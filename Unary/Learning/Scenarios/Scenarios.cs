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
        public static double GetScores(IEnumerable<KeyValuePair<Game, Scenario>> games, out Dictionary<string, double> scores)
        {
            var scenario_scores = new Dictionary<KeyValuePair<string, string>, double>();
            var scenario_counts = new Dictionary<KeyValuePair<string, string>, int>();

            foreach (var game in games)
            {
                var name = game.Value.Name;
                var opponent = game.Value.OpponentAiFile;
                var score = game.Value.GetScore(game.Key);
                var kvp = new KeyValuePair<string, string>(name, opponent);

                if (!scenario_scores.ContainsKey(kvp))
                {
                    scenario_scores.Add(kvp, 0);
                    scenario_counts.Add(kvp, 0);
                }

                scenario_scores[kvp] += score;
                scenario_counts[kvp]++;
            }

            scores = new();
            var player_scores = new Dictionary<string, double>();

            foreach (var key in scenario_scores.Keys)
            {
                scenario_scores[key] /= scenario_counts[key];
                var name = $"{key.Key} against {key.Value}";
                scores[name] = scenario_scores[key];

                if (!player_scores.ContainsKey(key.Value))
                {
                    player_scores.Add(key.Value, 0);
                }
            }

            foreach (var player in player_scores.Keys)
            {
                var score = GetScore(scenario_scores.Where(x => x.Key.Value == player).Select(x => x.Value));
                player_scores[player] = score;
            }

            var total_score = GetScore(player_scores.Values);

            return total_score;
        }
            

        public static List<Scenario> GetCombatRangedTests()
        {
            var scenarios = new List<Scenario>()
            {
                new Scenario()
                {
                    Name = "TCR 4v4 no ballistics",
                    Civilization = Civilization.BYZANTINES
                },
                new Scenario()
                {
                    Name = "TCR 5v5 no ballistics",
                    Civilization = Civilization.BYZANTINES
                },
                new Scenario()
                {
                    Name = "TCR 6v6 no ballistics",
                    Civilization = Civilization.BRITONS
                },
                new Scenario()
                {
                    Name = "TCR 8v8 ballistics",
                    Civilization = Civilization.BYZANTINES
                },
                new Scenario()
                {
                    Name = "TCR 10v10 ballistics",
                    Civilization = Civilization.BRITONS
                }
            };

            foreach (var scenario in scenarios)
            {
                scenario.PerfectScore = 336;
                scenario.HighResources = true;
                scenario.MapExplored = true;
                scenario.SetTimeLimit(TimeSpan.FromMinutes(10));
            }

            return scenarios;
        }

        public static List<Scenario> GetScoutingTests()
        {
            var scenarios = new List<Scenario>()
            {
                new Scenario()
                {
                    Name = "TS clear",
                    Civilization = Civilization.BRITONS
                }
            };

            foreach (var scenario in scenarios)
            {
                scenario.PerfectScore = 200;
                scenario.HighResources = true;
                scenario.SetTimeLimit(TimeSpan.FromMinutes(5));
            }

            return scenarios;
        }

        private static double GetScore(IEnumerable<double> scores)
        {
            var min = double.MaxValue;
            var total = 0d;
            var count = 0;

            foreach (var score in scores)
            {
                min = Math.Min(min, score);
                total += score;
                count++;
            }

            if (count <= 0)
            {
                throw new ArgumentException("Scores must contain at least 1 value", nameof(scores));
            }

            var avg = total / count;

            return (min + avg) / 2;
        }
    }
}
