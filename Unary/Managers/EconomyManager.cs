using AoE2Lib;
using AoE2Lib.Bots;
using AoE2Lib.Bots.GameElements;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using Unary.UnitControllers;

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

        private readonly List<Unit> Farms = new();
        private readonly List<Unit> Dropsites = new();
        private int FoodGatherers { get; set; } = 0;
        private int WoodGatherers { get; set; } = 0;
        private int GoldGatherers { get; set; } = 0;
        private int StoneGatherers { get; set; } = 0;

        public EconomyManager(Unary unary) : base(unary)
        {

        }

        public int GetMinimumGatherers(Resource resource)
        {
            return resource switch
            {
                Resource.FOOD => FoodGatherers,
                Resource.WOOD => WoodGatherers,
                Resource.GOLD => GoldGatherers,
                Resource.STONE => StoneGatherers,
                _ => throw new ArgumentOutOfRangeException(nameof(resource))
            };
        }

        public int GetMaximumGatherers(Resource resource)
        {
            var min = GetMinimumGatherers(resource);

            return Math.Max(min + 2, (int)Math.Round(min * 1.1));
        }

        public IEnumerable<Unit> GetFarms()
        {
            return Farms;
        }

        public IEnumerable<Unit> GetDropsites(Resource resource)
        {
            return resource switch
            {
                Resource.WOOD => Dropsites.Where(u => u[ObjectData.BASE_TYPE] == Unary.Mod.TownCenter || u[ObjectData.BASE_TYPE] == Unary.Mod.LumberCamp),
                Resource.FOOD => Dropsites.Where(u => u[ObjectData.BASE_TYPE] == Unary.Mod.TownCenter || u[ObjectData.BASE_TYPE] == Unary.Mod.Mill),
                Resource.GOLD or Resource.STONE => Dropsites.Where(u => u[ObjectData.BASE_TYPE] == Unary.Mod.TownCenter || u[ObjectData.BASE_TYPE] == Unary.Mod.MiningCamp),
                _ => throw new ArgumentOutOfRangeException(nameof(resource)),
            };
        }

        public IEnumerable<KeyValuePair<Tile, Unit>> GetGatherableResources(Resource resource, Unit dropsite)
        {
            var deltas = new[] { new Point(-1, 0), new Point(1, 0), new Point(0, -1), new Point(0, 1) };

            var range = 30;
            var type = UnitClass.Tree;
            type = resource switch
            {
                Resource.WOOD => UnitClass.Tree,
                Resource.FOOD => UnitClass.BerryBush,
                Resource.GOLD => UnitClass.GoldMine,
                Resource.STONE => UnitClass.StoneMine,
                _ => throw new ArgumentOutOfRangeException(nameof(resource)),
            };

            foreach (var tile in Unary.GameState.Map.GetTilesInRange(dropsite.Position.PointX, dropsite.Position.PointY, range))
            {
                foreach (var unit in tile.Units.Where(u => u.Targetable))
                {
                    if (unit[ObjectData.CLASS] == (int)type)
                    {
                        foreach (var delta in deltas)
                        {
                            var x = tile.X + delta.X;
                            var y = tile.Y + delta.Y;

                            if (Unary.GameState.Map.IsOnMap(x, y))
                            {
                                var t = Unary.GameState.Map.GetTile(x, y);

                                if (!Unary.BuildingManager.IsObstructed(t))
                                {
                                    yield return new KeyValuePair<Tile, Unit>(t, unit);
                                }
                            }
                        }
                    }
                }
            }
        }

        internal override void Update()
        {
            Farms.Clear();
            Dropsites.Clear();

            foreach (var unit in Unary.GameState.MyPlayer.Units.Where(u => u.Targetable))
            {
                var type = unit[ObjectData.BASE_TYPE];
                if (type == Unary.Mod.TownCenter || type == Unary.Mod.LumberCamp || type == Unary.Mod.MiningCamp || type == Unary.Mod.Mill || type == Unary.Mod.Dock)
                {
                    Dropsites.Add(unit);
                }
                else if (type == Unary.Mod.Farm)
                {
                    Farms.Add(unit);
                }
            }

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
            var mining_camp = Unary.GameState.GetUnitType(584);
            var farm = Unary.GameState.GetUnitType(50);

            Unary.GameState.SetStrategicNumber(StrategicNumber.MILL_MAX_DISTANCE, 30);

            if (Unary.GameState.MyPlayer.Units.Count(u => u.Targetable && u[ObjectData.BASE_TYPE] == tc.Id) < MaxTownCenters)
            {
                Unary.Log.Info("Building TC");
                var count = tc.CountTotal + tc_foundation.CountTotal;
                var pending = tc.Pending + tc_foundation.Pending;

                if (count < MaxTownCenters && pending < 1)
                {
                    tc.BuildNormal(MaxTownCenters, 1, Priority.DROPSITE);
                }
            }

            if (WoodGatherers > 0)
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
            
            if (FoodGatherers > 0)
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
                    var needed_farms = FoodGatherers;

                    Unary.Log.Info($"I have {farm.CountTotal} farms and I want {needed_farms}");

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

            if (GoldGatherers > 0)
            {
                gathered.Add(Resource.GOLD);
            }

            if (StoneGatherers > 0)
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
    }
}
