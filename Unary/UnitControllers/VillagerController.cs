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
        public BuildingController Building { get; set; } = null;

        public VillagerController(Unit unit, Unary unary) : base(unit, unary)
        {
            
        }

        protected override void Tick()
        {
            if (Building != null)
            {
                Build();

                return;
            }

            VillagerTick();
        }

        protected abstract void VillagerTick();

        private void Build()
        {
            if (Building.MaxBuilders <= 0)
            {
                Building = null;

                return;
            }

            if (Building.AssignedBuilders > Building.MaxBuilders && GetHashCode() % 10 == Unary.GameState.Tick % 10)
            {
                Building = null;

                return;
            }

            if (Unit.GetTarget() != Building.Unit)
            {
                Unit.Target(Building.Unit);
            }
        }
    }
}
