using AoE2Lib.Bots.GameElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unary
{
    abstract class UnitController
    {
        public readonly Unit Unit;
        public readonly Unary Unary;

        public UnitController(Unit unit, Unary unary)
        {
            Unit = unit;
            Unary = unary;

            Unary.UnitsManager.SetController(Unit, this);
        }

        public void Update()
        {
            Tick();
        }

        protected abstract void Tick();
    }
}
