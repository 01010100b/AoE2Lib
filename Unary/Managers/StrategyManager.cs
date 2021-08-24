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

        public override void Update()
        {
            Unary.SetStrategicNumber(StrategicNumber.INITIAL_EXPLORATION_REQUIRED, 0);
            BasicStrategy();
        }

        private void BasicStrategy()
        {
            var feudal_age = Unary.GetTechnology(101);
            var castle_age = Unary.GetTechnology(102);
            var imperial_age = Unary.GetTechnology(103);

            Unary.EconomyManager.MinFoodGatherers = 7;
            Unary.EconomyManager.MinWoodGatherers = 0;
            Unary.EconomyManager.MinGoldGatherers = 0;
            Unary.EconomyManager.MinStoneGatherers = 0;
            Unary.EconomyManager.ExtraFoodPercentage = 30;
            Unary.EconomyManager.ExtraWoodPercentage = 70;
            Unary.EconomyManager.ExtraGoldPercentage = 0;
            Unary.EconomyManager.ExtraStonePercentage = 0;

            feudal_age.Research((int)Priority.AGE_UP, false);
            castle_age.Research((int)Priority.AGE_UP, false);
            imperial_age.Research((int)Priority.AGE_UP, false);

            var barracks = Unary.GetUnitType(12);
            var archery_range = Unary.GetUnitType(87);
            var blacksmith = Unary.GetUnitType(103);
            
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

                if (barracks.CountTotal >= 1 && archery_range.CountTotal < 1)
                {
                    archery_range.Build(1, 1, (int)Priority.PRODUCTION_BUILDING);
                }

                if (archery_range.CountTotal >= 1 && blacksmith.CountTotal < 1)
                {
                    blacksmith.Build(1, 1, (int)Priority.PRODUCTION_BUILDING);
                }
            }
        }
    }
}
