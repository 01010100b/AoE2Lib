using AoE2Lib;
using AoE2Lib.Bots.GameElements;
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
        public int MaxTownCenters { get; set; } = 1;
        public int ConcurrentVillagers { get; set; } = 3;
        
        private readonly Dictionary<Unit, List<GatherOperation>> GatherOperations = new Dictionary<Unit, List<GatherOperation>>();
        private int FoodGatherers { get; set; } = 0;
        private int WoodGatherers { get; set; } = 0;
        private int GoldGatherers { get; set; } = 0;
        private int StoneGatherers { get; set; } = 0;

        public EconomyManager(Unary unary) : base(unary)
        {

        }

        internal override void Update()
        {
            ManagePopulation();
            ManageGatherers();
            ManageDropsites();
            ManageGatherOperations();
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

        private void ManageGatherOperations()
        {
            // assign/remove operations to dropsites

            var dropsites = new HashSet<Unit>();
            foreach (var unit in GatherOperations.Keys)
            {
                dropsites.Add(unit);
            }

            foreach (var unit in Unary.GameState.MyPlayer.Units.Where(u => u.Targetable))
            {
                var type = unit[ObjectData.BASE_TYPE];
                if (type == 109 || type == 562 || type == 584 || type == 68)
                {
                    dropsites.Add(unit);
                }
            }

            foreach (var site in dropsites)
            {
                if (!site.Targetable)
                {
                    // dropsite disappeared, stop all gather operations here
                    if (GatherOperations.TryGetValue(site, out List<GatherOperation> ops))
                    {
                        foreach (var op in ops)
                        {
                            op.Stop();
                        }

                        ops.Clear();
                        GatherOperations.Remove(site);
                    }
                }
                else
                {
                    if (!GatherOperations.ContainsKey(site))
                    {
                        GatherOperations.Add(site, new List<GatherOperation>());
                    }

                    var type = site[ObjectData.BASE_TYPE];
                    if (type == 109 || type == 68)
                    {
                        var op = GatherOperations[site].FirstOrDefault(o => o.Resource == Resource.FOOD);
                        if (op == null)
                        {
                            op = new GatherOperation(Unary, site, Resource.FOOD);
                            GatherOperations[site].Add(op);
                        }
                    }
                    if (type == 109 || type == 562)
                    {
                        var op = GatherOperations[site].FirstOrDefault(o => o.Resource == Resource.WOOD);
                        if (op == null)
                        {
                            op = new GatherOperation(Unary, site, Resource.WOOD);
                            GatherOperations[site].Add(op);
                        }
                    }
                    if (type == 109 || type == 584)
                    {
                        foreach (var res in new[] { Resource.GOLD, Resource.STONE })
                        {
                            var op = GatherOperations[site].FirstOrDefault(o => o.Resource == res);
                            if (op == null)
                            {
                                op = new GatherOperation(Unary, site, res);
                                GatherOperations[site].Add(op);
                            }
                        }
                    }
                }
            }

            // remove excess vills

            foreach (var op in GatherOperations.Values.SelectMany(g => g))
            {
                if (op.UnitCount > op.UnitCapacity)
                {
                    var free = op.Units.FirstOrDefault(u => u[ObjectData.CARRY] == 0);
                    if (free != null && Unary.Rng.NextDouble() < 0.1)
                    {
                        free.Target(op.Dropsite);
                        op.RemoveUnit(free);
                    }
                }
            }

            // assign new vills

            var free_vills = Operation.GetFreeUnits(Unary).Where(u => u[ObjectData.CMDID] == (int)CmdId.VILLAGER).ToList();
            if (free_vills.Count == 0)
            {
                return;
            }

            var resources = new[] { Resource.WOOD, Resource.FOOD, Resource.GOLD, Resource.STONE };
            foreach (var resource in resources)
            {
                if (free_vills.Count == 0)
                {
                    break;
                }

                var min_gatherers = 0;
                switch (resource)
                {
                    case Resource.WOOD: min_gatherers = WoodGatherers; break;
                    case Resource.FOOD: min_gatherers = FoodGatherers; break;
                    case Resource.GOLD: min_gatherers = GoldGatherers; break;
                    case Resource.STONE: min_gatherers = StoneGatherers; break;
                }

                var gatherers = 0;
                var open_ops = new List<GatherOperation>();
                foreach (var op in GatherOperations.Values.SelectMany(g => g).Where(o => o.Resource == resource))
                {
                    gatherers += op.UnitCount;
                    if (op.UnitCount < op.UnitCapacity)
                    {
                        open_ops.Add(op);
                    }
                }

                if (open_ops.Count == 0)
                {
                    continue;
                }

                if (gatherers < min_gatherers)
                {
                    var op = open_ops[0];
                    var vill = free_vills[free_vills.Count - 1];
                    op.AddUnit(vill);

                    free_vills.RemoveAt(free_vills.Count - 1);
                }
            }

            Unary.Log.Debug($"Gather operations: {GatherOperations.SelectMany(g => g.Value).Count()}");
        }
    }
}
