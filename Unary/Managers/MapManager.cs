using AoE2Lib;
using AoE2Lib.Bots;
using AoE2Lib.Bots.GameElements;
using System;
using System.Collections.Generic;
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
                var civ = unit.Player.Civilization;
                var id = unit[ObjectData.UPGRADE_TYPE];
                var terrain = tile.Terrain;

                return Unary.Mod.CanPassTerrain(civ, id, terrain);
            }
        }

        public bool CanBuildAt(UnitType building, Tile tile, bool exclusion)
        {
            var land = true;

            if (land && !tile.IsOnLand)
            {
                return false;
            }

            if (ConstructionBlockedTiles.Contains(tile))
            {
                return false;
            }

            if (exclusion && ConstructionExcludedTiles.Contains(tile))
            {
                return false;
            }

            var size = (int)Math.Round(Unary.Mod.GetUnitWidth(Unary.GameState.MyPlayer.Civilization, building[ObjectData.BASE_TYPE]));
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

                        if (ConstructionBlockedTiles.Contains(t))
                        {
                            return false;
                        }

                        if (exclusion && ConstructionExcludedTiles.Contains(t))
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
                var blocks_construction = true;
                var blocks_passage = false;
                var def = Unary.Mod.GetUnitDef(unit[ObjectData.UPGRADE_TYPE]);
                var width = Math.Max(1, (int)Math.Round(2 * def.CollisionSizeX));
                var height = Math.Max(1, (int)Math.Round(2 * def.CollisionSizeY));

                switch ((int)def.ObstructionType)
                {
                    case 2:
                    case 3:
                    case 5:
                    case 10: blocks_passage = true; break;
                }

                if (unit[ObjectData.SPEED] > 0)
                {
                    blocks_construction = false;
                    blocks_passage = false;
                }

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
                            var civ = Unary.GameState.MyPlayer.Civilization;
                            width = (int)Math.Max(1, Math.Round(Unary.Mod.GetUnitWidth(civ, farm.Id)));
                            height = (int)Math.Max(1, Math.Round(Unary.Mod.GetUnitHeight(civ, farm.Id)));

                            foreach (var tile in Unary.TownManager.GetFarms(unit))
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
