using AoE2Lib;
using AoE2Lib.Bots.GameElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unary.UnitControllers
{
    class GathererController : UnitController
    {
        public Resource Resource { get; set; } = Resource.NONE;
        public Unit Target { get; private set; } = null;
        public Tile Tile { get; private set; } = null;
        public Unit Dropsite { get; private set; } = null;

        public GathererController(Unit unit, Unary unary) : base(unit, unary)
        {

        }

        protected override void Tick()
        {
            if (Resource != Resource.WOOD && Resource != Resource.FOOD && Resource != Resource.GOLD && Resource != Resource.STONE)
            {
                ChooseResource();
            }
            else if (Target == null || Tile == null || Dropsite == null)
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
            foreach (var res in new[] {Resource.WOOD, Resource.FOOD, Resource.GOLD, Resource.STONE})
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
            var occupancy = new Dictionary<Tile, int>();
            foreach (var gatherer in Unary.UnitsManager.GetControllers<GathererController>())
            {
                if (gatherer.Tile != null)
                {
                    if (!occupancy.ContainsKey(gatherer.Tile))
                    {
                        occupancy.Add(gatherer.Tile, 0);
                    }

                    occupancy[gatherer.Tile]++;
                }
            }

            var best_distance = double.MaxValue;

            foreach (var dropsite in Unary.EconomyManager.GetDropsites(Resource))
            {
                foreach (var res in Unary.EconomyManager.GetGatherableResources(Resource, dropsite))
                {
                    if (occupancy.TryGetValue(res.Key, out int occ))
                    {
                        if (occ >= 2)
                        {
                            continue;
                        }
                    }

                    var distance = dropsite.Position.DistanceTo(res.Key.Position);
                    distance += Unit.Position.DistanceTo(res.Key.Position) / 10;
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
            }
            else
            {
                Unary.Log.Debug($"Gatherer {Unit.Id} failed choosing target");

                if (Resource == Resource.FOOD)
                {
                    var ctrl = new FarmerController(Unit, Unary);
                    Unary.UnitsManager.SetController(Unit, ctrl);
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

            if (Unit[ObjectData.TARGET_ID] != Target.Id)
            {
                Unit.Target(Target);
            }

            Target.RequestUpdate();
        }
    }
}
