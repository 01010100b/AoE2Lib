using AoE2Lib;
using AoE2Lib.Bots.GameElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unary.UnitControllers.BuildingControllers;
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
            else if (cmdid == CmdId.CIVILIAN_BUILDING || cmdid == CmdId.MILITARY_BUILDING)
            {
                HandleBuilding();
            }
        }

        private void HandleVillager()
        {
            if (Unary.UnitsManager.GetControllers<HunterController>().Count < 3)
            {
                new HunterController(Unit, Unary);
            }
            else
            {
                new GathererController(Unit, Unary);
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
