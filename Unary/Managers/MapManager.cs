using AoE2Lib.Bots.Modules;
using AoE2Lib.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Unary.Utils;
using Unary.Operations;
using AoE2Lib.Bots.GameElements;
using AoE2Lib;

namespace Unary.Managers
{
    class MapManager : Manager
    {
        public class ScoutingState
        {
            public Tile Tile { get; set; }
            public TimeSpan LastScoutedGameTime { get; set; } = TimeSpan.MinValue;
            public TimeSpan LastAccessFailureGameTime { get; set; } = TimeSpan.MinValue;
        }

        private readonly Dictionary<Tile, ScoutingState> ScoutingStates = new Dictionary<Tile, ScoutingState>();
        private ScoutOperation ScoutingOperation { get; set; } = null;

        public MapManager(Unary unary) : base(unary)
        {

        }

        public IEnumerable<Tile> GetGrid(int size)
        {
            var map = Unary.MapModule;
            var width = map.Width;
            var height = map.Height;

            for (int i = size / 2; i < width; i += size)
            {
                for (int j = size / 2; j < height; j += size)
                {
                    var pos = new Position(i, j);

                    if (map.IsOnMap(pos))
                    {
                        yield return map[pos];
                    }
                }
            }
        }

        public ScoutingState GetScoutingState(Tile tile)
        {
            if (!ScoutingStates.ContainsKey(tile))
            {
                ScoutingStates.Add(tile, new ScoutingState() { Tile = tile });
            }

            return ScoutingStates[tile];
        }

        public IEnumerable<Tile> GetPlacements(int building_id, HashSet<Tile> within)
        {
            var width = (int)Math.Ceiling(Unary.Mod.GetWidth(building_id));
            var height = (int)Math.Ceiling(Unary.Mod.GetHeight(building_id));
            var hill_mode = Unary.Mod.GetHillMode(building_id);
            var map = Unary.MapModule;
            var obstructions = new HashSet<Position>();

            foreach (var tile in within)
            {
                if (!tile.Explored)
                {
                    obstructions.Add(tile.Position);
                }
                else
                {
                    foreach (var unit in tile.Units.Where(u => u.Targetable))
                    {
                        var obstruction = false;
                        switch ((UnitClass)unit[ObjectData.CLASS])
                        {
                            case UnitClass.BerryBush:
                            case UnitClass.Building:
                            case UnitClass.Cliff:
                            case UnitClass.DeepSeaFish:
                            case UnitClass.Farm:
                            case UnitClass.Gate:
                            case UnitClass.GoldMine:
                            case UnitClass.MiscBuilding:
                            case UnitClass.OceanFish:
                            case UnitClass.OreMine:
                            case UnitClass.ResourcePile:
                            case UnitClass.SalvagePile:
                            case UnitClass.ShoreFish:
                            case UnitClass.StoneMine:
                            case UnitClass.Tower:
                            case UnitClass.Tree:
                            case UnitClass.TreeStump:
                            case UnitClass.Wall: obstruction = true; break;
                        }

                        if (obstruction)
                        {
                            var w = (int)Math.Ceiling(Unary.Mod.GetWidth(unit[ObjectData.UPGRADE_TYPE]));
                            var h = (int)Math.Ceiling(Unary.Mod.GetHeight(unit[ObjectData.UPGRADE_TYPE]));

                            var x_start = tile.Position.PointX - (w / 2);
                            var x_end = tile.Position.PointX + (w / 2);
                            if (w % 2 == 0)
                            {
                                x_end--;
                            }

                            var y_start = tile.Position.PointY - (h / 2);
                            var y_end = tile.Position.PointY + (h / 2);
                            if (h % 2 == 0)
                            {
                                y_end--;
                            }

                            for (int x = x_start; x <= x_end; x++)
                            {
                                for (int y = y_start; y <= y_end; y++)
                                {
                                    obstructions.Add(Position.FromPoint(x, y));
                                }
                            }
                        }
                    }
                }
            }

            foreach (var tile in within.Where(t => !obstructions.Contains(t.Position)))
            {
                var x_start = tile.Position.PointX - (width / 2);
                var x_end = tile.Position.PointX + (width / 2);
                if (width % 2 == 0)
                {
                    x_end--;
                }

                var y_start = tile.Position.PointY - (height / 2);
                var y_end = tile.Position.PointY + (height / 2);
                if (height % 2 == 0)
                {
                    y_end--;
                }

                var can_place = true;
                var elevation = -1;

                for (int x = x_start; x <= x_end; x++)
                {
                    if (!can_place)
                    {
                        break;
                    }

                    for (int y = y_start; y <= y_end; y++)
                    {
                        if (!can_place)
                        {
                            break;
                        }

                        can_place = false;
                        var pos = Position.FromPoint(x, y);

                        if (map.IsOnMap(pos))
                        {
                            var t = map[pos];

                            if (!obstructions.Contains(t.Position))
                            {
                                if (elevation == -1)
                                {
                                    elevation = t.Elevation;
                                }

                                var elevation_good = true;
                                if (hill_mode == 3 && Math.Abs(elevation - t.Elevation) > 1)
                                {
                                    elevation_good = false;
                                }

                                if (elevation_good)
                                {
                                    can_place = true;
                                }
                            }
                        }
                    }
                }

                if (can_place)
                {
                    yield return tile;
                }
            }
        }

        internal override void Update()
        {

            Unary.SetStrategicNumber(StrategicNumber.HOME_EXPLORATION_TIME, 600);
            Unary.SetStrategicNumber(StrategicNumber.NUMBER_EXPLORE_GROUPS, 1);
            //DoScouting();
        }

        private void DoScouting()
        {
            var ops = Unary.OperationsManager;
            var info = Unary.InfoModule;

            if (ScoutingOperation == null)
            {
                ScoutingOperation = new ScoutOperation(ops);
            }

            ScoutingOperation.Focus = info.MyPosition;

            if (info.GameTime > TimeSpan.FromMinutes(5))
            {
                ScoutingOperation.MinExploredFraction = 0.95;
            }
            else
            {
                ScoutingOperation.MinExploredFraction = 0.7;
            }

            if (ScoutingOperation.Units.Count() == 0)
            {
                var speed = 0;
                Unit best = null;

                foreach (var unit in ops.FreeUnits.Where(u => u.GetData(ObjectData.CMDID) == (int)CmdId.MILITARY))
                {
                    if (best == null || unit.GetData(ObjectData.SPEED) >= speed)
                    {
                        speed = unit.GetData(ObjectData.SPEED);
                        best = unit;
                    }
                }

                if (best != null)
                {
                    ScoutingOperation.AddUnit(best);
                }
            }
        }
    }
}
