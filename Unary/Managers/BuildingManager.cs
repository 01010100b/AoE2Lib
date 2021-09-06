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
        private static readonly Point[] TC_FARM_DELTAS = new[] { new Point(2, 3), new Point(-1, 3), new Point(3, 0), new Point(3, -3), new Point(-4, 2), new Point(-4, -1), new Point(0, -4), new Point(-3, -4) };
        private static readonly Point[] MILL_FARM_DELTAS = new[] { new Point(-1, 2), new Point(2, -1), new Point(2, 2), new Point(-3, -1), new Point(-1, -3) };

        private readonly HashSet<Unit> BuildingFoundations = new HashSet<Unit>();
        private readonly List<BuildOperation> BuildOperations = new List<BuildOperation>();

        public BuildingManager(Unary unary) : base(unary)
        {

        }

        public IEnumerable<Tile> GetBuildingFootprint(int x, int y, int width, int height)
        {
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

            for (int cx = x_start; cx <= x_end; cx++)
            {
                for (int cy = y_start; cy <= y_end; cy++)
                {
                    yield return Unary.GameState.Map.GetTile(cx, cy);
                }
            }
        }

        public IEnumerable<Tile> GetBuildingPlacements(UnitType building)
        {
            if (building.Id == 50)
            {
                return GetFarmPlacements();
            }

            throw new NotImplementedException();
        }

        internal override void Update()
        {
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

            foreach (var unit in Unary.GameState.MyPlayer.GetUnits().Where(u => u.Targetable && u[ObjectData.STATUS] == 0))
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

            Unary.Log.Info($"Build operations: {BuildOperations.Count}");
        }

        private IEnumerable<Tile> GetFarmPlacements()
        {
            var sites = new List<Unit>();

            foreach (var unit in Unary.GameState.MyPlayer.GetUnits().Where(u => u.Targetable))
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
