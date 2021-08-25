using AoE2Lib;
using AoE2Lib.Bots.GameElements;
using AoE2Lib.Bots.Modules;
using AoE2Lib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unary.Operations;

namespace Unary.Managers
{
    class StrategyManager : Manager
    {
        public StrategyManager(Unary unary) : base(unary)
        {

        }

        public void Attack(Player player)
        {
            Unary.SetStrategicNumber(StrategicNumber.ENABLE_PATROL_ATTACK, 1);
            Unary.SetStrategicNumber(StrategicNumber.TARGET_PLAYER_NUMBER, player.PlayerNumber);
            Unary.SetStrategicNumber(StrategicNumber.MINIMUM_ATTACK_GROUP_SIZE, 1);
            Unary.SetStrategicNumber(StrategicNumber.MAXIMUM_ATTACK_GROUP_SIZE, 1);
            Unary.SetStrategicNumber(StrategicNumber.NUMBER_ATTACK_GROUPS, 100);
            Unary.SetStrategicNumber(StrategicNumber.ZERO_PRIORITY_DISTANCE, 250);
        }

        public void Retreat()
        {
            Unary.SetStrategicNumber(StrategicNumber.NUMBER_ATTACK_GROUPS, 0);
        }

        internal override void Update()
        {
            Unary.SetStrategicNumber(StrategicNumber.CAP_CIVILIAN_EXPLORERS, 0);
            Unary.SetStrategicNumber(StrategicNumber.NUMBER_EXPLORE_GROUPS, 1);
            Unary.SetStrategicNumber(StrategicNumber.HOME_EXPLORATION_TIME, 600);

            Unary.SetStrategicNumber(StrategicNumber.TASK_UNGROUPED_SOLDIERS, 0);
            Unary.SetStrategicNumber(StrategicNumber.DISABLE_DEFEND_GROUPS, 8);

            BasicStrategy();
        }

        private void BasicStrategy()
        {
            var feudal_age = Unary.GetTechnology(101);
            var castle_age = Unary.GetTechnology(102);
            var imperial_age = Unary.GetTechnology(103);

            feudal_age.Research((int)Priority.AGE_UP, false);
            castle_age.Research((int)Priority.AGE_UP, false);
            imperial_age.Research((int)Priority.AGE_UP, false);

            var barracks = Unary.GetUnitType(12);
            var archery_range = Unary.GetUnitType(87);
            var blacksmith = Unary.GetUnitType(103);

            var ranges = (Unary.MyPlayer.CivilianPopulation - 25) / 10;
            ranges = Math.Max(1, ranges);

            if (barracks.CountTotal >= 1 && archery_range.CountTotal < ranges)
            {
                archery_range.Build(ranges, 1, (int)Priority.PRODUCTION_BUILDING);
            }

            if (archery_range.CountTotal >= 1 && blacksmith.CountTotal < 1)
            {
                blacksmith.Build(1, 1, (int)Priority.PRODUCTION_BUILDING);
            }

            var archer = Unary.GetUnitType(4);

            if (archery_range.Count >= 1)
            {
                archer.Train(50, 3, (int)Priority.MILITARY);
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
                    barracks.Build(1, 1, (int)Priority.PRODUCTION_BUILDING);
                }
            }

            if (Unary.MyPlayer.MilitaryPopulation > 20)
            {
                var target = Unary.GetPlayers().First(p => p.Stance == PlayerStance.ENEMY);
                Attack(target);
            }
            else if (Unary.MyPlayer.MilitaryPopulation < 10)
            {
                Retreat();
            }
        }
    }
}
