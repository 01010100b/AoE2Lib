using AoE2Lib;
using AoE2Lib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unary.Operations;

namespace Unary.Managers
{
    class EconomyManager : Manager
    {
        public EconomyManager(Unary unary) : base(unary)
        {

        }

        public override void Update()
        {
            var info = Unary.InfoModule;
            info.StrategicNumbers[StrategicNumber.MAXIMUM_FOOD_DROP_DISTANCE] = -2;
            info.StrategicNumbers[StrategicNumber.MAXIMUM_GOLD_DROP_DISTANCE] = -2;
            info.StrategicNumbers[StrategicNumber.MAXIMUM_HUNT_DROP_DISTANCE] = -2;
            info.StrategicNumbers[StrategicNumber.MAXIMUM_STONE_DROP_DISTANCE] = -2;
            info.StrategicNumbers[StrategicNumber.MAXIMUM_WOOD_DROP_DISTANCE] = -2;
            info.StrategicNumbers[StrategicNumber.CAP_CIVILIAN_GATHERERS] = 0;
            info.StrategicNumbers[StrategicNumber.MINIMUM_BOAR_HUNT_GROUP_SIZE] = 0;

            // train vills
            const int VILLAGER = 83;

            var units = Unary.UnitsModule;

            units.AddUnitType(VILLAGER);

            Unary.ProductionManager.Train(VILLAGER);
        }
    }
}
