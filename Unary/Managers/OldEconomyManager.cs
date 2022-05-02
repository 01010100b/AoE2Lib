using AoE2Lib;
using AoE2Lib.Bots;
using AoE2Lib.Bots.GameElements;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using Unary.UnitControllers;
using static Unary.Managers.ResourceManager;

namespace Unary.Managers
{
    class OldEconomyManager : Manager
    {
        private int MinFoodGatherers { get; set; } = 7;
        private int MinWoodGatherers { get; set; } = 0;
        private int MinGoldGatherers { get; set; } = 0;
        private int MinStoneGatherers { get; set; } = 0;
        private int ExtraFoodPercentage { get; set; } = 60;
        private int ExtraWoodPercentage { get; set; } = 40;
        private int ExtraGoldPercentage { get; set; } = 0;
        private int ExtraStonePercentage { get; set; } = 0;
        private int MaxTownCenters { get; set; } = 1;
        private int ConcurrentVillagers { get; set; } = 3;
        private readonly List<Unit> Meat = new();
        private readonly List<Unit> Deer = new();
        private int FoodGatherers { get; set; } = 0;
        private int WoodGatherers { get; set; } = 0;
        private int GoldGatherers { get; set; } = 0;
        private int StoneGatherers { get; set; } = 0;

        public OldEconomyManager(Unary unary) : base(unary)
        {

        }

        public IEnumerable<Unit> GetDeer()
        {
            return Deer;
        }

        public IEnumerable<Unit> GetMeat()
        {
            return Meat;
        }

        internal override void Update()
        {
            // update meat

            var meats = new HashSet<int>();
            foreach (var meat in Unary.Mod.GetSheep().Concat(Unary.Mod.GetDeer().Concat(Unary.Mod.GetBoar())))
            {
                meats.Add(meat);
            }

            Meat.Clear();
            
            foreach (var tile in Unary.GameState.Map.GetTilesInRange(Unary.GameState.MyPosition, 10))
            {
                foreach (var unit in tile.Units.Where(u => u.Targetable))
                {
                    if (meats.Contains(unit[ObjectData.BASE_TYPE]))
                    {
                        Meat.Add(unit);
                    }
                }
            }

            ManagePopulation();
            ManageGatherers();
            ManageDropsites();
        }

        private void ManagePopulation()
        {
            var house = Unary.GameState.GetUnitType(70);

            //villager.Train((int)Math.Round(0.6 * Unary.GameState.MyPlayer.GetFact(FactId.POPULATION_CAP)), ConcurrentVillagers, Priority.VILLAGER);

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
                Unary.OldProductionManager.Build(house, 1000, pending, Priority.HOUSING);
            }
        }

        private void ManageGatherers()
        {
            Unary.GameState.SetStrategicNumber(StrategicNumber.CAP_CIVILIAN_EXPLORERS, 0);
            Unary.GameState.SetStrategicNumber(StrategicNumber.ENABLE_BOAR_HUNTING, 0);
            Unary.GameState.SetStrategicNumber(StrategicNumber.LIVESTOCK_TO_TOWN_CENTER, 1);

            Unary.GameState.SetStrategicNumber(StrategicNumber.MAXIMUM_WOOD_DROP_DISTANCE, -2);
            Unary.GameState.SetStrategicNumber(StrategicNumber.MAXIMUM_GOLD_DROP_DISTANCE, -2);
            Unary.GameState.SetStrategicNumber(StrategicNumber.MAXIMUM_STONE_DROP_DISTANCE, -2);
            Unary.GameState.SetStrategicNumber(StrategicNumber.MAXIMUM_FOOD_DROP_DISTANCE, -2);
            Unary.GameState.SetStrategicNumber(StrategicNumber.MAXIMUM_HUNT_DROP_DISTANCE, -2);

            Unary.GameState.SetStrategicNumber(StrategicNumber.FOOD_GATHERER_PERCENTAGE, 0);
            Unary.GameState.SetStrategicNumber(StrategicNumber.WOOD_GATHERER_PERCENTAGE, 0);
            Unary.GameState.SetStrategicNumber(StrategicNumber.GOLD_GATHERER_PERCENTAGE, 0);
            Unary.GameState.SetStrategicNumber(StrategicNumber.STONE_GATHERER_PERCENTAGE, 0);

            double pop = Unary.GameState.MyPlayer.CivilianPopulation;

            double food = MinFoodGatherers;
            double wood = MinWoodGatherers;
            double gold = MinGoldGatherers;
            double stone = MinStoneGatherers;

            pop -= food;
            pop -= wood;
            pop -= gold;
            pop -= stone;

            if (pop > 0)
            {
                food += pop * ExtraFoodPercentage / 100d;
                wood += pop * ExtraWoodPercentage / 100d;
                gold += pop * ExtraGoldPercentage / 100d;
                stone += pop * ExtraStonePercentage / 100d;
            }

            FoodGatherers = (int)Math.Round(food);
            WoodGatherers = (int)Math.Round(wood);
            GoldGatherers = (int)Math.Round(gold);
            StoneGatherers = (int)Math.Round(stone);
        }

        private void ManageDropsites()
        {
            var tc = Unary.GameState.GetUnitType(109);
            var tc_foundation = Unary.GameState.GetUnitType(621);
            var mill = Unary.GameState.GetUnitType(68);
            var lumber_camp = Unary.GameState.GetUnitType(562);

            Unary.GameState.SetStrategicNumber(StrategicNumber.MILL_MAX_DISTANCE, 30);

            if (Unary.GameState.MyPlayer.Units.Count(u => u.Targetable && u[ObjectData.BASE_TYPE] == tc.Id) < MaxTownCenters)
            {
                Unary.Log.Info("Building TC");
                var count = tc.CountTotal + tc_foundation.CountTotal;
                var pending = tc.Pending + tc_foundation.Pending;

                if (count < MaxTownCenters && pending < 1)
                {
                    Unary.OldProductionManager.Build(tc, MaxTownCenters, 1, Priority.DROPSITE);
                }
            }

            if (FoodGatherers > 0)
            {
                if (mill.CountTotal < 1 && lumber_camp.Count > 0)
                {
                    if (Unary.GameState.MyPlayer.CivilianPopulation >= 11 && Unary.GameState.GetResourceFound(Resource.FOOD) && Unary.GameState.GetDropsiteMinDistance(Resource.FOOD) < 200)
                    {
                        Unary.Log.Info("Building mill");
                        Unary.OldProductionManager.Build(mill, 100, 1, Priority.DROPSITE);
                    }
                }
            }
        }
    }
}
