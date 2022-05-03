﻿using AoE2Lib;
using AoE2Lib.Bots;
using AoE2Lib.Bots.GameElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unary.UnitControllers.BuildingControllers;
using Unary.UnitControllers.VillagerControllers;

namespace Unary.UnitControllers.VillagerControllers
{
    class GathererController : VillagerController
    {
        public Resource Resource { get; private set; } = Resource.NONE;
        public Unit Target { get; private set; } = null;
        public Tile Tile { get; private set; } = null;
        public DropsiteController DropsiteController { get; private set; } = null;

        private int LastTargetTick { get; set; } = 0;

        public GathererController(Unit unit, Unary unary) : base(unit, unary)
        {
            LastTargetTick = Unary.GameState.Tick;
        }

        protected override void VillagerTick()
        {
            if (Resource != Resource.WOOD && Resource != Resource.FOOD && Resource != Resource.GOLD && Resource != Resource.STONE)
            {
                ChooseResource();
            }

            if (Resource == Resource.FOOD)
            {
                if (CheckFoodAlternatives())
                {
                    return;
                }
            }

            if (Target == null || GetHashCode() % 23 == Unary.GameState.Tick % 23)
            {
                ChooseTarget();
            }
            
            if (Target != null)
            {
                Gather();
            }
        }
        
        private void ChooseResource()
        {
            if (Resource == Resource.NONE)
            {
                foreach (var res in new[] { Resource.WOOD, Resource.FOOD, Resource.GOLD, Resource.STONE })
                {
                    var min = Unary.StrategyManager.GetMinimumGatherers(res);
                    var max = Unary.StrategyManager.GetMaximumGatherers(res);
                    var current = Unary.OldUnitsManager.GetControllers<GathererController>().Count(u => u.Resource == res);
                    if (res == Resource.FOOD)
                    {
                        current += Unary.OldUnitsManager.GetControllers<FarmerController>().Count;
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
            }

            if (Resource == Resource.NONE)
            {
                Resource = Resource.FOOD;
            }

            Unary.Log.Debug($"Gatherer {Unit.Id} choose resource {Resource}");
        }

        private bool CheckFoodAlternatives()
        {
            var hunters = Unary.OldUnitsManager.GetControllers<HunterController>().Count;

            if (hunters < Unary.Settings.MaxHunters)
            {
                new HunterController(Unit, Unary);

                return true;
            }

            if (Unary.GameState.Tick - LastTargetTick > 10)
            {
                var farmers = Unary.OldUnitsManager.GetControllers<FarmerController>().Count(c => c.Tile == null);

                if (farmers < Unary.Settings.MaxWaitingFarmers)
                {
                    new FarmerController(Unit, Unary);

                    return true;
                }
            }

            return false;
        }

        private void ChooseTarget()
        {
            var tile_occupancy = new Dictionary<Tile, int>();
            var dropsite_occupancy = new Dictionary<DropsiteController, int>();
            foreach (var gatherer in Unary.OldUnitsManager.GetControllers<GathererController>().Where(g => g != this))
            {
                if (gatherer.Tile != null)
                {
                    if (!tile_occupancy.ContainsKey(gatherer.Tile))
                    {
                        tile_occupancy.Add(gatherer.Tile, 0);
                    }

                    tile_occupancy[gatherer.Tile]++;
                }

                if (gatherer.DropsiteController != null)
                {
                    if (!dropsite_occupancy.ContainsKey(gatherer.DropsiteController))
                    {
                        dropsite_occupancy.Add(gatherer.DropsiteController, 0);
                    }

                    dropsite_occupancy[gatherer.DropsiteController]++;
                }
            }

            var best_cost = double.MaxValue;
            var found_close = false;

            foreach (var dropsite in Unary.OldUnitsManager.GetControllers<DropsiteController>())
            {
                if (dropsite_occupancy.TryGetValue(dropsite, out int occupancy))
                {
                    if (occupancy >= dropsite.MaxOccupancy)
                    {
                        continue;
                    }
                }

                foreach (var res in dropsite.GetGatherableResources(Resource))
                {
                    if (!res.Value.Targetable)
                    {
                        continue;
                    }

                    if (tile_occupancy.TryGetValue(res.Key, out occupancy))
                    {
                        if (occupancy >= 2)
                        {
                            continue;
                        }
                    }

                    var distance = dropsite.GetPathDistance(res.Key);

                    if (distance <= 2)
                    {
                        found_close = true;
                    }

                    var cost = distance + (Unary.Settings.VillagerRetaskDistanceCost * Unit.Position.DistanceTo(res.Key.Position));

                    if (Tile == res.Key)
                    {
                        cost -= 1;
                    }

                    if (DropsiteController != dropsite)
                    {
                        cost += 1;
                    }

                    if (cost <= best_cost)
                    {
                        Tile = res.Key;
                        Target = res.Value;
                        DropsiteController = dropsite;
                        best_cost = cost;
                    }
                }
            }

            if (!found_close)
            {
                Unary.OldProductionManager.RequestDropsite(Resource);
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
                    var mill = Unary.GameState.GetUnitType(Unary.Mod.Mill);
                    if (mill.Count > 0)
                    {
                        new FarmerController(Unit, Unary);

                        return;
                    }
                }

                Unary.OldProductionManager.RequestDropsite(Resource);

                if (Unary.Rng.NextDouble() < 0.1)
                {
                    Resource = Resource.NONE;
                }
            }
        }

        private void Gather()
        {
            if (!Target.Targetable)
            {
                Target = null;
                Tile = null;
                DropsiteController = null;

                return;
            }

            var target = Target;
            if (DropsiteController.Unit[ObjectData.STATUS] == 0)
            {
                target = DropsiteController.Unit;
            }

            if (Unit.GetTarget() != target)
            {
                Unit.Target(target);
            }

            target.RequestUpdate();
            LastTargetTick = Unary.GameState.Tick;
        }
    }
}
