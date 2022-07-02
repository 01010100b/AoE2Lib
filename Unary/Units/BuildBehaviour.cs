using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unary.Units
{
    internal class BuildBehaviour : Behaviour
    {
        public ConstructionSpotBehaviour ConstructionSpot { get; set; } = null;

        protected internal override int Priority => 300;

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
