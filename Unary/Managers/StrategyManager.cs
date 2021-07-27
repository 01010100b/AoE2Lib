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
            Unary.InfoModule.StrategicNumbers[StrategicNumber.INITIAL_EXPLORATION_REQUIRED] = 0;
        }

        private void BasicStrategy()
        {
            const int FEUDAL_AGE = 101;
            const int CASTLE_AGE = 102;
            const int IMPERIAL_AGE = 103;

            if (!Unary.PlayersModule.Players.ContainsKey(Unary.InfoModule.PlayerNumber))
            {
                return;
            }

            var me = Unary.PlayersModule.Players[Unary.InfoModule.PlayerNumber];

            Unary.EconomyManager.MinFoodGatherers = 7;
            Unary.EconomyManager.MinWoodGatherers = 0;
            Unary.EconomyManager.MinGoldGatherers = 0;
            Unary.EconomyManager.MinStoneGatherers = 0;
            Unary.EconomyManager.ExtraFoodPercentage = 60;
            Unary.EconomyManager.ExtraWoodPercentage = 40;
            Unary.EconomyManager.ExtraGoldPercentage = 0;
            Unary.EconomyManager.ExtraStonePercentage = 0;
            
            Unary.ProductionManager.Research(FEUDAL_AGE, 300);
            Unary.ProductionManager.Research(CASTLE_AGE, 300);
            Unary.ProductionManager.Research(IMPERIAL_AGE, 300);
            
            if (Unary.ResearchModule.Researches[FEUDAL_AGE].State == ResearchState.COMPLETE)
            {
                Unary.EconomyManager.MinFoodGatherers = 7;
                Unary.EconomyManager.MinWoodGatherers = 10;
                Unary.EconomyManager.MinGoldGatherers = 0;
                Unary.EconomyManager.MinStoneGatherers = 0;
                Unary.EconomyManager.ExtraFoodPercentage = 30;
                Unary.EconomyManager.ExtraWoodPercentage = 20;
                Unary.EconomyManager.ExtraGoldPercentage = 40;
                Unary.EconomyManager.ExtraStonePercentage = 10;
            }
        }
    }
}
