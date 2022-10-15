using AoE2Lib;
using AoE2Lib.Bots;
using AoE2Lib.Bots.GameElements;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Unary.Managers.ProductionManager;

namespace Unary.Managers
{
    // building placements, housing
    internal class TownManager : Manager
    {
        private static readonly Point[] TC_FARM_DELTAS = { new Point(2, 3), new Point(-1, 3), new Point(3, 0), new Point(3, -3), new Point(-4, 2), new Point(-4, -1), new Point(0, -4), new Point(-3, -4) };
        private static readonly Point[] MILL_FARM_DELTAS = { new Point(-1, 2), new Point(2, -1), new Point(2, 2), new Point(-3, -1), new Point(-1, -3) };

        public Position MyPosition { get; private set; } = Position.Zero;

        public TownManager(Unary unary) : base(unary)
        {

        }

        private readonly HashSet<Tile> InsideTiles = new();

        public bool IsInside(Tile tile) => InsideTiles.Contains(tile);

        public IEnumerable<Tile> GetInsideTiles() => Unary.GetCached(GetSortedInsideTiles);

        public bool CanBuildAt(UnitType building, Tile tile, bool exclusion)
        {
            var land = true;

            if (land && !tile.IsOnLand)
            {
                return false;
            }

            var sitrep = Unary.SitRepManager[tile];

            if (sitrep.IsConstructionBlocked)
            {
                return false;
            }

            if (exclusion && sitrep.IsConstructionExcluded)
            {
                return false;
            }

            var size = Unary.Mod.GetBuildingSizeOld(building[ObjectData.BASE_TYPE]);
            var footprint = Utils.GetUnitFootprint(tile.X, tile.Y, size, size);

            for (int x = footprint.X; x < footprint.Right; x++)
            {
                for (int y = footprint.Y; y < footprint.Bottom; y++)
                {
                    if (Unary.GameState.Map.TryGetTile(x, y, out var t))
                    {
                        if (land && !t.IsOnLand)
                        {
                            return false;
                        }

                        var sr = Unary.SitRepManager[t];

                        if (sr.IsConstructionBlocked)
                        {
                            return false;
                        }

                        if (exclusion && sr.IsConstructionExcluded)
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public IEnumerable<Tile> GetPlacements(UnitType building)
        {
            foreach (var tile in GetInsideTiles())
            {
                if (Unary.GameState.Tick > 10 || tile.Position.DistanceTo(MyPosition) > 8)
                {
                    if (CanBuildAt(building, tile, true))
                    {
                        yield return tile;
                    }
                }
            }
        }

        protected internal override void Update()
        {
            UpdateMyPosition();
            UpdateInsideTiles();
            UpdateHousing();
        }

        private void UpdateMyPosition()
        {
            var tcs = ObjectPool.Get(() => new List<Unit>(), x => x.Clear());
            var buildings = ObjectPool.Get(() => new List<Unit>(), x => x.Clear());
            var units = ObjectPool.Get(() => new List<Unit>(), x => x.Clear());

            foreach (var unit in Unary.GameState.MyPlayer.Units.Where(u => u.Targetable))
            {
                if (unit[ObjectData.BASE_TYPE] == Unary.Mod.TownCenter)
                {
                    tcs.Add(unit);
                }
                else if (unit.IsBuilding)
                {
                    buildings.Add(unit);
                }
                else
                {
                    units.Add(unit);
                }
            }

            var lst = tcs;

            if (lst.Count == 0)
            {
                lst = buildings;

                if (lst.Count == 0)
                {
                    lst = units;
                }
            }

            Tile home = null;

            if (lst.Count > 0)
            {
                var oldest = lst[0];

                foreach (var unit in lst)
                {
                    if (unit.FirstUpdateGameTime < oldest.FirstUpdateGameTime)
                    {
                        oldest = unit;
                    }
                }

                var pos = oldest.Position;

                if (Unary.GameState.Map.TryGetTile(pos, out var tile))
                {
                    home = tile;
                }
            }

            if (home == null)
            {
                var pos = Unary.GameState.Map.Center;

                if (Unary.GameState.Map.TryGetTile(pos, out var tile))
                {
                    home = tile;
                }
            }

            if (home != null)
            {
                MyPosition = home.Position;
            }
            else
            {
                MyPosition = Unary.GameState.Map.Center;
            }

            ObjectPool.Add(tcs);
            ObjectPool.Add(buildings);
            ObjectPool.Add(units);
        }

        private void UpdateInsideTiles()
        {
            InsideTiles.Clear();

            foreach (var tile in Unary.GameState.Map.GetTilesInRange(MyPosition, 30))
            {
                InsideTiles.Add(tile);
            }
        }

        private void UpdateHousing()
        {
            var state = Unary.GameState;

            if (state.TryGetUnitType(Unary.Mod.House, out var house))
            {
                var margin = 5;
                var pending = 1;

                if (state.GameTime > TimeSpan.FromMinutes(5))
                {
                    margin = 10;
                }

                if (state.GameTime > TimeSpan.FromMinutes(10))
                {
                    pending = 2;
                }

                var housing_room = state.MyPlayer.GetFact(FactId.HOUSING_HEADROOM);
                var population_room = state.MyPlayer.GetFact(FactId.POPULATION_HEADROOM);

                if (population_room > 0 && housing_room < margin && house.Pending < pending)
                {
                    Unary.ProductionManager.Build(house, GetPlacements(house).Take(100), int.MaxValue, pending, Priority.HOUSING);
                }
            }
        }

        private List<Tile> GetSortedInsideTiles()
        {
            var tiles = ObjectPool.Get(() => new List<Tile>(), x => x.Clear());

            tiles.AddRange(InsideTiles);
            tiles.Sort((a, b) => a.Position.DistanceTo(MyPosition).CompareTo(b.Position.DistanceTo(MyPosition)));

            return tiles;
        }
    }
}
