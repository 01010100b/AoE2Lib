using AoE2Lib;
using AoE2Lib.Bots;
using AoE2Lib.Bots.GameElements;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Unary.Managers.ResourcesManager;

namespace Unary.Managers
{
    // building placements, housing
    internal class TownManager : Manager
    {
        private static readonly Point[] TC_FARM_DELTAS = { new Point(2, 3), new Point(-1, 3), new Point(3, 0), new Point(3, -3), new Point(-4, 2), new Point(-4, -1), new Point(0, -4), new Point(-3, -4) };
        private static readonly Point[] MILL_FARM_DELTAS = { new Point(-1, 2), new Point(2, -1), new Point(2, 2), new Point(-3, -1), new Point(-1, -3) };

        public TownManager(Unary unary) : base(unary)
        {

        }

        private readonly HashSet<Tile> InsideTiles = new();

        public bool IsInside(Tile tile) => InsideTiles.Contains(tile);

        public List<Tile> GetPossiblePlacements(UnitType building)
        {
            var tiles = new List<Tile>();

            foreach (var tile in InsideTiles)
            {
                if (Unary.GameState.Tick > 10 || Unary.GameState.MyPosition.DistanceTo(tile.Center) > 8)
                {
                    tiles.Add(tile);
                }
            }

            return tiles;
        }

        internal IEnumerable<Rectangle> GetExclusionZones(Unit building)
        {
            var size = Unary.Mod.GetBuildingWidth(building[ObjectData.BASE_TYPE]);
            var exclusion = 1;

            yield return Utils.GetUnitFootprint(building.Position.PointX, building.Position.PointY, size, size, exclusion);
        }

        internal List<Tile> GetBuildingPlacements(UnitType building, IEnumerable<Tile> possible_placements)
        {
            var placements = new List<Tile>();
            var sorted_possible_placements = possible_placements.ToList();

            // TODO sort per building type
            var my_pos = Unary.GameState.MyPosition;
            sorted_possible_placements.Sort((a, b) => a.Position.DistanceTo(my_pos).CompareTo(b.Position.DistanceTo(my_pos)));

            foreach (var tile in sorted_possible_placements)
            {
                if (CanBuildAt(building, tile, true))
                {
                    placements.Add(tile);
                }

                if (placements.Count >= 100)
                {
                    break;
                }
            }

            return placements;
        }

        internal override void Update()
        {
            UpdateInsideTiles();
            UpdateHousing();
        }

        private void UpdateInsideTiles()
        {
            InsideTiles.Clear();

            foreach (var tile in Unary.GameState.Map.GetTilesInRange(Unary.GameState.MyPosition, 30))
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
                    var placements = GetPossiblePlacements(house);
                    Unary.ResourcesManager.Build(house, placements, int.MaxValue, pending, Priority.HOUSING);
                }
            }
        }

        private bool CanBuildAt(UnitType building, Tile tile, bool exclusion)
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

            var size = Unary.Mod.GetBuildingWidth(building[ObjectData.BASE_TYPE]);
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
    }
}
