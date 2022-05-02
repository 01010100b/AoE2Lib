using AoE2Lib;
using AoE2Lib.Bots;
using AoE2Lib.Bots.GameElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unary.Managers
{
    internal class SitRepManager : Manager
    {
        public class SitRep
        {
            public readonly Tile Tile;
            public bool IsLandAccessible { get; internal set; }
            public bool IsWaterAccessible { get; internal set; }
            public bool IsConstructionBlocked { get; internal set; }
            public bool IsConstructionExcluded { get; internal set; }
            public int PathDistanceToHome { get; internal set; }

            public SitRep(Tile tile)
            {
                Tile = tile;
                Reset();
            }

            public void Reset()
            {
                IsLandAccessible = true;
                IsWaterAccessible = true;
                IsConstructionBlocked = false;
                IsConstructionExcluded = false;
                PathDistanceToHome = int.MaxValue;
            }
        }

        public SitRep this[Tile tile] => GetSitRep(tile);

        private readonly Dictionary<Tile, SitRep> SitReps = new();

        public SitRepManager(Unary unary) : base(unary)
        {

        }

        internal override void Update()
        {
            foreach (var sitrep in SitReps.Values)
            {
                sitrep.Reset();
            }

            UpdateAccessibilities();
        }

        private void UpdateAccessibilities()
        {
            var map = Unary.GameState.Map;

            foreach (var player in Unary.GameState.GetPlayers())
            {
                foreach (var unit in player.Units)
                {
                    var blocks_construction = true;
                    var blocks_movement = true;
                    var size = Unary.Mod.GetBuildingWidth(unit[ObjectData.BASE_TYPE]);

                    if (unit.Targetable == false || unit[ObjectData.SPEED] > 0)
                    {
                        blocks_construction = false;
                        blocks_movement = false;
                    }

                    if (unit[ObjectData.BASE_TYPE] == Unary.Mod.Farm)
                    {
                        blocks_movement = false;
                    }

                    if (blocks_construction || blocks_movement)
                    {
                        var footprint = Utils.GetUnitFootprint(unit.Tile.X, unit.Tile.Y, size, size, 0);

                        for (int x = footprint.X; x < footprint.Right; x++)
                        {
                            for (int y = footprint.Y; y < footprint.Bottom; y++)
                            {
                                if (map.IsOnMap(x, y))
                                {
                                    var t = map.GetTile(x, y);
                                    var sr = this[t];

                                    if (blocks_construction)
                                    {
                                        sr.IsConstructionBlocked = true;
                                    }

                                    if (blocks_movement)
                                    {
                                        sr.IsLandAccessible = false;
                                    }
                                }
                            }
                        }
                    }

                    if (blocks_construction)
                    {
                        if (unit[ObjectData.CMDID] == (int)CmdId.CIVILIAN_BUILDING || unit[ObjectData.CMDID] == (int)CmdId.MILITARY_BUILDING)
                        {
                            var exclusion = 1;
                            if (Unary.GameState.TryGetUnitType(unit[ObjectData.BASE_TYPE], out var type))
                            {
                                exclusion = Unary.TownManager.GetDefaultExclusionZone(type);
                            }

                            var footprint = Utils.GetUnitFootprint(unit.Tile.X, unit.Tile.Y, size, size, exclusion);

                            for (int x = footprint.X; x < footprint.Right; x++)
                            {
                                for (int y = footprint.Y; y < footprint.Bottom; y++)
                                {
                                    if (map.IsOnMap(x, y))
                                    {
                                        var t = map.GetTile(x, y);
                                        var sr = this[t];

                                        sr.IsConstructionExcluded = true;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private SitRep GetSitRep(Tile tile)
        {
            if (!SitReps.ContainsKey(tile))
            {
                var sitrep = new SitRep(tile);
                SitReps.Add(tile, sitrep);
            }

            return SitReps[tile];
        }
    }
}
