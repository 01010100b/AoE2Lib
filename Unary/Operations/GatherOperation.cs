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
            if (Resource == Resource.WOOD)
            {
                DoWood();
            }
            else if (Resource == Resource.GOLD)
            {
                DoGold();
            }
            else if (Resource == Resource.STONE)
            {
                DoStone();
            }
            else
            {
                throw new NotImplementedException();
            }

            Unary.Log.Debug($"Gathering {Resource} at {Dropsite[ObjectData.BASE_TYPE]}({Dropsite.Position}) with {UnitCount}/{UnitCapacity} units");
        }

        private void DoWood()
        {
            var range = 4;
            if (Dropsite[ObjectData.BASE_TYPE] == 109)
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

            UnitCapacity = Math.Min(6, trees.Count * 2);
            DoGathering(Units, trees);
        }

        private void DoGold()
        {
            var range = 4;
            if (Dropsite[ObjectData.BASE_TYPE] == 109)
            {
                range = 7;
            }

            var golds = new List<Unit>();
            foreach (var tile in Unary.GameState.Map.GetTilesInRange(Dropsite.Position.PointX, Dropsite.Position.PointY, range + 1))
            {
                foreach (var unit in tile.Units.Where(u => u.Targetable && u[ObjectData.CLASS] == (int)UnitClass.GoldMine))
                {
                    if (unit.Position.DistanceTo(Dropsite.Position) <= range)
                    {
                        golds.Add(unit);
                        unit.RequestUpdate();
                    }
                }
            }

            UnitCapacity = Math.Min(8, golds.Count * 2);
            DoGathering(Units, golds);
        }

        private void DoStone()
        {
            var range = 4;
            if (Dropsite[ObjectData.BASE_TYPE] == 109)
            {
                range = 7;
            }

            var stones = new List<Unit>();
            foreach (var tile in Unary.GameState.Map.GetTilesInRange(Dropsite.Position.PointX, Dropsite.Position.PointY, range + 1))
            {
                foreach (var unit in tile.Units.Where(u => u.Targetable && u[ObjectData.CLASS] == (int)UnitClass.StoneMine))
                {
                    if (unit.Position.DistanceTo(Dropsite.Position) <= range)
                    {
                        stones.Add(unit);
                        unit.RequestUpdate();
                    }
                }
            }

            UnitCapacity = Math.Min(8, stones.Count * 2);
            DoGathering(Units, stones);
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
