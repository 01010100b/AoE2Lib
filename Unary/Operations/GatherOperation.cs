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
            else if (Resource == Resource.FOOD)
            {
                DoFood();
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
            if (Dropsite[ObjectData.BASE_TYPE] == 109)
            {
                UnitCapacity = Math.Min(1, UnitCapacity);
            }

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

        private void DoFood()
        {
            UnitCapacity = 0;

            var units = Units;
            units.Sort((a, b) => a[ObjectData.ID].CompareTo(b[ObjectData.ID]));

            var meat = GetMeat();
            if (meat.Count > 0)
            {
                UnitCapacity += 7;

                meat.Sort((a, b) =>
                {
                    if (a[ObjectData.HITPOINTS] < b[ObjectData.HITPOINTS])
                    {
                        return -1;
                    }
                    else if (a[ObjectData.HITPOINTS] > b[ObjectData.HITPOINTS])
                    {
                        return 1;
                    }
                    else
                    {
                        return a[ObjectData.CARRY].CompareTo(b[ObjectData.CARRY]);
                    }
                });

                for (int i = 0; i < 7; i++)
                {
                    if (units.Count == 0)
                    {
                        break;
                    }

                    if (units[0][ObjectData.TARGET_ID] != meat[0].Id)
                    {
                        units[0].Target(meat[0]);
                    }

                    units.RemoveAt(0);
                }
            }

            var berries = GetBerries();
            UnitCapacity += Math.Min(4, berries.Count * 2);
            if (berries.Count > 0)
            {
                var gatherers = new List<Unit>();
                for (int i = 0; i < 4; i++)
                {
                    if (units.Count == 0)
                    {
                        break;
                    }

                    gatherers.Add(units[0]);
                    units.RemoveAt(0);
                }
                DoGathering(gatherers, berries);
            }
            
            var farms = GetFarms();
            UnitCapacity += farms.Count;
            for (int i = 0; i < Math.Min(farms.Count, units.Count); i++)
            {
                if (units[i][ObjectData.TARGET_ID] != farms[i].Id)
                {
                    units[i].Target(farms[i]);
                }
            }
        }

        private List<Unit> GetMeat()
        {
            var meat = new List<Unit>();

            if (Dropsite[ObjectData.BASE_TYPE] != 109)
            {
                return meat;
            }

            var range = 4;

            foreach (var tile in Unary.GameState.Map.GetTilesInRange(Dropsite.Position.PointX, Dropsite.Position.PointY, range + 1))
            {
                foreach (var unit in tile.Units.Where(u => u.Targetable && u[ObjectData.CLASS] == (int)UnitClass.Livestock))
                {
                    if (unit.Position.DistanceTo(Dropsite.Position) <= range)
                    {
                        meat.Add(unit);
                        unit.RequestUpdate();
                    }
                }
            }

            return meat;
        }

        private List<Unit> GetBerries()
        {
            var range = 4;
            if (Dropsite[ObjectData.BASE_TYPE] == 109)
            {
                range = 7;
            }

            var berries = new List<Unit>();
            foreach (var tile in Unary.GameState.Map.GetTilesInRange(Dropsite.Position.PointX, Dropsite.Position.PointY, range + 1))
            {
                foreach (var unit in tile.Units.Where(u => u.Targetable && u[ObjectData.CLASS] == (int)UnitClass.BerryBush))
                {
                    if (unit.Position.DistanceTo(Dropsite.Position) <= range)
                    {
                        berries.Add(unit);
                        unit.RequestUpdate();
                    }
                }
            }

            return berries;
        }

        private List<Unit> GetFarms()
        {
            var farms = new List<Unit>();
            foreach (var tile in Unary.BuildingManager.GetFarmPlacements(Dropsite))
            {
                var farm = tile.Units.FirstOrDefault(u => u.Targetable && u[ObjectData.CLASS] == (int)UnitClass.Farm);
                if (farm != null)
                {
                    farms.Add(farm);
                }
            }

            return farms;
        }

        private void DoGathering(List<Unit> units, List<Unit> resources)
        {
            if (units.Count == 0 || resources.Count == 0)
            {
                return;
            }

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
