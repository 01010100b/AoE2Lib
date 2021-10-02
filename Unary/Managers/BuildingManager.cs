using AoE2Lib;
using AoE2Lib.Bots;
using AoE2Lib.Bots.GameElements;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using Unary.Operations;

namespace Unary.Managers
{
    class BuildingManager : Manager
    {
        internal static readonly Point[] TC_FARM_DELTAS = new[] { new Point(2, 3), new Point(-1, 3), new Point(3, 0), new Point(3, -3), new Point(-4, 2), new Point(-4, -1), new Point(0, -4), new Point(-3, -4) };
        internal static readonly Point[] MILL_FARM_DELTAS = new[] { new Point(-1, 2), new Point(2, -1), new Point(2, 2), new Point(-3, -1), new Point(-1, -3) };

        private readonly HashSet<Tile> ObstructedTiles = new();
        private readonly HashSet<Tile> ExcludedTiles = new();
        private readonly HashSet<Unit> BuildingFoundations = new();
        private readonly List<BuildOperation> BuildOperations = new();

        public BuildingManager(Unary unary) : base(unary)
        {

        }

        public Rectangle GetUnitFootprint(int type_id, Position position, int exclusion)
        {
            var width = Unary.Mod.GetUnitSize(type_id);
            var height = Unary.Mod.GetUnitSize(type_id);
            var x = position.PointX;
            var y = position.PointY;

            width += 2 * exclusion;
            height += 2 * exclusion;

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

        public int GetExclusionZoneSize(int type_id)
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

        public bool IsObstructed(Tile tile)
        {
            if (!tile.IsOnLand)
            {
                return true;
            }

            return ObstructedTiles.Contains(tile);
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

            DoBuildOperations();
        }

        private void DoBuildOperations()
        {
            // get available foundations

            foreach (var unit in Unary.GameState.MyPlayer.Units.Where(u => u.Targetable && u[ObjectData.STATUS] == 0))
            {
                if (unit[ObjectData.CMDID] == (int)CmdId.CIVILIAN_BUILDING || unit[ObjectData.CMDID] == (int)CmdId.MILITARY_BUILDING)
                {
                    BuildingFoundations.Add(unit);
                }
            }

            var foundations = new HashSet<Unit>();
            foreach (var f in BuildingFoundations)
            {
                foundations.Add(f);
            }

            foreach (var foundation in foundations)
            {
                if (foundation.Targetable == false || foundation[ObjectData.STATUS] != 0)
                {
                    BuildingFoundations.Remove(foundation);
                }
            }

            // check for finished ops and remove foundations already worked on

            var builders = new HashSet<Unit>();
            foreach (var op in BuildOperations)
            {
                foundations.Remove(op.Building);

                if (op.Building.Targetable == false || op.Building[ObjectData.STATUS] != 0)
                {
                    foreach (var unit in op.Units)
                    {
                        op.RemoveUnit(unit);
                        builders.Add(unit);
                    }
                }

                if (op.UnitCount == 0)
                {
                    op.Stop();
                }
            }

            BuildOperations.RemoveAll(op => op.UnitCount == 0);

            // assign new ops

            while (BuildOperations.Count < 5)
            {
                if (foundations.Count == 0)
                {
                    break;
                }

                if (builders.Count == 0)
                {
                    foreach (var unit in Operation.GetFreeUnits(Unary).Where(u => u[ObjectData.CMDID] == (int)CmdId.VILLAGER))
                    {
                        builders.Add(unit);
                    }
                }

                if (builders.Count == 0)
                {
                    Unary.Log.Info($"Could not find enough builders");

                    break;
                }

                var builder = builders.First();
                var foundation = foundations.First();
                foreach (var f in foundations)
                {
                    if (f.Position.DistanceTo(builder.Position) < foundation.Position.DistanceTo(builder.Position))
                    {
                        foundation = f;
                    }
                }

                var op = new BuildOperation(Unary, foundation);
                op.AddUnit(builder);
                BuildOperations.Add(op);

                foundations.Remove(foundation);
                builders.Remove(builder);
            }

            Unary.Log.Debug($"Build operations: {BuildOperations.Count}");
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
