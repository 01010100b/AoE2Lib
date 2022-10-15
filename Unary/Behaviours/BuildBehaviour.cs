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

        public override int GetPriority() => 100;

        protected override bool Tick(bool perform)
        {
            if (ConstructionSpot != null)
            {
                Controller.Unit.Target(ConstructionSpot.Controller.Unit);

                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
