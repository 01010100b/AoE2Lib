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

        public override void Update()
        {
            var info = Unary.InfoModule;
            info.StrategicNumbers[StrategicNumber.CAP_CIVILIAN_EXPLORERS] = 0;
            info.StrategicNumbers[StrategicNumber.CAP_CIVILIAN_BUILDERS] = 1;
            info.StrategicNumbers[StrategicNumber.DISABLE_BUILDER_ASSISTANCE] = 1;

            BuildHouses();
            DoBuildingOperations();
        }

        private void BuildHouses()
        {
            const int HOUSE = 70;

            var units = Unary.UnitsModule;
            var info = Unary.InfoModule;

            units.AddUnitType(HOUSE);

            if (info.PopulationHeadroom > 0 && info.HousingHeadroom < 5 && units.UnitTypes[HOUSE].Pending == 0)
            {
                Unary.ProductionManager.Build(HOUSE, new List<Position>(), 1000, 1);
                Unary.Log.Info("Building house");
            }
        }

        private void DoBuildingOperations()
        {
            if (BuildOperation == null)
            {
                BuildOperation = new BuildOperation(Unary.OperationsManager);
            }

            var foundations = new HashSet<Unit>();
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

                foundations.Add(unit);
            }

            foreach (var build in Unary.OperationsManager.Operations.OfType<BuildOperation>().Cast<BuildOperation>())
            {
                foreach (var foundation in build.Foundations)
                {
                    foundations.Remove(foundation);
                }
            }

            foreach (var foundation in foundations)
            {
                BuildOperation.Foundations.Add(foundation);
            }

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
