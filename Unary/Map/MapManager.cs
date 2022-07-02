using AoE2Lib.Bots;
using AoE2Lib.Bots.GameElements;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unary.Map
{
    internal class MapManager : Manager
    {
        private readonly Dictionary<Tile, TileState> TileStates = new();

        public MapManager(Unary unary) : base(unary)
        {
        }

        public TileState GetTileState(Tile tile)
        {
            if (!TileStates.ContainsKey(tile))
            {
                TileStates.Add(tile, new TileState(tile, Unary));
            }

            return TileStates[tile];
        }

        protected internal override void Update()
        {
            UpdateTileStates();
        }

        private void UpdateTileStates()
        {
            foreach (var state in TileStates.Values)
            {
                state.IsConstructionBlocked = !state.Tile.Explored;
                state.IsConstructionExcluded = !state.Tile.Explored;
            }

            foreach (var unit in Unary.GameState.GetPlayers().SelectMany(p => p.Units))
            {
                var blocks = false;
                var def = Unary.Mod.GetUnitDef(unit[ObjectData.UPGRADE_TYPE]);
                var width = Math.Max(1, (int)Math.Round(2 * def.CollisionSizeX));
                var height = Math.Max(1, (int)Math.Round(2 * def.CollisionSizeY));

                switch ((int)def.ObstructionType)
                {
                    case 2:
                    case 3:
                    case 5:
                    case 10: blocks = true; break;
                }

                if (unit[ObjectData.SPEED] > 0)
                {
                    blocks = false;
                }
                
                if (blocks)
                {
                    var footprint = Utils.GetUnitFootprint(unit.Position.PointX, unit.Position.PointY, width, height);

                    for (int x = footprint.X; x < footprint.Right; x++)
                    {
                        for (int y = footprint.Y; y < footprint.Bottom; y++)
                        {
                            if (Unary.GameState.Map.TryGetTile(x, y, out var tile))
                            {
                                GetTileState(tile).IsConstructionBlocked = true;
                            }
                        }
                    }

                    footprint = Utils.GetUnitFootprint(unit.Position.PointX, unit.Position.PointY, width, height, 1);

                    for (int x = footprint.X; x < footprint.Right; x++)
                    {
                        for (int y = footprint.Y; y < footprint.Bottom; y++)
                        {
                            if (Unary.GameState.Map.TryGetTile(x, y, out var tile))
                            {
                                GetTileState(tile).IsConstructionExcluded = true;
                            }
                        }
                    }
                }
            }
        }
    }
}
