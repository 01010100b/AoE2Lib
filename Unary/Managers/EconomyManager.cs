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
            var villager = Unary.GetUnitType(83);

            villager.Train((int)Math.Round(0.6 * Unary.InfoModule.PopulationCap), ConcurrentVillagers, (int)Priority.VILLAGER);
        }

        private void ManageGatherers()
        {
            /* set auto gathering off
            info.StrategicNumbers[StrategicNumber.MAXIMUM_FOOD_DROP_DISTANCE] = -2;
            info.StrategicNumbers[StrategicNumber.MAXIMUM_GOLD_DROP_DISTANCE] = -2;
            info.StrategicNumbers[StrategicNumber.MAXIMUM_HUNT_DROP_DISTANCE] = -2;
            info.StrategicNumbers[StrategicNumber.MAXIMUM_STONE_DROP_DISTANCE] = -2;
            info.StrategicNumbers[StrategicNumber.MAXIMUM_WOOD_DROP_DISTANCE] = -2;
            info.StrategicNumbers[StrategicNumber.CAP_CIVILIAN_GATHERERS] = 0;
            info.StrategicNumbers[StrategicNumber.MINIMUM_BOAR_HUNT_GROUP_SIZE] = 0;
            */

            Unary.SetStrategicNumber(StrategicNumber.CAP_CIVILIAN_EXPLORERS, 0);

            var pop = Unary.MyPlayer.CivilianPopulation;

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

            Unary.SetStrategicNumber(StrategicNumber.FOOD_GATHERER_PERCENTAGE, food * 100 / pop);
            Unary.SetStrategicNumber(StrategicNumber.WOOD_GATHERER_PERCENTAGE, wood * 100 / pop);
            Unary.SetStrategicNumber(StrategicNumber.GOLD_GATHERER_PERCENTAGE, gold * 100 / pop);
            Unary.SetStrategicNumber(StrategicNumber.STONE_GATHERER_PERCENTAGE, stone * 100 / pop);
        }

        private void ManageDropsites()
        {
            var tc = Unary.GetUnitType(109);
            var mill = Unary.GetUnitType(68);
            var lumber_camp = Unary.GetUnitType(562);
            var mining_camp = Unary.GetUnitType(584);
            var farm = Unary.GetUnitType(50);

            if (Unary.MyPlayer.GetUnits().Count(u => u.Targetable && u[ObjectData.BASE_TYPE] == tc.Id) < TownCenters)
            {
                tc.Build(TownCenters, 1, (int)Priority.DROPSITE);
            }

            if (Unary.GetStrategicNumber(StrategicNumber.WOOD_GATHERER_PERCENTAGE) > 0)
            {
                if (Unary.GetResourceFound(Resource.WOOD) && Unary.GetDropsiteMinDistance(Resource.WOOD) > 2 && Unary.GetDropsiteMinDistance(Resource.WOOD) < 200)
                {
                    var camp_distance = Unary.GetStrategicNumber(StrategicNumber.LUMBER_CAMP_MAX_DISTANCE);
                    if (Unary.GetDropsiteMinDistance(Resource.WOOD) < camp_distance)
                    {
                        Unary.SetStrategicNumber(StrategicNumber.LUMBER_CAMP_MAX_DISTANCE, camp_distance + 1);
                    }
                    else
                    {
                        lumber_camp.Build(100, 1, (int)Priority.DROPSITE);
                    }
                }
            }

            if (lumber_camp.Count < 1)
            {
                return;
            }
            
            if (Unary.GetStrategicNumber(StrategicNumber.FOOD_GATHERER_PERCENTAGE) > 0)
            {
                if (mill.CountTotal < 1)
                {
                    if (Unary.GetResourceFound(Resource.FOOD) && Unary.GetDropsiteMinDistance(Resource.FOOD) > 2 && Unary.GetDropsiteMinDistance(Resource.FOOD) < 200)
                    {
                        mill.Build(100, 1, (int)Priority.DROPSITE);
                    }
                }
                else if (mill.Count >= 1)
                {
                    var needed_farms = Unary.MyPlayer.CivilianPopulation * Unary.GetStrategicNumber(StrategicNumber.FOOD_GATHERER_PERCENTAGE) / 100;
                    if (Unary.GetDropsiteMinDistance(Resource.FOOD) <= 3)
                    {
                        needed_farms -= 4;
                    }

                    if (Unary.MyPlayer.GetUnits().Count(u => u[ObjectData.CMDID] == (int)CmdId.LIVESTOCK_GAIA) >= 2)
                    {
                        needed_farms -= 6;
                    }

                    if (farm.CountTotal < needed_farms)
                    {
                        farm.Build(needed_farms, 3, (int)Priority.FARM);
                    }

                    var needed_mills = 1 + ((needed_farms - 5) / 4);
                    if (mill.CountTotal < needed_mills)
                    {
                        mill.Build(needed_mills, 1, (int)Priority.FARM);
                    }
                }
            }

            var gathered = new List<Resource>();

            if (Unary.GetStrategicNumber(StrategicNumber.GOLD_GATHERER_PERCENTAGE) > 0)
            {
                gathered.Add(Resource.GOLD);
            }

            if (Unary.GetStrategicNumber(StrategicNumber.STONE_GATHERER_PERCENTAGE) > 0)
            {
                gathered.Add(Resource.STONE);
            }

            foreach (var resource in gathered)
            {
                if (Unary.GetResourceFound(resource) && Unary.GetDropsiteMinDistance(resource) > 2 && Unary.GetDropsiteMinDistance(resource) < 200)
                {
                    var camp_distance = Unary.GetStrategicNumber(StrategicNumber.MINING_CAMP_MAX_DISTANCE);
                    if (Unary.GetDropsiteMinDistance(resource) < camp_distance)
                    {
                        Unary.SetStrategicNumber(StrategicNumber.MINING_CAMP_MAX_DISTANCE, camp_distance + 1);
                    }
                    else
                    {
                        mining_camp.Build(100, 1, (int)Priority.DROPSITE);
                    }
                }
            }
        }
    }
}
