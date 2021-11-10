using AoE2Lib.Bots.GameElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unary.UnitControllers
{
    abstract class VillagerController : UnitController
    {
        public VillagerController(Unit unit, Unary unary) : base(unit, unary)
        {
            
        }

        protected override void Tick()
        {
            VillagerTick();
        }

        protected abstract void VillagerTick();
    }
}
