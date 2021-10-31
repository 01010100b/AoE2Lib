using AoE2Lib;
using AoE2Lib.Bots;
using AoE2Lib.Bots.GameElements;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using Unary.UnitControllers;

namespace Unary.Managers
{
    class BuildingManager : Manager
    {
        internal static readonly Point[] TC_FARM_DELTAS = new[] { new Point(2, 3), new Point(-1, 3), new Point(3, 0), new Point(3, -3), new Point(-4, 2), new Point(-4, -1), new Point(0, -4), new Point(-3, -4) };
        internal static readonly Point[] MILL_FARM_DELTAS = new[] { new Point(-1, 2), new Point(2, -1), new Point(2, 2), new Point(-3, -1), new Point(-1, -3) };

        private readonly HashSet<Tile> ObstructedTiles = new();
        private readonly HashSet<Tile> ExcludedTiles = new();
        private readonly List<Unit> Foundations = new();

        public BuildingManager(Unary unary) : base(unary)
        {

        }

        public int GetMaximumBuilders()
        {
            return 4;
        }

        public IEnumerable<Unit> GetFoundations()
        {
            return Foundations;
        }

        public bool CanBuildAt(UnitType type, Tile tile, bool ignore_exclusion = false)
        {
            var footprint = GetUnitFootprint(type[ObjectData.BASE_TYPE], tile.Position, 0);

            for (int x = footprint.X; x < footprint.Right; x++)
            {
                for (int y = footprint.Y; y < footprint.Bottom; y++)
                {
                    if (!Unary.GameState.Map.IsOnMap(x, y))
                    {
                        return false;
                    }

                    var t = Unary.GameState.Map.GetTile(x, y);

                    if (IsObstructed(t))
                    {
                        return false;
                    }

                    if (IsExcluded(t) && !ignore_exclusion)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public bool IsObstructed(Tile tile)
        {
            if (!tile.IsOnLand)
            {
                return true;
            }

            return ObstructedTiles.Contains(tile);
        }

        public IEnumerable<Tile> GetObstructedTiles()
        {
            return ObstructedTiles;
        }

        public bool IsExcluded(Tile tile)
        {
            if (!tile.IsOnLand)
            {
                return true;
            }

            return ExcludedTiles.Contains(tile);
        }

        public IEnumerable<Tile> GetBuildingPlacements(UnitType building)
        {
            if (building.Id == 50)
            {
                return GetFarmPlacements();
            }

            throw new NotImplementedException();
        }

        public IEnumerable<Tile> GetFarmPlacements(Unit dropsite)
        {
            var deltas = TC_FARM_DELTAS;
            if (dropsite[ObjectData.BASE_TYPE] == Unary.Mod.Mill)
            {
                deltas = MILL_FARM_DELTAS;
            }

            foreach (var delta in deltas)
            {
                var x = dropsite.Position.PointX + delta.X;
                var y = dropsite.Position.PointY + delta.Y;

                if (Unary.GameState.Map.IsOnMap(x, y))
                {
                    yield return Unary.GameState.Map.GetTile(x, y);
                }
            }
        }

        internal override void Update()
        {
            ObstructedTiles.Clear();
            ExcludedTiles.Clear();

            var map = Unary.GameState.Map;
            foreach (var player in Unary.GameState.GetPlayers())
            {
                foreach (var unit in player.Units.Where(u => u.Targetable && u[ObjectData.SPEED] <= 0))
                {
                    var footprint = GetUnitFootprint(unit[ObjectData.BASE_TYPE], unit.Position, 0);
                    for (int x = footprint.X; x < footprint.Right; x++)
                    {
                        for (int y = footprint.Y; y < footprint.Bottom; y++)
                        {
                            if (map.IsOnMap(x, y))
                            {
                                ObstructedTiles.Add(map.GetTile(x, y));
                            }
                        }
                    }

                    var excl = GetExclusionZoneSize(unit[ObjectData.BASE_TYPE]);
                    footprint = GetUnitFootprint(unit[ObjectData.BASE_TYPE], unit.Position, excl);
                    for (int x = footprint.X; x < footprint.Right; x++)
                    {
                        for (int y = footprint.Y; y < footprint.Bottom; y++)
                        {
                            if (map.IsOnMap(x, y))
                            {
                                ExcludedTiles.Add(map.GetTile(x, y));
                            }
                        }
                    }
                }
            }

            Foundations.Clear();
            foreach (var unit in Unary.GameState.MyPlayer.Units.Where(u => u.Targetable && u[ObjectData.STATUS] == 0))
            {
                if (unit[ObjectData.CMDID] == (int)CmdId.CIVILIAN_BUILDING || unit[ObjectData.CMDID] == (int)CmdId.MILITARY_BUILDING)
                {
                    Foundations.Add(unit);
                }
            }

            var max_builders = Math.Min(GetMaximumBuilders(), Foundations.Count);
            var builders = Unary.UnitsManager.GetControllers<BuilderController>().Count;
            if (builders < max_builders)
            {
                var foundation = Foundations[Unary.Rng.Next(Foundations.Count)];
                var gatherers = Unary.UnitsManager.GetControllers<GathererController>();
                gatherers.Sort((a, b) => a.Unit.Position.DistanceTo(foundation.Position).CompareTo(b.Unit.Position.DistanceTo(foundation.Position)));

                if (gatherers.Count > 0)
                {
                    var builder = gatherers[0].Unit;
                    var ctrl = new BuilderController(builder, Unary);
                    Unary.UnitsManager.SetController(builder, ctrl);
                }
            }

            Unary.GameState.SetStrategicNumber(StrategicNumber.DISABLE_BUILDER_ASSISTANCE, 1);
            Unary.GameState.SetStrategicNumber(StrategicNumber.ENABLE_NEW_BUILDING_SYSTEM, 1);
            Unary.GameState.SetStrategicNumber(StrategicNumber.INITIAL_EXPLORATION_REQUIRED, 0);
            
            Unary.GameState.SetStrategicNumber(StrategicNumber.CAP_CIVILIAN_BUILDERS, 4);
            Unary.GameState.SetStrategicNumber(StrategicNumber.MAXIMUM_TOWN_SIZE, 20);
            Unary.GameState.SetStrategicNumber(StrategicNumber.MINING_CAMP_MAX_DISTANCE, 30);
            Unary.GameState.SetStrategicNumber(StrategicNumber.LUMBER_CAMP_MAX_DISTANCE, 30);
            Unary.GameState.SetStrategicNumber(StrategicNumber.MILL_MAX_DISTANCE, 30);

            if (Unary.GameState.GetTechnology(101).State == ResearchState.COMPLETE)
            {
                Unary.GameState.SetStrategicNumber(StrategicNumber.CAP_CIVILIAN_BUILDERS, 8);
                Unary.GameState.SetStrategicNumber(StrategicNumber.MAXIMUM_TOWN_SIZE, 25);
                Unary.GameState.SetStrategicNumber(StrategicNumber.MINING_CAMP_MAX_DISTANCE, 35);
                Unary.GameState.SetStrategicNumber(StrategicNumber.LUMBER_CAMP_MAX_DISTANCE, 35);
                Unary.GameState.SetStrategicNumber(StrategicNumber.MILL_MAX_DISTANCE, 35);
            }

            if (Unary.GameState.GetTechnology(102).State == ResearchState.COMPLETE)
            {
                Unary.GameState.SetStrategicNumber(StrategicNumber.CAP_CIVILIAN_BUILDERS, 12);
                Unary.GameState.SetStrategicNumber(StrategicNumber.MAXIMUM_TOWN_SIZE, 30);
                Unary.GameState.SetStrategicNumber(StrategicNumber.MINING_CAMP_MAX_DISTANCE, 40);
                Unary.GameState.SetStrategicNumber(StrategicNumber.LUMBER_CAMP_MAX_DISTANCE, 40);
                Unary.GameState.SetStrategicNumber(StrategicNumber.MILL_MAX_DISTANCE, 40);
            }

            if (Unary.GameState.GetTechnology(103).State == ResearchState.COMPLETE)
            {
                Unary.GameState.SetStrategicNumber(StrategicNumber.CAP_CIVILIAN_BUILDERS, 16);
                Unary.GameState.SetStrategicNumber(StrategicNumber.MAXIMUM_TOWN_SIZE, 35);
                Unary.GameState.SetStrategicNumber(StrategicNumber.MINING_CAMP_MAX_DISTANCE, 50);
                Unary.GameState.SetStrategicNumber(StrategicNumber.LUMBER_CAMP_MAX_DISTANCE, 50);
                Unary.GameState.SetStrategicNumber(StrategicNumber.MILL_MAX_DISTANCE, 50);
            }

            //DoBuildOperations();
        }

        private Rectangle GetUnitFootprint(int type_id, Position position, int exclusion_zone_size)
        {
            var width = Unary.Mod.GetBuildingSize(type_id);
            var height = Unary.Mod.GetBuildingSize(type_id);
            var x = position.PointX;
            var y = position.PointY;

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

        private int GetExclusionZoneSize(int type_id)
        {
            if (type_id == Unary.Mod.TownCenter || type_id == Unary.Mod.TownCenterFoundation || type_id == Unary.Mod.Mill)
            {
                return 3;
            }
            else if (type_id == Unary.Mod.LumberCamp || type_id == Unary.Mod.MiningCamp || type_id == Unary.Mod.Dock)
            {
                return 3;
            }
            else
            {
                return 1;
            }
        }

        private IEnumerable<Tile> GetFarmPlacements()
        {
            var sites = new List<Unit>();

            foreach (var unit in Unary.GameState.MyPlayer.Units.Where(u => u.Targetable))
            {
                if (unit[ObjectData.BASE_TYPE] == 109 || unit[ObjectData.BASE_TYPE] == 68)
                {
                    sites.Add(unit);
                }
            }

            if (sites.Count == 0)
            {
                Unary.Log.Warning("Could not find site for farms");

                yield break;
            }

            var site = sites[Unary.Rng.Next(sites.Count)];
            var deltas = TC_FARM_DELTAS;
            if (site[ObjectData.BASE_TYPE] == 68)
            {
                deltas = MILL_FARM_DELTAS;
            }

            foreach (var delta in deltas)
            {
                var x = site.Position.PointX + delta.X;
                var y = site.Position.PointY + delta.Y;

                if (x >= 0 && x < Unary.GameState.Map.Width && y >= 0 && y < Unary.GameState.Map.Height)
                {
                    yield return Unary.GameState.Map.GetTile(x, y);
                }
            }
        }
    }
}
