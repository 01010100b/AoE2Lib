using AoE2Lib;
using AoE2Lib.Bots.GameElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unary.Algorithms;
using Unary.Managers;

namespace Unary.UnitControllers
{
    class DropsiteController : UnitController
    {
        private readonly Dictionary<Resource, List<KeyValuePair<Tile, Unit>>> Resources = new();
        private Dictionary<Tile, int> Distances { get; set; } = new();

        public DropsiteController(Unit unit, Unary unary) : base(unit, unary)
        {

        }

        public IEnumerable<KeyValuePair<Tile, Unit>> GetGatherableResources(Resource resource)
        {
            if (Resources.TryGetValue(resource, out List<KeyValuePair<Tile, Unit>> res))
            {
                return res;
            }
            else
            {
                return Enumerable.Empty<KeyValuePair<Tile, Unit>>();
            }
        }

        public int GetPathDistance(Tile tile)
        {
            if (Distances.TryGetValue(tile, out int distance))
            {
                return distance;
            }
            else
            {
                return int.MaxValue;
            }
        }

        protected override void Tick()
        {
            UpdateResources();
            UpdateDistances();
        }

        private void UpdateResources()
        {
            var range = 10;

            Resources.Clear();

            var resources = new List<Resource>();
            var basetype = Unit[ObjectData.BASE_TYPE];

            if (basetype == Unary.Mod.TownCenter)
            {
                resources.Add(Resource.FOOD);
                resources.Add(Resource.WOOD);
                resources.Add(Resource.GOLD);
                resources.Add(Resource.STONE);
            }
            else if (basetype == Unary.Mod.Mill)
            {
                resources.Add(Resource.FOOD);
            }
            else if (basetype == Unary.Mod.LumberCamp)
            {
                resources.Add(Resource.WOOD);
            }
            else if (basetype == Unary.Mod.MiningCamp)
            {
                resources.Add(Resource.GOLD);
                resources.Add(Resource.STONE);
            }

            foreach (var resource in resources)
            {
                Resources[resource] = new List<KeyValuePair<Tile, Unit>>();

                var type = UnitClass.Tree;
                type = resource switch
                {
                    Resource.WOOD => UnitClass.Tree,
                    Resource.FOOD => UnitClass.BerryBush,
                    Resource.GOLD => UnitClass.GoldMine,
                    Resource.STONE => UnitClass.StoneMine,
                    _ => throw new ArgumentOutOfRangeException(nameof(resource)),
                };

                foreach (var tile in Unary.GameState.Map.GetTilesInRange(Unit.Position, range))
                {
                    foreach (var unit in tile.Units.Where(u => u.Targetable))
                    {
                        if (unit[ObjectData.CLASS] == (int)type)
                        {
                            foreach (var t in tile.GetNeighbours())
                            {
                                if (Unary.MapManager.CanReach(t))
                                {
                                    Resources[resource].Add(new KeyValuePair<Tile, Unit>(t, unit));
                                }
                            }
                        }
                    }
                }
            }
        }

        private void UpdateDistances()
        {
            var range = 10;
            Distances.Clear();

            var initial = new List<Tile>();
            var size = Unary.Mod.GetBuildingSize(Unit[ObjectData.BASE_TYPE]);
            var footprint = MapManager.GetUnitFootprint(size, size, Unit.Tile, 1);

            var x = footprint.Left;
            var y = 0;
            for (y = footprint.Top; y < footprint.Bottom; y++)
            {
                if (Unary.GameState.Map.IsOnMap(x, y))
                {
                    var tile = Unary.GameState.Map.GetTile(x, y);

                    if (Unary.MapManager.CanReach(tile))
                    {
                        initial.Add(tile);
                    }
                }
            }

            x = footprint.Right - 1;
            for (y = footprint.Top; y < footprint.Bottom; y++)
            {
                if (Unary.GameState.Map.IsOnMap(x, y))
                {
                    var tile = Unary.GameState.Map.GetTile(x, y);

                    if (Unary.MapManager.CanReach(tile))
                    {
                        initial.Add(tile);
                    }
                }
            }

            y = footprint.Top;
            for (x = footprint.Left + 1; x < footprint.Right - 1; x++)
            {
                if (Unary.GameState.Map.IsOnMap(x, y))
                {
                    var tile = Unary.GameState.Map.GetTile(x, y);

                    if (Unary.MapManager.CanReach(tile))
                    {
                        initial.Add(tile);
                    }
                }
            }

            y = footprint.Bottom - 1;
            for (x = footprint.Left + 1; x < footprint.Right - 1; x++)
            {
                if (Unary.GameState.Map.IsOnMap(x, y))
                {
                    var tile = Unary.GameState.Map.GetTile(x, y);

                    if (Unary.MapManager.CanReach(tile))
                    {
                        initial.Add(tile);
                    }
                }
            }

            Distances = Pathing.GetAllPathDistances(initial, x => x.GetNeighbours().Where(t => !Unary.MapManager.IsLandBlocked(t)), range);
        }
    }
}
