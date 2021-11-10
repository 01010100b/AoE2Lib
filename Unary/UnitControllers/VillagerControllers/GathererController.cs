using AoE2Lib;
using AoE2Lib.Bots.GameElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unary.UnitControllers.VillagerControllers;

namespace Unary.UnitControllers.VillagerControllers
{
    class GathererController : VillagerController
    {
        public Resource Resource { get; set; } = Resource.NONE;
        public Unit Target { get; private set; } = null;
        public Tile Tile { get; private set; } = null;
        public Unit Dropsite { get; private set; } = null;

        public GathererController(Unit unit, Unary unary) : base(unit, unary)
        {

        }

        protected override void VillagerTick()
        {
            if (Resource != Resource.WOOD && Resource != Resource.FOOD && Resource != Resource.GOLD && Resource != Resource.STONE)
            {
                ChooseResource();
            }
            else if (Target == null || Tile == null || Dropsite == null || GetHashCode() % 50 == Unary.GameState.Tick % 50)
            {
                ChooseTarget();
            }
            else
            {
                Gather();
            }
        }

        private void ChooseResource()
        {
            foreach (var res in new[] {Resource.WOOD, Resource.FOOD, Resource.GOLD, Resource.STONE })
            {
                var min = Unary.EconomyManager.GetMinimumGatherers(res);
                var max = Unary.EconomyManager.GetMaximumGatherers(res);
                var current = Unary.UnitsManager.GetControllers<GathererController>().Count(u => u.Resource == res);
                if (res == Resource.FOOD)
                {
                    current += Unary.UnitsManager.GetControllers<FarmerController>().Count;
                }

                if (current < min)
                {
                    Resource = res;
                    
                    break;
                }
                else if (current < max)
                {
                    Resource = res;
                }
            }

            Unary.Log.Debug($"Gatherer {Unit.Id} choose resource {Resource}");
        }

        private void ChooseTarget()
        {
            var tile_occupancy = new Dictionary<Tile, int>();
            var dropsite_occupancy = new Dictionary<Unit, int>();
            foreach (var gatherer in Unary.UnitsManager.GetControllers<GathererController>().Where(g => g != this))
            {
                if (gatherer.Tile != null)
                {
                    if (!tile_occupancy.ContainsKey(gatherer.Tile))
                    {
                        tile_occupancy.Add(gatherer.Tile, 0);
                    }

                    tile_occupancy[gatherer.Tile]++;
                }

                if (gatherer.Dropsite != null)
                {
                    if (!dropsite_occupancy.ContainsKey(gatherer.Dropsite))
                    {
                        dropsite_occupancy.Add(gatherer.Dropsite, 0);
                    }

                    dropsite_occupancy[gatherer.Dropsite]++;
                }
            }

            var best_distance = double.MaxValue;
            var request = true;

            foreach (var dropsite in Unary.EconomyManager.GetDropsites(Resource))
            {
                if (dropsite_occupancy.TryGetValue(dropsite, out int count))
                {
                    if (count >= 7 && dropsite[ObjectData.BASE_TYPE] != Unary.Mod.TownCenter)
                    {
                        continue;
                    }
                }

                foreach (var res in Unary.EconomyManager.GetGatherableResources(Resource, dropsite))
                {
                    if (tile_occupancy.TryGetValue(res.Key, out int occ))
                    {
                        if (occ >= 2)
                        {
                            continue;
                        }
                    }

                    if (Resource == Resource.FOOD && res.Value.Position.DistanceTo(Unary.GameState.MyPosition) > 30)
                    {
                        continue;
                    }

                    var distance = dropsite.Position.DistanceTo(res.Key.Position);
                    var max = 4;
                    if (Resource == Resource.WOOD)
                    {
                        max = 3;
                    }

                    if (distance <= max)
                    {
                        request = false;
                    }

                    distance += 0.05 * Unit.Position.DistanceTo(res.Key.Position);

                    if (distance < best_distance)
                    {
                        Tile = res.Key;
                        Target = res.Value;
                        Dropsite = dropsite;
                        best_distance = distance;
                    }
                }
            }

            if (Target != null)
            {
                Unary.Log.Debug($"Gatherer {Unit.Id} choose target {Target.Id}");

                if (request)
                {
                    Unary.ProductionManager.RequestDropsite(this);
                }
            }
            else
            {
                Unary.Log.Debug($"Gatherer {Unit.Id} failed choosing target");

                if (Resource == Resource.FOOD)
                {
                    var mill = Unary.GameState.GetUnitType(Unary.Mod.Mill);
                    if (mill.Count > 0)
                    {
                        new FarmerController(Unit, Unary);
                    }
                }
            }
        }

        private void Gather()
        {
            if (!Target.Targetable)
            {
                Target = null;
                Tile = null;
                Dropsite = null;

                return;
            }

            var target = Target;
            if (Dropsite[ObjectData.STATUS] == 0)
            {
                target = Dropsite;
            }

            if (Unit[ObjectData.TARGET_ID] != target.Id)
            {
                Unit.Target(target);
            }

            Target.RequestUpdate();
            Dropsite.RequestUpdate();
        }
    }
}
