using AoE2Lib;
using AoE2Lib.Bots.GameElements;
using AoE2Lib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unary.Operations;

namespace Unary.Managers
{
    class EconomyManager : Manager
    {
        public int MinFoodGatherers { get; set; } = 7;
        public int MinWoodGatherers { get; set; } = 0;
        public int MinGoldGatherers { get; set; } = 0;
        public int MinStoneGatherers { get; set; } = 0;
        public int ExtraFoodPercentage { get; set; } = 60;
        public int ExtraWoodPercentage { get; set; } = 40;
        public int ExtraGoldPercentage { get; set; } = 0;
        public int ExtraStonePercentage { get; set; } = 0;
        public int TownCenters { get; set; } = 3;
        public int ConcurrentVillagers { get; set; } = 3;

        public EconomyManager(Unary unary) : base(unary)
        {

        }

        public override void Update()
        {
            ManagePopulation();
            ManageGatherers();
            ManageDropsites();
        }

        private void ManagePopulation()
        {
            const int VILLAGER = 83;

            Unary.ProductionManager.Train(VILLAGER, (int)Math.Round(0.6 * Unary.InfoModule.PopulationCap), ConcurrentVillagers, 100, true);
        }

        private void ManageGatherers()
        {
            var info = Unary.InfoModule;
            /* set auto gathering off
            info.StrategicNumbers[StrategicNumber.MAXIMUM_FOOD_DROP_DISTANCE] = -2;
            info.StrategicNumbers[StrategicNumber.MAXIMUM_GOLD_DROP_DISTANCE] = -2;
            info.StrategicNumbers[StrategicNumber.MAXIMUM_HUNT_DROP_DISTANCE] = -2;
            info.StrategicNumbers[StrategicNumber.MAXIMUM_STONE_DROP_DISTANCE] = -2;
            info.StrategicNumbers[StrategicNumber.MAXIMUM_WOOD_DROP_DISTANCE] = -2;
            info.StrategicNumbers[StrategicNumber.CAP_CIVILIAN_GATHERERS] = 0;
            info.StrategicNumbers[StrategicNumber.MINIMUM_BOAR_HUNT_GROUP_SIZE] = 0;
            */

            var pop = 0;
            if (Unary.PlayersModule.Players.TryGetValue(info.PlayerNumber, out Player me))
            {
                pop = me.CivilianPopulation;
            }

            var food = MinFoodGatherers;
            var wood = MinWoodGatherers;
            var gold = MinGoldGatherers;
            var stone = MinStoneGatherers;

            pop -= food;
            pop -= wood;
            pop -= gold;
            pop -= stone;

            if (pop > 0)
            {
                food += (int)Math.Round(pop * ExtraFoodPercentage / 100d);
                wood += (int)Math.Round(pop * ExtraWoodPercentage / 100d);
                gold += (int)Math.Round(pop * ExtraGoldPercentage / 100d);
                stone += (int)Math.Round(pop * ExtraStonePercentage / 100d);
            }

            pop = food + wood + gold + stone;

            info.StrategicNumbers[StrategicNumber.FOOD_GATHERER_PERCENTAGE] = food * 100 / pop;
            info.StrategicNumbers[StrategicNumber.WOOD_GATHERER_PERCENTAGE] = wood * 100 / pop;
            info.StrategicNumbers[StrategicNumber.GOLD_GATHERER_PERCENTAGE] = gold * 100 / pop;
            info.StrategicNumbers[StrategicNumber.STONE_GATHERER_PERCENTAGE] = stone * 100 / pop;
        }

        private void ManageDropsites()
        {
            const int TC = 109;
            const int MILL = 68;
            const int MINING_CAMP = 584;
            const int LUMBER_CAMP = 562;
            var info = Unary.InfoModule;
            var camp_distance = info.StrategicNumbers[StrategicNumber.CAMP_MAX_DISTANCE];

            var town_centers = 0;
            if (Unary.PlayersModule.Players.TryGetValue(info.PlayerNumber, out Player me))
            {
                town_centers = me.Units.Count(u => u.Targetable && u[ObjectData.BASE_TYPE] == TC);
            }

            if (town_centers < TownCenters)
            {
                Unary.ProductionManager.Build(TC, new List<Position>(), TownCenters, 1, 200, true);
            }
            
            var gathered = new List<Resource>();
            if (info.StrategicNumbers[StrategicNumber.FOOD_GATHERER_PERCENTAGE] > 0)
            {
                gathered.Add(Resource.FOOD);
            }

            if (info.StrategicNumbers[StrategicNumber.WOOD_GATHERER_PERCENTAGE] > 0)
            {
                gathered.Add(Resource.WOOD);
            }

            if (info.StrategicNumbers[StrategicNumber.GOLD_GATHERER_PERCENTAGE] > 0)
            {
                gathered.Add(Resource.GOLD);
            }

            if (info.StrategicNumbers[StrategicNumber.STONE_GATHERER_PERCENTAGE] > 0)
            {
                gathered.Add(Resource.STONE);
            }

            foreach (var resource in gathered)
            {
                if (info.ResourceFound[resource] && info.DropsiteMinDistance[resource] > 2 && info.DropsiteMinDistance[resource] < 200)
                {
                    if (info.DropsiteMinDistance[resource] > camp_distance)
                    {
                        info.StrategicNumbers[StrategicNumber.CAMP_MAX_DISTANCE]++;
                    }
                    else
                    {
                        var camp = MINING_CAMP;
                        if (resource == Resource.FOOD)
                        {
                            camp = MILL;
                        }
                        else if (resource == Resource.WOOD)
                        {
                            camp = LUMBER_CAMP;
                        }

                        Unary.ProductionManager.Build(camp, new List<Position>(), 100, 1, 200, true);
                    }
                }
            }
        }
    }
}
