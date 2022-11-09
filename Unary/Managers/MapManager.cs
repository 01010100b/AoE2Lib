using AoE2Lib;
using AoE2Lib.Bots;
using AoE2Lib.Bots.GameElements;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unary.Managers
{
    internal class MapManager : Manager
    {
        private readonly HashSet<Tile> PassageBlockedTiles = new();
        private readonly HashSet<Tile> ConstructionBlockedTiles = new();
        private readonly HashSet<Tile> ConstructionExcludedTiles = new();
        private readonly Dictionary<Tile, int> PathDistances = new();

        public MapManager(Unary unary) : base(unary)
        {

        }

        public bool CanReach(Tile tile) => PathDistances.ContainsKey(tile);

        public int GetPathDistanceToHome(Tile tile)
        {
            if (PathDistances.TryGetValue(tile, out var distance))
            {
                return distance;
            }
            else
            {
                return int.MaxValue;
            }
        }

        public bool IsPassageBlocked(Tile tile) => PassageBlockedTiles.Contains(tile);

        public bool IsConstructionBlocked(Tile tile)
        {
            if (!tile.Explored)
            {
                return true;
            }
            else
            {
                return ConstructionBlockedTiles.Contains(tile);
            }
        }

        public bool CanPass(Unit unit, Tile tile)
        {
            if (!tile.Explored)
            {
                return true;
            }
            else if (PassageBlockedTiles.Contains(tile))
            {
                return false;
            }
            else
            {
                var civ = Unary.Mod.GetCivInfo(unit.Player.Civilization);
                var id = unit[ObjectData.UPGRADE_TYPE];
                var terrain = tile.Terrain;

                return civ.CanPassTerrain(id, terrain);
            }
        }

        public bool CanBuild(UnitType building, Tile tile, bool exclusion = true)
        {
            if (!tile.Explored)
            {
                return false;
            }

            var land = true;

            if (land && !tile.IsOnLand)
            {
                return false;
            }

            if (IsConstructionBlocked(tile))
            {
                return false;
            }

            if (exclusion && ConstructionExcludedTiles.Contains(tile))
            {
                return false;
            }

            var civ = Unary.Mod.GetCivInfo(Unary.GameState.MyPlayer.Civilization);
            var id = building[ObjectData.BASE_TYPE];
            var size = civ.GetUnitWidth(id);
            var footprint = Utils.GetUnitFootprint(tile.X, tile.Y, size, size);
            var min_all = int.MaxValue;
            var min_left = int.MaxValue;
            var min_bottom = int.MaxValue;
            var max_all = int.MinValue;
            var max_left = int.MinValue;
            var max_bottom = int.MinValue;

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

                        if (IsConstructionBlocked(t))
                        {
                            return false;
                        }

                        if (exclusion && ConstructionExcludedTiles.Contains(t))
                        {
                            return false;
                        }

                        var elevation = tile.Height;
                        min_all = Math.Min(min_all, elevation);
                        max_all = Math.Max(max_all, elevation);

                        if (x == footprint.X)
                        {
                            min_left = Math.Min(min_left, elevation);
                            max_left = Math.Max(max_left, elevation);
                        }

                        if (y == footprint.Bottom - 1)
                        {
                            min_bottom = Math.Min(min_bottom, elevation);
                            max_bottom = Math.Max(max_bottom, elevation);
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            var hill_mode = civ.GetUnitHillMode(id);

            if (hill_mode == 2 && min_all != max_all)
            {
                return false;
            }
            else if (hill_mode == 3 && Math.Max(max_left - min_left, max_bottom - min_bottom) > 1)
            {
                return false;
            }

            return true;
        }

        protected internal override void Update()
        {
            var actions = ObjectPool.Get(() => new List<Action>(), x => x.Clear());
            actions.Add(UpdateTileStates);
            actions.Add(UpdatePathDistances);

            Run(actions);
            ObjectPool.Add(actions);
        }

        private void UpdateTileStates()
        {
            PassageBlockedTiles.Clear();
            ConstructionBlockedTiles.Clear();
            ConstructionExcludedTiles.Clear();

            foreach (var unit in Unary.GameState.GetPlayers().SelectMany(p => p.Units))
            {
                var civ = Unary.Mod.GetCivInfo(unit.Player.Civilization);
                var id = unit[ObjectData.UPGRADE_TYPE];
                var blocks_construction = unit[ObjectData.SPEED] <= 0;
                var blocks_passage = civ.BlocksPassage(id);
                var width = civ.GetUnitWidth(id);
                var height = civ.GetUnitHeight(id);

                if (blocks_construction || blocks_passage)
                {
                    var footprint = Utils.GetUnitFootprint(unit.Position.PointX, unit.Position.PointY, width, height);

                    for (int x = footprint.X; x < footprint.Right; x++)
                    {
                        for (int y = footprint.Y; y < footprint.Bottom; y++)
                        {
                            if (Unary.GameState.Map.TryGetTile(x, y, out var tile))
                            {
                                if (blocks_construction)
                                {
                                    ConstructionBlockedTiles.Add(tile);
                                }

                                if (blocks_passage)
                                {
                                    PassageBlockedTiles.Add(tile);
                                }
                            }
                        }
                    }

                    if (blocks_construction && unit.IsBuilding)
                    {
                        footprint = Utils.GetUnitFootprint(unit.Position.PointX, unit.Position.PointY, width, height, 1);

                        for (int x = footprint.X; x < footprint.Right; x++)
                        {
                            for (int y = footprint.Y; y < footprint.Bottom; y++)
                            {
                                if (Unary.GameState.Map.TryGetTile(x, y, out var tile))
                                {
                                    ConstructionExcludedTiles.Add(tile);
                                }
                            }
                        }

                        if (Unary.GameState.TryGetUnitType(Unary.Mod.Farm, out var farm))
                        {
                            civ = Unary.Mod.GetCivInfo(Unary.GameState.MyPlayer.Civilization);
                            width = civ.GetUnitWidth(farm.Id);
                            height = civ.GetUnitHeight(farm.Id);

                            foreach (var tile in Unary.TownManager.GetFarmTiles(unit))
                            {
                                footprint = Utils.GetUnitFootprint(tile.X, tile.Y, width, height, 0);

                                for (int x = footprint.X; x < footprint.Right; x++)
                                {
                                    for (int y = footprint.Y; y < footprint.Bottom; y++)
                                    {
                                        if (Unary.GameState.Map.TryGetTile(x, y, out var t))
                                        {
                                            ConstructionExcludedTiles.Add(t);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void UpdatePathDistances()
        {
            if (Unary.GameState.Map.TryGetTile(Unary.TownManager.MyPosition, out var tile))
            {
                PathDistances.Clear();
                PathDistances.Add(tile, 0);
                Algorithms.AddAllPathDistances(PathDistances, GetPathNeighbours);
                Unary.Log.Debug($"Got {PathDistances.Count:N0} reachable tiles");
            }
        }

        private int GetExclusionZoneSize(Unit unit)
        {
            throw new NotImplementedException();
        }

        private readonly List<Tile> __PathNeighbours = new();
        private IReadOnlyList<Tile> GetPathNeighbours(Tile tile)
        {
            var neighbours = tile.GetNeighbours();
            __PathNeighbours.Clear();

            for (int i = 0; i < neighbours.Count; i++)
            {
                var neighbour = neighbours[i];
                var access = neighbour.IsOnLand && !PassageBlockedTiles.Contains(neighbour);

                if (access || neighbour.Center.DistanceTo(Unary.TownManager.MyPosition) < 3)
                {
                    __PathNeighbours.Add(neighbour);
                }
            }

            return __PathNeighbours;
        }
    }
}
