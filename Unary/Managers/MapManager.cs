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
        public const int REGION_SIZE = 8;

        public Region this[Position position] { get { return GetRegion(position); } }
        public IEnumerable<Region> Regions => _Regions != null ? _Regions.SelectMany(r => r) : Enumerable.Empty<Region>();

        private Region[][] _Regions { get; set; } = null;
        private int Width { get; set; } = 0;
        private int Height { get; set; } = 0;
        private ScoutOperation ScoutingOperation { get; set; } = null;
        private readonly List<ScoutOperation> SheepScoutingOperations = new List<ScoutOperation>();

        public MapManager(Unary unary) : base(unary)
        {

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

        public override void Update()
        {
            if (_Regions == null)
            {
                var map = Unary.MapModule;
                var width = map.Width;
                var height = map.Height;

                if (width <= 0 || height <= 0)
                {
                    return;
                }

                Width = width / REGION_SIZE;
                if (width % REGION_SIZE != 0)
                {
                    Width++;
                }

                Height = height / REGION_SIZE;
                if (height % REGION_SIZE != 0)
                {
                    Height++;
                }

                Unary.Log.Debug($"MapManager: Regions width {Width} height {Height}");

                _Regions = new Region[Width][];
                
                for (int x = 0; x < Width; x++)
                {
                    _Regions[x] = new Region[Height];

                    for (int y = 0; y < Height; y++)
                    {
                        _Regions[x][y] = new Region();
                    }
                }

                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        var pos = new Position(x, y);
                        var tile = map[pos];
                        var region = GetRegion(pos);

                        region.Tiles.Add(tile);
                    }
                }
            }

            DoScouting();
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
                    Unary.Log.Info($"Scout with unit {best.Id}");
                }
            }
            /*
            foreach (var sheep in ops.FreeUnits.Where(u => u[ObjectData.CLASS] == (int)UnitClass.Livestock && u[ObjectData.HITPOINTS] > 0).ToList())
            {
                var op = new ScoutOperation(ops)
                {
                    Focus = ScoutingOperation.Focus,
                    MinExploredFraction = ScoutingOperation.MinExploredFraction
                };

                op.AddUnit(sheep);
                SheepScoutingOperations.Add(op);

                Unary.Log.Info($"Sheep scout with {sheep.Id}");
            }

            foreach (var sheepscouts in SheepScoutingOperations.Where(o => o.Units.Count() == 0).ToList())
            {
                sheepscouts.Stop();
                SheepScoutingOperations.Remove(sheepscouts);
            }
            */
        }

        private Region GetRegion(Position position)
        {
            var x = position.PointX / REGION_SIZE;
            var y = position.PointY / REGION_SIZE;

            return _Regions[x][y];
        }
    }
}
