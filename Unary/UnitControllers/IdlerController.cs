using AoE2Lib;
using AoE2Lib.Bots;
using AoE2Lib.Bots.GameElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unary.UnitControllers.BuildingControllers;
using Unary.UnitControllers.MilitaryControllers;
using Unary.UnitControllers.VillagerControllers;

namespace Unary.UnitControllers
{
    class IdlerController : UnitController
    {
        public IdlerController(Unit unit, Unary unary) : base(unit, unary)
        {

        }

        protected override void Tick()
        {
            var cmdid = (CmdId)Unit[ObjectData.CMDID];

            if (cmdid == CmdId.VILLAGER)
            {
                HandleVillager();
            }
            else if (cmdid == CmdId.MILITARY)
            {
                HandleMilitary();
            }
            else if (cmdid == CmdId.CIVILIAN_BUILDING || cmdid == CmdId.MILITARY_BUILDING)
            {
                HandleBuilding();
            }
        }

        private void HandleVillager()
        {
            new GathererController(Unit, Unary);
        }

        private void HandleMilitary()
        {
            if (Unary.GameState.Tick < 50 || GetHashCode() % 53 == Unary.GameState.Tick % 53)
            {
                var scouts = Unary.OldUnitsManager.GetControllers<ScoutController>();
                if (scouts.Count == 0)
                {
                    new ScoutController(Unit, Unary);

                    return;
                }
                else if (Unit[ObjectData.SPEED] > scouts[0].Unit[ObjectData.SPEED])
                {
                    new IdlerController(scouts[0].Unit, Unary);
                    new ScoutController(Unit, Unary);

                    return;
                }
            }

            if (GetHashCode() % 3 == Unary.GameState.Tick % 3)
            {
                if (Unary.StrategyManager.Attacking != null)
                {
                    new AttackerController(Unit, Unary);

                    return;
                }
            }
        }

        private void HandleBuilding()
        {
            var type = Unit[ObjectData.BASE_TYPE];
            var mod = Unary.Mod;
            
            if (type == mod.TownCenter || type == mod.Mill || type == mod.LumberCamp || type == mod.MiningCamp)
            {
                new DropsiteController(Unit, Unary);

                return;
            }
            else
            {
                new GenericBuildingController(Unit, Unary);

                return;
            }
        }
    }
}
