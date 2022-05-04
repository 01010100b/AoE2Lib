using AoE2Lib;
using AoE2Lib.Bots;
using AoE2Lib.Bots.GameElements;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unary.Strategies;
using static Unary.Managers.ResourcesManager;
using static Unary.Strategies.Strategy.BuildOrderCommand;

namespace Unary.Managers
{
    // choose strategy
    // actual strategy implementation in Strategy.cs class
    class StrategyManager : Manager
    {
        public Player Attacking { get; private set; } = null;

        private Strategy CurrentStrategy { get; set; }
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

            ChooseStrategy();
        }

        public int GetDesiredGatherers(Resource resource) => CurrentStrategy.GetDesiredGatherers(resource);

        public int GetMinimumGatherers(Resource resource) => CurrentStrategy.GetMinimumGatherers(resource);

        public int GetMaximumGatherers(Resource resource) => CurrentStrategy.GetMaximumGatherers(resource);

        internal override void Update()
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

        private void DoAttacking()
        {
            Player attack = null;

            if (Unary.GameState.MyPlayer.MilitaryPopulation >= 20)
            {
                attack = Unary.GameState.CurrentEnemies.Where(p => p.InGame).FirstOrDefault();
            }

            if (attack != Attacking)
            {
                if (attack != null)
                {
                    Unary.Log.Info($"Attacking {attack.Stance} player {attack.PlayerNumber}");
                }
                else
                {
                    Unary.Log.Info($"Stop attacking {Attacking.Stance} player {Attacking.PlayerNumber}");
                }

                Attacking = attack;
            }
        }

        private void DoAutoEcoTechs()
        {
            var horse_collar = Unary.GameState.GetTechnology(14);
            var heavy_plow = Unary.GameState.GetTechnology(13);
            var crop_rotation = Unary.GameState.GetTechnology(12);

            horse_collar.OldResearch(Priority.TECH);
            heavy_plow.OldResearch(Priority.TECH);
            crop_rotation.OldResearch(Priority.TECH);

            var double_bit_axe = Unary.GameState.GetTechnology(202);
            var bow_saw = Unary.GameState.GetTechnology(203);
            var two_man_saw = Unary.GameState.GetTechnology(221);

            double_bit_axe.OldResearch(Priority.TECH);
            bow_saw.OldResearch(Priority.TECH);
            two_man_saw.OldResearch(Priority.TECH);

            var gold_mining = Unary.GameState.GetTechnology(55);
            var stone_mining = Unary.GameState.GetTechnology(278);
            var gold_shaft_mining = Unary.GameState.GetTechnology(182);
            var stone_shaft_mining = Unary.GameState.GetTechnology(279);

            gold_mining.OldResearch(Priority.TECH);
            stone_mining.OldResearch(Priority.TECH);
            gold_shaft_mining.OldResearch(Priority.TECH);
            stone_shaft_mining.OldResearch(Priority.TECH);

            var loom = Unary.GameState.GetTechnology(22);
            var wheelbarrow = Unary.GameState.GetTechnology(213);
            var hand_cart = Unary.GameState.GetTechnology(249);

            loom.OldResearch(Priority.AGE_UP, false);
            wheelbarrow.OldResearch(Priority.AGE_UP, false);
            hand_cart.OldResearch(Priority.AGE_UP, false);
        }
    }
}
