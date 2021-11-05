using AoE2Lib;
using AoE2Lib.Bots.GameElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Unary.Managers
{
    class StrategyManager : Manager
    {
        public StrategyManager(Unary unary) : base(unary)
        {

        }

        public IEnumerable<Resource> GetGatherers()
        {
            throw new NotImplementedException();
        }

        public void Attack(Player player)
        {
            Unary.GameState.SetStrategicNumber(StrategicNumber.ENABLE_PATROL_ATTACK, 1);
            Unary.GameState.SetStrategicNumber(StrategicNumber.TARGET_PLAYER_NUMBER, player.PlayerNumber);
            Unary.GameState.SetStrategicNumber(StrategicNumber.MINIMUM_ATTACK_GROUP_SIZE, 1);
            Unary.GameState.SetStrategicNumber(StrategicNumber.MAXIMUM_ATTACK_GROUP_SIZE, 1);
            Unary.GameState.SetStrategicNumber(StrategicNumber.NUMBER_ATTACK_GROUPS, 1000);
            Unary.GameState.SetStrategicNumber(StrategicNumber.ZERO_PRIORITY_DISTANCE, 250);

            Unary.Log.Info($"Attacking player {player.PlayerNumber}");
        }

        public void Retreat()
        {
            Unary.GameState.SetStrategicNumber(StrategicNumber.NUMBER_ATTACK_GROUPS, 0);
        }

        internal override void Update()
        {
            BasicStrategy();
            DoAutoEcoTechs();
        }

        private void BasicStrategy()
        {
            var feudal_age = Unary.GameState.GetTechnology(101);
            var castle_age = Unary.GameState.GetTechnology(102);
            var imperial_age = Unary.GameState.GetTechnology(103);

            var barracks = Unary.GameState.GetUnitType(12);
            var archery_range = Unary.GameState.GetUnitType(87);
            var blacksmith = Unary.GameState.GetUnitType(103);
            var castle = Unary.GameState.GetUnitType(82);
            var archer = Unary.GameState.GetUnitType(4);

            feudal_age.Research(Priority.AGE_UP, false);
            castle_age.Research(Priority.AGE_UP, false);
            imperial_age.Research(Priority.AGE_UP, false);

            Unary.ProductionManager.Build(castle);

            if (castle_age.State == ResearchState.COMPLETE)
            {
                Unary.EconomyManager.MaxTownCenters = 3;
            }

            if (barracks.CountTotal >= 1)
            {
                var max_ranges = (Unary.GameState.MyPlayer.CivilianPopulation - 25) / 10;
                max_ranges = Math.Max(1, max_ranges);
                max_ranges = Math.Min(3, max_ranges);

                if (archery_range.CountTotal < max_ranges)
                {
                    Unary.ProductionManager.Build(archery_range, max_ranges, 1, Priority.PRODUCTION_BUILDING);
                }
                else if (archery_range.Count > 1 && archer.TrainSiteReady == false && Unary.Rng.NextDouble() < 0.1)
                {
                    Unary.ProductionManager.Build(archery_range, 10, 1, Priority.PRODUCTION_BUILDING);
                }
            }

            if (archery_range.CountTotal >= 1 && blacksmith.CountTotal < 1)
            {
                Unary.ProductionManager.Build(blacksmith, 1, 1, Priority.PRODUCTION_BUILDING);
            }

            if (archery_range.Count >= 1)
            {
                archer.Train(50, 3, Priority.MILITARY);
            }

            Unary.EconomyManager.MinFoodGatherers = 7;
            Unary.EconomyManager.MinWoodGatherers = 0;
            Unary.EconomyManager.MinGoldGatherers = 0;
            Unary.EconomyManager.MinStoneGatherers = 0;
            Unary.EconomyManager.ExtraFoodPercentage = 40;
            Unary.EconomyManager.ExtraWoodPercentage = 60;
            Unary.EconomyManager.ExtraGoldPercentage = 0;
            Unary.EconomyManager.ExtraStonePercentage = 0;

            if (feudal_age.State == ResearchState.COMPLETE)
            {
                Unary.EconomyManager.MinFoodGatherers = 7;
                Unary.EconomyManager.MinWoodGatherers = 10;
                Unary.EconomyManager.MinGoldGatherers = 0;
                Unary.EconomyManager.MinStoneGatherers = 0;
                Unary.EconomyManager.ExtraFoodPercentage = 40;
                Unary.EconomyManager.ExtraWoodPercentage = 20;
                Unary.EconomyManager.ExtraGoldPercentage = 30;
                Unary.EconomyManager.ExtraStonePercentage = 10;

                if (barracks.CountTotal < 1)
                {
                    Unary.ProductionManager.Build(barracks, 1, 1, Priority.PRODUCTION_BUILDING);
                }
            }

            if (castle_age.State == ResearchState.COMPLETE && Unary.GameState.MyPlayer.CivilianPopulation > 40)
            {
                Unary.EconomyManager.MinFoodGatherers = 25;
                Unary.EconomyManager.MinWoodGatherers = 15;
                Unary.EconomyManager.MinGoldGatherers = 0;
                Unary.EconomyManager.MinStoneGatherers = 0;
                Unary.EconomyManager.ExtraFoodPercentage = 10;
                Unary.EconomyManager.ExtraWoodPercentage = 40;
                Unary.EconomyManager.ExtraGoldPercentage = 40;
                Unary.EconomyManager.ExtraStonePercentage = 10;
            }

            if (Unary.GameState.MyPlayer.MilitaryPopulation > 20)
            {
                var target = Unary.GameState.GetPlayers().First(p => p.InGame && p.Stance == PlayerStance.ENEMY);
                Attack(target);
            }
            else if (Unary.GameState.MyPlayer.MilitaryPopulation < 10)
            {
                Retreat();
            }
        }

        private void DoAutoEcoTechs()
        {
            var horse_collar = Unary.GameState.GetTechnology(14);
            var heavy_plow = Unary.GameState.GetTechnology(13);
            var crop_rotation = Unary.GameState.GetTechnology(12);

            horse_collar.Research(Priority.TECH);
            heavy_plow.Research(Priority.TECH);
            crop_rotation.Research(Priority.TECH);

            var double_bit_axe = Unary.GameState.GetTechnology(202);
            var bow_saw = Unary.GameState.GetTechnology(203);
            var two_man_saw = Unary.GameState.GetTechnology(221);

            double_bit_axe.Research(Priority.TECH);
            bow_saw.Research(Priority.TECH);
            two_man_saw.Research(Priority.TECH);

            var gold_mining = Unary.GameState.GetTechnology(55);
            var stone_mining = Unary.GameState.GetTechnology(278);
            var gold_shaft_mining = Unary.GameState.GetTechnology(182);
            var stone_shaft_mining = Unary.GameState.GetTechnology(279);

            gold_mining.Research(Priority.TECH);
            stone_mining.Research(Priority.TECH);
            gold_shaft_mining.Research(Priority.TECH);
            stone_shaft_mining.Research(Priority.TECH);

            var loom = Unary.GameState.GetTechnology(22);
            var wheelbarrow = Unary.GameState.GetTechnology(213);
            var hand_cart = Unary.GameState.GetTechnology(249);

            loom.Research(Priority.AGE_UP, false);
            wheelbarrow.Research(Priority.AGE_UP, false);
            hand_cart.Research(Priority.AGE_UP, false);
        }
    }
}
