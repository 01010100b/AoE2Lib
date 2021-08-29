using AoE2Lib;
using AoE2Lib.Bots.GameElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Unary.Managers
{
    class BuildingManager : Manager
    {
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
                house.BuildNormal(1000, pending, Priority.HOUSING);
            }
        }
    }
}
