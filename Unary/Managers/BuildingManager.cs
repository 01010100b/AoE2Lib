using AoE2Lib;
using AoE2Lib.Bots;
using AoE2Lib.Bots.GameElements;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace Unary.Managers
{
    class BuildingManager : Manager
    {
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
            if (Unary.GameState.GetTechnology(101).State == ResearchState.COMPLETE)
            {
                Unary.GameState.SetStrategicNumber(StrategicNumber.CAP_CIVILIAN_BUILDERS, 8);
                Unary.GameState.SetStrategicNumber(StrategicNumber.MAXIMUM_TOWN_SIZE, 25);
            }

            if (Unary.GameState.GetTechnology(102).State == ResearchState.COMPLETE)
            {
                Unary.GameState.SetStrategicNumber(StrategicNumber.CAP_CIVILIAN_BUILDERS, 12);
                Unary.GameState.SetStrategicNumber(StrategicNumber.MAXIMUM_TOWN_SIZE, 30);
            }

            if (Unary.GameState.GetTechnology(103).State == ResearchState.COMPLETE)
            {
                Unary.GameState.SetStrategicNumber(StrategicNumber.CAP_CIVILIAN_BUILDERS, 16);
                Unary.GameState.SetStrategicNumber(StrategicNumber.MAXIMUM_TOWN_SIZE, 35);
            }

            BuildHouses();
        }

        private void BuildHouses()
        {
            var house = Unary.GameState.GetUnitType(70);

            var margin = 5;
            var pending = 1;

            if (Unary.GameState.GetTechnology(101).State == ResearchState.COMPLETE)
            {
                margin = 10;
            }

            if (Unary.GameState.GetTechnology(102).State == ResearchState.COMPLETE)
            {
                pending = 2;
            }

            if (Unary.GameState.MyPlayer.GetFact(FactId.POPULATION_HEADROOM) > 0 && Unary.GameState.MyPlayer.GetFact(FactId.HOUSING_HEADROOM) < margin && house.Pending < pending)
            {
                house.BuildNormal(1000, pending, Priority.HOUSING);
            }
        }

        private IEnumerable<Tile> GetFarmPlacements()
        {
            var sites = new List<Unit>();

            foreach (var unit in Unary.GameState.MyPlayer.Units.Where(u => u.Targetable))
            {
                if (unit[ObjectData.BASE_TYPE] == 109)
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
            var deltas = new[] { new Point(3, 3) };

            foreach (var delta in deltas)
            {
                var x = site.Position.PointX + delta.X;
                var y = site.Position.PointY + delta.Y;

                try
                {
                    yield return Unary.GameState.Map.GetTile(x, y);
                }
                finally
                {

                }
            }
        }
    }
}
