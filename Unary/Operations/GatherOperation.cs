using AoE2Lib;
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
        public override int UnitCapacity => _UnitCapacity;
        private int _UnitCapacity { get; set; } = 0;

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
            if (Resource == Resource.WOOD)
            {
                DoWood();
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        private void DoWood()
        {
            var range = 3;
            var trees = new List<Unit>();
            foreach (var tile in Unary.GameState.Map.GetTilesInRange(Dropsite.Position.PointX, Dropsite.Position.PointY, range + 1))
            {
                foreach (var unit in tile.Units.Where(u => u.Targetable && u[ObjectData.CLASS] == (int)UnitClass.Tree && u[ObjectData.HITPOINTS] > 0))
                {
                    if (unit.Position.DistanceTo(Dropsite.Position) <= range)
                    {
                        trees.Add(unit);
                    }
                }
            }

            _UnitCapacity = Math.Max(6, trees.Count);

            DoGathering(trees);
        }

        private void DoGathering(List<Unit> resources)
        {
            resources.Sort((a, b) => a.Position.DistanceTo(Dropsite.Position).CompareTo(b.Position.DistanceTo(Dropsite.Position)));

            var assigned = new Dictionary<int, int>();
            var unassigned_gatherers = new List<Unit>();
            foreach (var gatherer in Units)
            {
                var target_id = gatherer[ObjectData.TARGET_ID];
                if (!assigned.ContainsKey(target_id))
                {
                    assigned.Add(target_id, 0);
                }

                assigned[target_id]++;

                if (gatherer.Position.DistanceTo(Dropsite.Position) > 10)
                {
                    if (Unary.Rng.NextDouble() < 0.1)
                    {
                        gatherer.TargetPosition(Dropsite.Position, UnitAction.MOVE);
                    }
                }
                else if (target_id <= 0)
                {
                    unassigned_gatherers.Add(gatherer);
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
                    gatherer.TargetUnit(resource);
                    unassigned_gatherers.RemoveAt(unassigned_gatherers.Count - 1);
                }
            }
        }
    }
}
