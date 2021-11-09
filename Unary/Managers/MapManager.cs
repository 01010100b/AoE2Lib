using AoE2Lib;
using AoE2Lib.Bots.GameElements;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unary.Algorithms;

namespace Unary.Managers
{
    class MapManager : Manager
    {
        public static Rectangle GetUnitFootprint(int width, int height, Tile tile, int exclusion_zone_size)
        {
            var x = tile.X;
            var y = tile.Y;

            width += 2 * exclusion_zone_size;
            height += 2 * exclusion_zone_size;

            var x_start = x - (width / 2);
            var x_end = x + (width / 2);
            if (width % 2 == 0)
            {
                x_end--;
            }

            var y_start = y - (height / 2);
            var y_end = y + (height / 2);
            if (height % 2 == 0)
            {
                y_end--;
            }

            return new Rectangle(x_start, y_start, x_end - x_start + 1, y_end - y_start + 1);
        }

        private readonly HashSet<Tile> ConstructionBlockedTiles = new();
        private readonly HashSet<Tile> ConstructionExcludedTiles = new();
        private readonly HashSet<Tile> LandBlockedTiles = new();
        private Dictionary<Tile, int> Distances { get; set; } = new();

        public MapManager(Unary unary) : base(unary)
        {

        }

        public bool IsConstructionBlocked(Tile tile)
        {
            return ConstructionBlockedTiles.Contains(tile);
        }

        public bool IsConstructionExcluded(Tile tile)
        {
            return ConstructionExcludedTiles.Contains(tile);
        }

        public bool IsLandBlocked(Tile tile)
        {
            if (!tile.IsOnLand)
            {
                return true;
            }

            if (tile.Position.DistanceTo(Unary.GameState.MyPosition) < 4)
            {
                return false;
            }

            return LandBlockedTiles.Contains(tile);
        }

        public bool CanReach(Tile tile)
        {
            return Distances.ContainsKey(tile);
        }

        public int GetDistance(Tile tile)
        {
            if (Distances.TryGetValue(tile, out int distance))
            {
                return distance;
            }
            else
            {
                return -1;
            }
        }

        internal override void Update()
        {
            UpdateTiles();
            UpdateDistances();
        }

        private void UpdateTiles()
        {
            ConstructionBlockedTiles.Clear();
            ConstructionExcludedTiles.Clear();
            LandBlockedTiles.Clear();
            var map = Unary.GameState.Map;

            foreach (var player in Unary.GameState.GetPlayers())
            {
                foreach (var unit in player.Units)
                {
                    var construction = BlocksConstruction(unit);
                    var land = BlocksLand(unit);
                    var width = Unary.Mod.GetBuildingSize(unit[ObjectData.BASE_TYPE]);
                    var height = width;
                    var footprint = GetUnitFootprint(width, height, unit.Tile, 0);

                    for (int x = footprint.X; x < footprint.Right; x++)
                    {
                        for (int y = footprint.Y; y < footprint.Bottom; y++)
                        {
                            if (map.IsOnMap(x, y))
                            {
                                var tile = map.GetTile(x, y);

                                if (construction)
                                {
                                    ConstructionBlockedTiles.Add(tile);
                                }

                                if (land)
                                {
                                    LandBlockedTiles.Add(tile);
                                }
                            }
                        }
                    }

                    if (construction)
                    {
                        if (unit[ObjectData.CMDID] == (int)CmdId.CIVILIAN_BUILDING || unit[ObjectData.CMDID] == (int)CmdId.MILITARY_BUILDING)
                        {
                            var excl = GetExclusionZoneSize(unit[ObjectData.BASE_TYPE]);
                            footprint = GetUnitFootprint(width, height, unit.Tile, excl);
                            for (int x = footprint.X; x < footprint.Right; x++)
                            {
                                for (int y = footprint.Y; y < footprint.Bottom; y++)
                                {
                                    if (map.IsOnMap(x, y))
                                    {
                                        ConstructionExcludedTiles.Add(map.GetTile(x, y));
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void UpdateDistances()
        {
            if (Unary.GameState.Map.IsOnMap(Unary.GameState.MyPosition))
            {
                var tile = Unary.GameState.Map.GetTile(Unary.GameState.MyPosition);
                var dict = Pathing.GetAllPathDistances(new[] { tile }, x => x.GetNeighbours().Where(t => !IsLandBlocked(t)));
                Distances = dict;
                Unary.Log.Debug($"Got {dict.Count} reachable tiles");
            }
        }

        private bool BlocksConstruction(Unit unit)
        {
            if (!unit.Targetable)
            {
                return false;
            }

            if (unit[ObjectData.SPEED] > 0)
            {
                return false;
            }

            if (unit[ObjectData.CLASS] == (int)UnitClass.TreeStump)
            {
                return false;
            }

            return true;
        }

        private bool BlocksLand(Unit unit)
        {
            if (!unit.Targetable)
            {
                return false;
            }

            if (unit[ObjectData.SPEED] > 0)
            {
                return false;
            }

            if (unit[ObjectData.CLASS] == (int)UnitClass.TreeStump)
            {
                return false;
            }

            if (unit[ObjectData.BASE_TYPE] == Unary.Mod.Farm)
            {
                return false;
            }

            if (unit[ObjectData.STATUS] == 0)
            {
                return false;
            }

            return true;
        }

        private int GetExclusionZoneSize(int base_type_id)
        {
            if (base_type_id == Unary.Mod.TownCenter || base_type_id == Unary.Mod.TownCenterFoundation || base_type_id == Unary.Mod.Mill)
            {
                return 3;
            }
            else if (base_type_id == Unary.Mod.LumberCamp || base_type_id == Unary.Mod.MiningCamp || base_type_id == Unary.Mod.Dock)
            {
                return 3;
            }
            else if (base_type_id == Unary.Mod.Farm)
            {
                return 0;
            }
            else
            {
                return 1;
            }
        }
    }
}
