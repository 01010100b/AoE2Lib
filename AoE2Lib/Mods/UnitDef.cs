using System;
using System.Collections.Generic;
using System.Text;

namespace AoE2Lib.Mods
{
    public class UnitDef
    {
        public int Id { get; set; }
        public int FoundationId { get; set; }
        public int Width => (int)Math.Ceiling(2 * CollisionX - 0.01);
        public int Height => (int)Math.Ceiling(2 * CollisionY - 0.01);
        public double CollisionX { get; set; } = 0.5;
        public double CollisionY { get; set; } = 0.5;
        public int HillMode { get; set; } = 0;
        public int PlacementTerrain1 { get; set; } = -1;
        public int PlacementTerrain2 { get; set; } = -1;
        public int PlacementSideTerrain1 { get; set; } = -1;
        public int PlacementSideTerrain2 { get; set; } = -1;
        public int TerrainTable { get; set; } = -1;
    }
}
