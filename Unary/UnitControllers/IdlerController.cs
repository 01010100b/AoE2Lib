using AoE2Lib;
using AoE2Lib.Bots.GameElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            if (Unit[ObjectData.CMDID] == (int)CmdId.VILLAGER)
            {
                new GathererController(Unit, Unary);
            }
        }
    }
}
