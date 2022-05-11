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
                scenario.TimeLimit = 1200;
            }

            return scenarios;
        }
    }
}
