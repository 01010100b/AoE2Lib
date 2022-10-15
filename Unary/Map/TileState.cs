using AoE2Lib.Bots.GameElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unary.Map
{
    internal class TileState
    {
        public readonly Tile Tile;
        public bool IsPassageBlocked { get; internal set; } = true;
        public bool IsConstructionBlocked { get; internal set; } = true;
        public bool IsConstructionExcluded { get; internal set; } = true;

        private readonly Unary Unary;
        private HashSet<int> TerrainTables { get; set; } = null;

        internal TileState(Tile tile, Unary unary)
        {
            Tile = tile;
            Unary = unary;
        }

        public bool CanPass(int table)
        {
            if (!Tile.Explored)
            {
                return true;
            }

            var terrain = Tile.Terrain;

            if (TerrainTables == null)
            {
                TerrainTables = new();

                foreach (var kvp in Unary.Mod.GetTerrainTables())
                {
                    if (kvp.Value.Contains(terrain))
                    {
                        TerrainTables.Add(kvp.Key);
                    }
                }
            }

            return TerrainTables.Contains(table);
        }
    }
}
