using AoE2Lib;
using AoE2Lib.Bots.GameElements;
using AoE2Lib.Utils;
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
            Unary.SetStrategicNumber(StrategicNumber.DISABLE_BUILDER_ASSISTANCE, 1);
            Unary.SetStrategicNumber(StrategicNumber.ENABLE_NEW_BUILDING_SYSTEM, 1);
            Unary.SetStrategicNumber(StrategicNumber.INITIAL_EXPLORATION_REQUIRED, 0);

            Unary.SetStrategicNumber(StrategicNumber.MAXIMUM_TOWN_SIZE, 25);
            if (Unary.GetTechnology(101).State == ResearchState.COMPLETE)
            {
                Unary.SetStrategicNumber(StrategicNumber.MAXIMUM_TOWN_SIZE, 30);
            }

            if (Unary.GetTechnology(102).State == ResearchState.COMPLETE)
            {
                Unary.SetStrategicNumber(StrategicNumber.MAXIMUM_TOWN_SIZE, 35);
            }

            if (Unary.GetTechnology(103).State == ResearchState.COMPLETE)
            {
                Unary.SetStrategicNumber(StrategicNumber.MAXIMUM_TOWN_SIZE, 40);
            }

            BuildHouses();
            //DoBuildingOperations();
        }

        private void BuildHouses()
        {
            var house = Unary.GetUnitType(70);
            var info = Unary.InfoModule;

            var margin = 5;
            var pending = 1;

            if (Unary.GetTechnology(101).State == ResearchState.COMPLETE)
            {
                margin = 10;
            }

            if (Unary.GetTechnology(102).State == ResearchState.COMPLETE)
            {
                pending = 2;
            }

            if (info.PopulationHeadroom > 0 && info.HousingHeadroom < margin && house.Pending < pending)
            {
                house.Build(1000, pending, (int)Priority.HOUSING);
                Unary.Log.Info("Building house");
            }
        }

        private void DoBuildingOperations()
        {
            if (BuildOperation == null)
            {
                BuildOperation = new BuildOperation(Unary.OperationsManager);
            }

            var free_foundations = new HashSet<Unit>();
            foreach (var unit in Unary.UnitsModule.Units.Values)
            {
                if (unit.PlayerNumber != Unary.InfoModule.PlayerNumber)
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
