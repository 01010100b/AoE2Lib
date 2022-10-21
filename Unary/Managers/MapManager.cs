using AoE2Lib.Bots;
using AoE2Lib.Bots.GameElements;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unary.Map;

namespace Unary.Managers
{
    internal class MapManager : Manager
    {
        private readonly HashSet<Tile> PassageBlockedTiles = new();
        private readonly HashSet<Tile> ConstructionBlockedTiles = new();
        private readonly HashSet<Tile> ConstructionExcludedTiles = new();

        public MapManager(Unary unary) : base(unary)
        {

        }

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
            UpdateTileStates();
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

                    if (blocks_construction)
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
                    }
                }
            }
        }
    }
}
