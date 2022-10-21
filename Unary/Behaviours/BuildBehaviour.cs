using AoE2Lib.Bots.GameElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unary.Behaviours
{
    internal class BuildBehaviour : Behaviour
    {
        public ConstructionSpotBehaviour ConstructionSpot { get; set; } = null;
        public Unit Construction { get; set; } = null;

        public override int GetPriority() => 800;

        protected override bool Tick(bool perform)
        {
            /*
            if (ConstructionSpot != null)
            {
                Controller.Unit.Target(ConstructionSpot.Controller.Unit);
                Controller.Unit.RequestUpdate();

                return true;
            }
            else
            {
                return false;
            }
            */
            if (Construction != null)
            {
                Controller.Unit.Target(Construction);
                Construction.RequestUpdate();

                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
