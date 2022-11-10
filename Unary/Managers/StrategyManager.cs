using AoE2Lib;
using AoE2Lib.Bots;
using AoE2Lib.Bots.GameElements;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unary.Strategies;
using static Unary.Managers.ProductionManager;

namespace Unary.Managers
{
    internal class StrategyManager : Manager
    {
        private Strategy CurrentStrategy { get; set; } = null;
        private readonly List<Strategy> Strategies = new();

        public StrategyManager(Unary unary) : base(unary)
        {
            var folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Strategies");

            foreach (var file in Directory.EnumerateFiles(folder, "*.json"))
            {
                var strat = Program.Deserialize<Strategy>(file);
                Strategies.Add(strat);
                strat.SetUnary(Unary);
            }
        }

        public int GetDesiredGatherers(Resource resource) => CurrentStrategy.GetDesiredGatherers(resource);

        protected internal override void Update()
        {
            if (CurrentStrategy == null)
            {
                ChooseStrategy();
            }
            else
            {
                CurrentStrategy.Update();
            }
        }

        private void ChooseStrategy()
        {
            CurrentStrategy = Strategies[0];

            Unary.Log.Info($"Choose strategy: {CurrentStrategy.Name}");
        }
    }
}
