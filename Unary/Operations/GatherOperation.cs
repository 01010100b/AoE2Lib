﻿using AoE2Lib;
using AoE2Lib.Bots;
using AoE2Lib.Bots.GameElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Unary.Operations
{
    class GatherOperation : Operation
    {
        public override Position Position => Dropsite.Position;
        public int UnitCapacity { get; private set; } = 0;

        public readonly Unit Dropsite;
        public readonly Resource Resource;

        public GatherOperation(Unary unary, Unit dropsite, Resource resource) : base(unary)
        {
            if (dropsite == null)
            {
                throw new ArgumentNullException(nameof(dropsite));
            }
            else if (resource != Resource.WOOD && resource != Resource.FOOD && resource != Resource.GOLD && resource != Resource.STONE)
            {
                throw new ArgumentOutOfRangeException(nameof(resource));
            }

            Dropsite = dropsite;
            Resource = resource;
        }

        public override void Update()
        {
            List<Unit> resources;
            if (Resource == Resource.WOOD)
            {
                resources = GetWoodResources();
                UnitCapacity = Math.Min(6, resources.Count * 2);
            }
            else
            {
                throw new NotImplementedException();
            }

            DoGathering(Units, resources);

            Unary.Log.Debug($"Gathering {Resource} at {Dropsite.UnitType.Id}({Dropsite.Position}) with {UnitCount}/{UnitCapacity} units for {resources.Count} resources");
        }

        private List<Unit> GetWoodResources()
        {
            var range = 4;
            if (Dropsite.UnitType.Id == 109)
            {
                range = 7;
            }

            var trees = new List<Unit>();
            foreach (var tile in Unary.GameState.Map.GetTilesInRange(Dropsite.Position.PointX, Dropsite.Position.PointY, range + 1))
            {
                foreach (var unit in tile.Units.Where(u => u.Targetable && u[ObjectData.CLASS] == (int)UnitClass.Tree))
                {
                    if (unit.Position.DistanceTo(Dropsite.Position) <= range)
                    {
                        trees.Add(unit);
                        unit.RequestUpdate();
                    }
                }
            }

            return trees;
        }

        private void DoGathering(List<Unit> units, List<Unit> resources)
        {
            var assigned = new Dictionary<int, int>();
            var unassigned_gatherers = new List<Unit>();

            resources.Sort((a, b) => a.Position.DistanceTo(Dropsite.Position).CompareTo(b.Position.DistanceTo(Dropsite.Position)));
            foreach (var resource in resources)
            {
                assigned.Add(resource.Id, 0);
            }
            
            foreach (var gatherer in units)
            {
                var target_id = gatherer[ObjectData.TARGET_ID];
                if (target_id <= 0 || assigned.ContainsKey(target_id) == false)
                {
                    unassigned_gatherers.Add(gatherer);
                }
                else
                {
                    assigned[target_id]++;
                }

                if (gatherer.Position.DistanceTo(Dropsite.Position) > 10)
                {
                    if (Unary.Rng.NextDouble() < 0.1)
                    {
                        gatherer.Target(Dropsite.Position, UnitAction.MOVE);
                    }
                }
            }

            foreach (var resource in resources)
            {
                var working = 0;
                if (assigned.TryGetValue(resource.Id, out int a))
                {
                    working = a;
                }

                for (int i = 2; i > working; i--)
                {
                    if (unassigned_gatherers.Count == 0)
                    {
                        break;
                    }

                    var gatherer = unassigned_gatherers[unassigned_gatherers.Count - 1];
                    gatherer.Target(resource);
                    unassigned_gatherers.RemoveAt(unassigned_gatherers.Count - 1);
                }
            }
        }
    }
}
