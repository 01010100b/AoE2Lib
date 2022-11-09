using AoE2Lib.Bots.GameElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unary.Behaviours
{
    internal class FarmingBehaviour : Behaviour
    {
        public override int GetPriority() => 400;
        public Unit Farm { get; set; } = null;

        protected override bool Tick(bool perform)
        {
            if (perform && Farm != null)
            {
                if (!Farm.Targetable)
                {
                    Farm = null;
                }

                if (Farm != null)
                {
                    Controller.Unit.Target(Farm);

                    return true;
                }
            }

            return false;
        }
    }
}
