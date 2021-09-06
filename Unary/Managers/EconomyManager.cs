﻿using AoE2Lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
        public int MaxTownCenters { get; set; } = 1;
        public int ConcurrentVillagers { get; set; } = 3;

        public EconomyManager(Unary unary) : base(unary)
        {

        }

        internal override void Update()
        {
            ManagePopulation();
            ManageGatherers();
            ManageDropsites();
        }

        private void ManagePopulation()
        {
            var villager = Unary.GameState.GetUnitType(83);
            var house = Unary.GameState.GetUnitType(70);

            villager.Train((int)Math.Round(0.6 * Unary.GameState.MyPlayer.GetFact(FactId.POPULATION_CAP)), ConcurrentVillagers, Priority.VILLAGER);

            var margin = 5;
            var pending = 1;

            if (Unary.GameState.GetTechnology(101).State == ResearchState.COMPLETE)
            {
                margin = 10;
            }

            if (Unary.GameState.GetTechnology(102).State == ResearchState.COMPLETE)
            {
                pending = 2;
            }

            if (Unary.GameState.MyPlayer.GetFact(FactId.POPULATION_HEADROOM) > 0 && Unary.GameState.MyPlayer.GetFact(FactId.HOUSING_HEADROOM) < margin && house.Pending < pending)
            {
                house.BuildNormal(1000, pending, Priority.HOUSING);
            }
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
            Unary.GameState.SetStrategicNumber(StrategicNumber.CAP_CIVILIAN_EXPLORERS, 0);

            Unary.GameState.SetStrategicNumber(StrategicNumber.INTELLIGENT_GATHERING, 1);
            Unary.GameState.SetStrategicNumber(StrategicNumber.USE_BY_TYPE_MAX_GATHERING, 1);

            Unary.GameState.SetStrategicNumber(StrategicNumber.MAXIMUM_WOOD_DROP_DISTANCE, 7);
            Unary.GameState.SetStrategicNumber(StrategicNumber.MAXIMUM_GOLD_DROP_DISTANCE, 7);
            Unary.GameState.SetStrategicNumber(StrategicNumber.MAXIMUM_STONE_DROP_DISTANCE, 7);

            Unary.GameState.SetStrategicNumber(StrategicNumber.MAXIMUM_FOOD_DROP_DISTANCE, 8);
            Unary.GameState.SetStrategicNumber(StrategicNumber.MAXIMUM_HUNT_DROP_DISTANCE, 8);
            Unary.GameState.SetStrategicNumber(StrategicNumber.ENABLE_BOAR_HUNTING, 0);
            Unary.GameState.SetStrategicNumber(StrategicNumber.LIVESTOCK_TO_TOWN_CENTER, 1);

            var pop = Unary.GameState.MyPlayer.CivilianPopulation;

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

            Unary.GameState.SetStrategicNumber(StrategicNumber.FOOD_GATHERER_PERCENTAGE, food * 100 / pop);
            Unary.GameState.SetStrategicNumber(StrategicNumber.WOOD_GATHERER_PERCENTAGE, wood * 100 / pop);
            Unary.GameState.SetStrategicNumber(StrategicNumber.GOLD_GATHERER_PERCENTAGE, gold * 100 / pop);
            Unary.GameState.SetStrategicNumber(StrategicNumber.STONE_GATHERER_PERCENTAGE, stone * 100 / pop);
        }

        private void ManageDropsites()
        {
            var tc = Unary.GameState.GetUnitType(109);
            var mill = Unary.GameState.GetUnitType(68);
            var lumber_camp = Unary.GameState.GetUnitType(562);
            var mining_camp = Unary.GameState.GetUnitType(584);
            var farm = Unary.GameState.GetUnitType(50);

            Unary.GameState.SetStrategicNumber(StrategicNumber.MILL_MAX_DISTANCE, 30);

            if (Unary.GameState.MyPlayer.GetUnits().Count(u => u.Targetable && u[ObjectData.BASE_TYPE] == tc.Id) < MaxTownCenters)
            {
                Unary.Log.Info("Building TC");
                tc.BuildNormal(MaxTownCenters, 1, Priority.DROPSITE);
            }

            if (Unary.GameState.GetStrategicNumber(StrategicNumber.WOOD_GATHERER_PERCENTAGE) > 0)
            {
                if (Unary.GameState.GetResourceFound(Resource.WOOD) && Unary.GameState.GetDropsiteMinDistance(Resource.WOOD) > 2 && Unary.GameState.GetDropsiteMinDistance(Resource.WOOD) < 200)
                {
                    Unary.Log.Info("Building lumber camp");
                    lumber_camp.BuildNormal(100, 1, Priority.DROPSITE);
                }
            }

            if (lumber_camp.Count < 1)
            {
                return;
            }
            
            if (Unary.GameState.GetStrategicNumber(StrategicNumber.FOOD_GATHERER_PERCENTAGE) > 0)
            {
                if (mill.CountTotal < 1)
                {
                    if (Unary.GameState.MyPlayer.CivilianPopulation >= 11 && Unary.GameState.GetResourceFound(Resource.FOOD) && Unary.GameState.GetDropsiteMinDistance(Resource.FOOD) < 200)
                    {
                        Unary.Log.Info("Building mill");
                        mill.BuildNormal(100, 1, Priority.DROPSITE);
                    }
                }
                else if (mill.Count >= 1)
                {
                    var needed_farms = Unary.GameState.MyPlayer.CivilianPopulation * Unary.GameState.GetStrategicNumber(StrategicNumber.FOOD_GATHERER_PERCENTAGE) / 100;

                    Unary.Log.Info($"I have {farm.CountTotal} farms and I want {needed_farms} with {Unary.GameState.GetStrategicNumber(StrategicNumber.FOOD_GATHERER_PERCENTAGE)} food perc");

                    if (farm.CountTotal < needed_farms)
                    {
                        Unary.Log.Info("Building farm");
                        farm.BuildLine(Unary.BuildingManager.GetBuildingPlacements(farm), needed_farms, 3, Priority.FARM);
                    }

                    var needed_mills = 1 + ((needed_farms - 8) / 4);
                    if (mill.CountTotal < needed_mills)
                    {
                        Unary.Log.Info("Building mill");
                        mill.BuildNormal(needed_mills, 1, Priority.FARM);
                    }
                }
            }

            var gathered = new List<Resource>();

            if (Unary.GameState.GetStrategicNumber(StrategicNumber.GOLD_GATHERER_PERCENTAGE) > 0)
            {
                gathered.Add(Resource.GOLD);
            }

            if (Unary.GameState.GetStrategicNumber(StrategicNumber.STONE_GATHERER_PERCENTAGE) > 0)
            {
                gathered.Add(Resource.STONE);
            }

            foreach (var resource in gathered)
            {
                if (Unary.GameState.GetResourceFound(resource) && Unary.GameState.GetDropsiteMinDistance(resource) > 2 && Unary.GameState.GetDropsiteMinDistance(resource) < 200)
                {
                    var camp_distance = Unary.GameState.GetStrategicNumber(StrategicNumber.MINING_CAMP_MAX_DISTANCE);
                    Unary.GameState.SetStrategicNumber(StrategicNumber.MINING_CAMP_MAX_DISTANCE, camp_distance + 1);
                    if (Unary.GameState.GetDropsiteMinDistance(resource) < camp_distance)
                    {
                        Unary.Log.Info($"Building {resource} camp");
                        mining_camp.BuildNormal(100, 1, Priority.DROPSITE);
                    }
                }
            }
        }

        private void ManageGatherOperations()
        {

        }
    }
}
