using AoE2Lib;
using AoE2Lib.Bots.GameElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unary.Operations;

namespace Unary.Managers
{
    class BuildingManager : Manager
    {
        private BuildOperation BuildOperation { get; set; }

        public BuildingManager(Unary unary) : base(unary)
        {

        }

        internal override void Update()
        {
            //Unary.SetStrategicNumber(StrategicNumber.CAP_CIVILIAN_BUILDERS, 0);
            Unary.GameState.SetStrategicNumber(StrategicNumber.DISABLE_BUILDER_ASSISTANCE, 1);
            Unary.GameState.SetStrategicNumber(StrategicNumber.ENABLE_NEW_BUILDING_SYSTEM, 1);
            Unary.GameState.SetStrategicNumber(StrategicNumber.INITIAL_EXPLORATION_REQUIRED, 0);

            Unary.GameState.SetStrategicNumber(StrategicNumber.MAXIMUM_TOWN_SIZE, 20);
            if (Unary.GameState.GetTechnology(101).State == ResearchState.COMPLETE)
            {
                Unary.GameState.SetStrategicNumber(StrategicNumber.MAXIMUM_TOWN_SIZE, 25);
            }

            if (Unary.GameState.GetTechnology(102).State == ResearchState.COMPLETE)
            {
                Unary.GameState.SetStrategicNumber(StrategicNumber.MAXIMUM_TOWN_SIZE, 30);
            }

            if (Unary.GameState.GetTechnology(103).State == ResearchState.COMPLETE)
            {
                Unary.GameState.SetStrategicNumber(StrategicNumber.MAXIMUM_TOWN_SIZE, 35);
            }

            BuildHouses();
            //DoBuildingOperations();
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
                house.Build(1000, pending, (int)Priority.HOUSING);
            }
        }

        private void DoBuildingOperations()
        {
            if (BuildOperation == null)
            {
                BuildOperation = new BuildOperation(Unary.OperationsManager);
            }

            var free_foundations = new HashSet<Unit>();
            foreach (var unit in Unary.GameState.GetAllUnits())
            {
                if (unit.PlayerNumber != Unary.PlayerNumber)
                {
                    continue;
                }

                if (unit.Targetable == false || unit.Updated == false)
                {
                    continue;
                }

                if (unit[ObjectData.STATUS] != 0)
                {
                    continue;
                }

                if (unit[ObjectData.CMDID] != (int)CmdId.CIVILIAN_BUILDING && unit[ObjectData.CMDID] != (int)CmdId.MILITARY_BUILDING)
                {
                    continue;
                }

                free_foundations.Add(unit);
            }

            foreach (var build in Unary.OperationsManager.Operations.OfType<BuildOperation>().Cast<BuildOperation>())
            {
                foreach (var foundation in build.Foundations)
                {
                    free_foundations.Remove(foundation);
                }
            }

            BuildOperation.Foundations.Clear();
            BuildOperation.Foundations.AddRange(free_foundations);

            var builders = BuildOperation.Units.Count();
            if (builders < BuildOperation.Foundations.Count && builders < 4)
            {
                var free_vill = Unary.OperationsManager.FreeUnits.Where(u => u[ObjectData.CMDID] == (int)CmdId.VILLAGER).FirstOrDefault();
                if (free_vill != null)
                {
                    BuildOperation.AddUnit(free_vill);
                }
            }

            if (BuildOperation.Foundations.Count == 0)
            {
                BuildOperation.ClearUnits();
            }
        }
    }
}
