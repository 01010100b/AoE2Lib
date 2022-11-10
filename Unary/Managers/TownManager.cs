using AoE2Lib;
using AoE2Lib.Bots;
using AoE2Lib.Bots.GameElements;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unary.Jobs;
using static Unary.Managers.ProductionManager;

namespace Unary.Managers
{
    internal class TownManager : Manager
    {
        private static readonly Point[] FARM_DELTAS_4 = { new Point(2, 3), new Point(-1, 3), new Point(3, 0), new Point(3, -3), new Point(-4, 2), new Point(-4, -1), new Point(0, -4), new Point(-3, -4) };
        private static readonly Point[] FARM_DELTAS_2 = { new Point(-1, 2), new Point(2, -1), new Point(2, 2), new Point(-3, -1), new Point(-1, -3) };

        public Position MyPosition { get; private set; } = Position.Zero;

        private readonly HashSet<Tile> InsideTiles = new();

        public TownManager(Unary unary) : base(unary)
        {
        }

        public bool IsInside(Tile tile) => InsideTiles.Contains(tile);

        public IReadOnlyList<Tile> GetInsideTiles() => Unary.GetCached(GetSortedInsideTiles);

        public IEnumerable<Tile> GetPlacements(UnitType building)
        {
            foreach (var tile in GetInsideTiles())
            {
                if (Unary.GameState.Tick > 10 || tile.Position.DistanceTo(MyPosition) > 8)
                {
                    if (Unary.MapManager.CanBuild(building, tile))
                    {
                        yield return tile;
                    }
                }
            }
        }

        public IEnumerable<Tile> GetFarmTiles(int x, int y, int width, int height)
        {
            if (width != height)
            {
                throw new NotImplementedException();
            }
            else if (width != 2 && width != 4)
            {
                yield break;
            }

            IEnumerable<Point> deltas = null;

            if (width == 2)
            {
                deltas = FARM_DELTAS_2;
            }
            else if (width == 4)
            {
                deltas = FARM_DELTAS_4;
            }

            foreach (var delta in deltas)
            {
                var sx = x + delta.X;
                var sy = y + delta.Y;

                if (Unary.GameState.Map.TryGetTile(sx, sy, out var tile))
                {
                    yield return tile;
                }
            }
        }

        public IEnumerable<Tile> GetFarmTiles(Unit building)
        {
            var civ = Unary.CivInfo;
            var width = civ.GetUnitTileWidth(building[ObjectData.BASE_TYPE]);
            var height = civ.GetUnitTileHeight(building[ObjectData.BASE_TYPE]);

            return GetFarmTiles(building.Position.PointX, building.Position.PointY, width, height);
        }

        protected internal override void Update()
        {
            var actions = ObjectPool.Get(() => new List<Action>(), x => x.Clear());
            actions.Add(UpdateMyPosition);
            actions.Add(UpdateInsideTiles);
            actions.Add(UpdateHousing);

            Run(actions);
            ObjectPool.Add(actions);
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
            if (Unary.GameState.TryGetUnitType(Unary.Mod.House, out var house))
            {
                var pending = Math.Min(5, 1 + (int)Math.Floor(Unary.GameState.GameTime.TotalMinutes / 10));
                var margin = 5 * pending;
                var housing_room = Unary.GameState.MyPlayer.GetFact(FactId.HOUSING_HEADROOM);
                var population_room = Unary.GameState.MyPlayer.GetFact(FactId.POPULATION_HEADROOM);

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
