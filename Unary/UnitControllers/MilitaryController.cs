using AoE2Lib.Bots.GameElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unary.UnitControllers
{
    abstract class MilitaryController : UnitController
    {
        public MilitaryController(Unit unit, Unary unary) : base(unit, unary)
        {

        }

        protected override void Tick()
        {
            MilitaryTick();
        }

        protected abstract void MilitaryTick();
    }
}
