using AoE2Lib.Bots;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unary.Behaviours
{
    internal class ConstructionSpotBehaviour : Behaviour
    {
        public int RequestedBuilders => GetRequestedBuilders();

        public override int GetPriority() => 0;

        protected override bool Tick(bool perform)
        {
            if (RequestedBuilders > 0)
            {
                Controller.Unit.RequestUpdate();
            }

            return false;
        }

        private int GetRequestedBuilders()
        {
            if (Controller.Unit[ObjectData.STATUS] == 0)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }
    }
}
