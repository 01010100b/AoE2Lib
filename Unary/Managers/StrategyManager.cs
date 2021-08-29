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

        public void Attack(Player player)
        {
            Unary.GameState.SetStrategicNumber(StrategicNumber.ENABLE_PATROL_ATTACK, 1);
            Unary.GameState.SetStrategicNumber(StrategicNumber.TARGET_PLAYER_NUMBER, player.PlayerNumber);
            Unary.GameState.SetStrategicNumber(StrategicNumber.MINIMUM_ATTACK_GROUP_SIZE, 1);
            Unary.GameState.SetStrategicNumber(StrategicNumber.MAXIMUM_ATTACK_GROUP_SIZE, 1);
            Unary.GameState.SetStrategicNumber(StrategicNumber.NUMBER_ATTACK_GROUPS, 1000);
            Unary.GameState.SetStrategicNumber(StrategicNumber.ZERO_PRIORITY_DISTANCE, 250);
        }

        public void Retreat()
        {
            Unary.GameState.SetStrategicNumber(StrategicNumber.NUMBER_ATTACK_GROUPS, 0);
        }

        internal override void Update()
        {
            Unary.GameState.SetStrategicNumber(StrategicNumber.CAP_CIVILIAN_EXPLORERS, 0);
            Unary.GameState.SetStrategicNumber(StrategicNumber.NUMBER_EXPLORE_GROUPS, 1);
            Unary.GameState.SetStrategicNumber(StrategicNumber.HOME_EXPLORATION_TIME, 600);

            Unary.GameState.SetStrategicNumber(StrategicNumber.TASK_UNGROUPED_SOLDIERS, 0);
            Unary.GameState.SetStrategicNumber(StrategicNumber.DISABLE_DEFEND_GROUPS, 8);

            BasicStrategy();
        }

        private void BasicStrategy()
        {
            var feudal_age = Unary.GameState.GetTechnology(101);
            var castle_age = Unary.GameState.GetTechnology(102);
            var imperial_age = Unary.GameState.GetTechnology(103);

            feudal_age.Research(Priority.AGE_UP, false);
            castle_age.Research(Priority.AGE_UP, false);
            imperial_age.Research(Priority.AGE_UP, false);

            if (castle_age.State == ResearchState.COMPLETE)
            {
                Unary.EconomyManager.MaxTownCenters = 3;
            }

            var barracks = Unary.GameState.GetUnitType(12);
            var archery_range = Unary.GameState.GetUnitType(87);
            var blacksmith = Unary.GameState.GetUnitType(103);

            var max_ranges = (Unary.GameState.MyPlayer.CivilianPopulation - 25) / 10;
            max_ranges = Math.Max(1, max_ranges);

            if (barracks.CountTotal >= 1 && archery_range.CountTotal < max_ranges)
            {
                archery_range.BuildNormal(max_ranges, 1, Priority.PRODUCTION_BUILDING);
            }

            if (archery_range.CountTotal >= 1 && blacksmith.CountTotal < 1)
            {
                blacksmith.BuildNormal(1, 1, Priority.PRODUCTION_BUILDING);
            }

            var archer = Unary.GameState.GetUnitType(4);

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
                    barracks.BuildNormal(1, 1, Priority.PRODUCTION_BUILDING);
                }
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
    }
}
