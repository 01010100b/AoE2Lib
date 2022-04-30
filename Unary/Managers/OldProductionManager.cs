using AoE2Lib;
using AoE2Lib.Bots.GameElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unary.UnitControllers;
using Unary.UnitControllers.VillagerControllers;
using static Unary.Managers.ProductionManager;

namespace Unary.Managers
{
    class OldProductionManager : Manager
    {
        

        private FarmerController FarmRequest { get; set; }
        private readonly Dictionary<Resource, int> DropsiteRequestTicks = new();

        public OldProductionManager(Unary unary) : base(unary)
        {
            DropsiteRequestTicks.Add(Resource.WOOD, -1000);
            DropsiteRequestTicks.Add(Resource.FOOD, -1000);
            DropsiteRequestTicks.Add(Resource.GOLD, -1000);
            DropsiteRequestTicks.Add(Resource.STONE, -1000);
        }

        public void RequestFarm(FarmerController controller)
        {
            if (FarmRequest == null || controller.Unit.FirstUpdateGameTime < FarmRequest.Unit.FirstUpdateGameTime)
            {
                FarmRequest = controller;
            }
        }

        public void RequestDropsite(Resource resource)
        {
            if (!DropsiteRequestTicks.ContainsKey(resource))
            {
                throw new ArgumentOutOfRangeException(nameof(resource));
            }

            DropsiteRequestTicks[resource] = Unary.GameState.Tick;
        }

        public void Build(UnitType building, int max_count = 10000, int max_pending = 10000, int priority = 10, bool blocking = true)
        {
            var placements = Unary.OldBuildingManager.GetBuildingPlacements(building).ToList();

            if (placements.Count > 100)
            {
                placements.Sort((a, b) => a.Position.DistanceTo(Unary.GameState.MyPosition).CompareTo(b.Position.DistanceTo(Unary.GameState.MyPosition)));
                placements.RemoveRange(100, placements.Count - 100);
            }

            placements.Sort((a, b) => GetPlacementScore(building, b).CompareTo(GetPlacementScore(building, a)));

            if (building[ObjectData.BASE_TYPE] == Unary.Mod.TownCenter)
            {
                building = Unary.GameState.GetUnitType(Unary.Mod.TownCenterFoundation);
            }

            building.OldBuild(placements, max_count, max_pending, priority, blocking);
        }

        public void Train(UnitType unit, int max_count = 10000, int max_pending = 10000, int priority = 10, bool blocking = true)
        {
            unit.OldTrain(max_count, max_pending, priority, blocking);
        }

        public void Research(Technology technology, int priority = 10, bool blocking = true)
        {
            technology.OldResearch(priority, blocking);
        }

        internal override void Update()
        {
            PerformFarmRequest();
            PerformDropsiteRequest();
        }

        private void PerformFarmRequest()
        {
            if (FarmRequest == null)
            {
                return;
            }

            var farm = Unary.GameState.GetUnitType(Unary.Mod.Farm);
            var mill = Unary.GameState.GetUnitType(Unary.Mod.Mill);

            if (FarmRequest.Tile == null)
            {
                var placements = Unary.OldBuildingManager.GetFarmPlacements();

                if (placements.Count > 0)
                {
                    Unary.Log.Info("Building new farm");
                    farm.OldBuild(placements, 100, 3, Priority.FARM);
                }

                if (placements.Count < 3)
                {
                    Unary.Log.Info("Building mill");
                    Unary.OldProductionManager.Build(mill, 100, 1, Priority.FARM + 1);
                }
            }
            else
            {
                var size = Unary.Mod.GetBuildingSize(Unary.Mod.Farm);
                if (Unary.OldBuildingManager.CanBuildAt(size, size, FarmRequest.Tile, true))
                {
                    Unary.Log.Info($"Refreshing farm at {FarmRequest.Tile.Position}");
                    farm.OldBuild(new[] { FarmRequest.Tile }, 100, 3, Priority.FARM);
                }
                else
                {
                    Unary.Log.Debug($"Can not build farm at {FarmRequest.Tile.Position}");
                }
            }

            FarmRequest = null;
        }

        private void PerformDropsiteRequest()
        {
            foreach (var resource in new[] { Resource.WOOD, Resource.FOOD, Resource.GOLD, Resource.STONE })
            {
                var site = Unary.GameState.GetUnitType(Unary.Mod.LumberCamp);

                switch (resource)
                {
                    case Resource.FOOD: site = Unary.GameState.GetUnitType(Unary.Mod.Mill); break;
                    case Resource.WOOD: site = Unary.GameState.GetUnitType(Unary.Mod.LumberCamp); break;
                    case Resource.GOLD: site = Unary.GameState.GetUnitType(Unary.Mod.MiningCamp); break;
                    case Resource.STONE: site = Unary.GameState.GetUnitType(Unary.Mod.MiningCamp); break;
                }

                if (!site.Updated)
                {
                    continue;
                }

                if (site.Pending > 0)
                {
                    DropsiteRequestTicks[resource] = -1000;

                    continue;
                }

                if (site.Pending == 0 && Unary.GameState.Tick - DropsiteRequestTicks[resource] < 100)
                {
                    var placements = Unary.OldBuildingManager.GetDropsitePlacements(resource);
                    placements.Sort((a, b) => b.Value.CompareTo(a.Value));
                    site.OldBuild(placements.Select(p => p.Key), 100, 1, Priority.DROPSITE);
                    Unary.Log.Debug($"Found {placements.Count} placements for {resource}");

                    continue;
                }
            }
        }

        private double GetPlacementScore(UnitType building, Tile tile)
        {
            var score = -tile.Position.DistanceTo(Unary.GameState.MyPosition);

            if (building[ObjectData.BASE_TYPE] == Unary.Mod.Mill || building[ObjectData.BASE_TYPE] == Unary.Mod.TownCenter)
            {
                var deltas = OldBuildingManager.TC_FARM_DELTAS;
                if (building[ObjectData.BASE_TYPE] == Unary.Mod.Mill)
                {
                    deltas = OldBuildingManager.MILL_FARM_DELTAS;
                }

                var farm_size = Unary.Mod.GetBuildingSize(Unary.Mod.Farm);

                foreach (var delta in deltas)
                {
                    var x = tile.X + delta.X;
                    var y = tile.Y + delta.Y;

                    if (Unary.GameState.Map.IsOnMap(x, y))
                    {
                        var t = Unary.GameState.Map.GetTile(x, y);
                        
                        if (Unary.OldBuildingManager.CanBuildAt(farm_size, farm_size, t))
                        {
                            score += 4;
                        }
                    }
                }
            }

            return score;
        }
    }
}
