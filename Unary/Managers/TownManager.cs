using AoE2Lib;
using AoE2Lib.Bots;
using AoE2Lib.Bots.GameElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Unary.Managers.ResourcesManager;

namespace Unary.Managers
{
    // building placements, housing
    internal class TownManager : Manager
    {
        public TownManager(Unary unary) : base(unary)
        {

        }

        private readonly HashSet<Tile> InsideTiles = new();

        public List<Tile> GetDefaultSortedPossiblePlacements(UnitType building)
        {
            var tiles = InsideTiles.ToList();
            var pos = Unary.GameState.MyPosition;

            tiles.Sort((a, b) => a.Position.DistanceTo(pos).CompareTo(b.Position.DistanceTo(pos)));

            return tiles;
        }

        public int GetDefaultExclusionZone(UnitType building)
        {
            return 1;
        }

        public List<Tile> GetBuildingPlacements(UnitType building, IEnumerable<Tile> sorted_possible_placements)
        {
            var placements = new List<Tile>();

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

                if (state.TryGetTechnology(101, out var feudal))
                {
                    if (feudal.Finished)
                    {
                        margin = 10;
                    }
                }

                if (state.TryGetTechnology(102, out var castle))
                {
                    if (castle.Finished)
                    {
                        pending = 2;
                    }
                }

                var housing_room = state.MyPlayer.GetFact(FactId.HOUSING_HEADROOM);
                var population_room = state.MyPlayer.GetFact(FactId.POPULATION_HEADROOM);

                if (population_room > 0 && housing_room < margin && house.Pending < pending)
                {
                    var placements = GetDefaultSortedPossiblePlacements(house);
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

            for (int x = footprint.Left; x <= footprint.Right; x++)
            {
                for (int y = footprint.Top; y <= footprint.Bottom; y++)
                {
                    if (!Unary.GameState.Map.IsOnMap(x, y))
                    {
                        return false;
                    }

                    var t = Unary.GameState.Map.GetTile(x, y);

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
            }

            return true;
        }
    }
}
