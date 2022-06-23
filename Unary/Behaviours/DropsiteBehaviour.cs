using AoE2Lib;
using AoE2Lib.Bots;
using AoE2Lib.Bots.GameElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unary.Behaviours
{
    internal class DropsiteBehaviour : Behaviour
    {
        private readonly Dictionary<Resource, List<KeyValuePair<Tile, Unit>>> Resources = new();
        private readonly Dictionary<Controller, KeyValuePair<Tile, Unit>> Assignments = new();
        private readonly Dictionary<Controller, int> LastRequestTick = new();

        public IEnumerable<KeyValuePair<Tile, Unit>> GetResources(Resource resource)
        {
            if (Resources.TryGetValue(resource, out var resources))
            {
                return resources;
            }
            else
            {
                return Enumerable.Empty<KeyValuePair<Tile, Unit>>();
            }
        }

        public int GetPathDistance(Tile tile)
        {
            throw new NotImplementedException();
        }

        public Unit RequestAssignment(Controller gatherer, Resource resource)
        {
            LastRequestTick[gatherer] = Controller.Unary.GameState.Tick;

            if (Resources.TryGetValue(resource, out var resources))
            {
                if (resources.Count == 0)
                {
                    return null;
                }
            }
            else
            {
                return null;
            }

            if (Assignments.TryGetValue(gatherer, out var a))
            {
                if (a.Value.Targetable == false)
                {
                    Assignments.Remove(gatherer);
                }
                else
                {
                    return a.Value;
                }
            }

            var occ = new Dictionary<Tile, int>();

            foreach (var assignment in Assignments)
            {
                var tile = assignment.Value.Key;

                if (tile != null)
                {
                    if (!occ.ContainsKey(tile))
                    {
                        occ.Add(tile, 0);
                    }

                    occ[tile]++;
                }
            }

            foreach (var res in Resources[resource])
            {
                if (occ.TryGetValue(res.Key, out var o))
                {
                    if (o >= 2)
                    {
                        continue;
                    }
                }

                Assignments.Add(gatherer, res);

                return res.Value;
            }

            return null;
        }

        protected override bool Tick(bool perform)
        {
            UpdateResources();

            foreach (var kvp in LastRequestTick)
            {
                if (kvp.Value < Controller.Unary.GameState.Tick - 100)
                {
                    Assignments.Remove(kvp.Key);
                }
            }

            return false;
        }

        private void UpdateResources()
        {
            Resources.Clear();

            var range = Controller.Unary.Settings.DropsiteMaxResourceRange;
            var resources = new List<Resource>();
            var basetype = Controller.Unit[ObjectData.BASE_TYPE];

            if (basetype == Controller.Unary.Mod.TownCenter)
            {
                resources.Add(Resource.FOOD);
                resources.Add(Resource.WOOD);
                resources.Add(Resource.GOLD);
                resources.Add(Resource.STONE);

                range = Math.Max(range, 30);
            }
            else if (basetype == Controller.Unary.Mod.Mill)
            {
                resources.Add(Resource.FOOD);
            }
            else if (basetype == Controller.Unary.Mod.WoodCamp)
            {
                resources.Add(Resource.WOOD);
            }
            else if (basetype == Controller.Unary.Mod.MiningCamp)
            {
                resources.Add(Resource.GOLD);
                resources.Add(Resource.STONE);
            }

            var all_units = Controller.Unary.GameState.Map.GetUnitsInRange(Controller.Unit.Position, range).ToList();

            foreach (var resource in resources)
            {
                Resources.Add(resource, new List<KeyValuePair<Tile, Unit>>());

                var type = UnitClass.Tree;
                type = resource switch
                {
                    Resource.WOOD => UnitClass.Tree,
                    Resource.FOOD => UnitClass.BerryBush,
                    Resource.GOLD => UnitClass.GoldMine,
                    Resource.STONE => UnitClass.StoneMine,
                    _ => throw new ArgumentOutOfRangeException(nameof(resource)),
                };

                foreach (var unit in all_units.Where(u => u.Targetable && u[ObjectData.CLASS] == (int)type))
                {
                    foreach (var tile in unit.Tile.GetNeighbours())
                    {
                        if (Controller.Unary.SitRepManager[tile].PathDistanceToHome < 1000)
                        {
                            Resources[resource].Add(new KeyValuePair<Tile, Unit>(tile, unit));
                        }
                    }
                }
            }
        }
    }
}
