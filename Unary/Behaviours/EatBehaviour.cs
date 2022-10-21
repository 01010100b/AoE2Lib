using AoE2Lib.Bots.GameElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unary.Behaviours
{
    internal class EatBehaviour : Behaviour
    {
        public Unit Target { get; set; } = null;

        public override int GetPriority() => 600;

        protected override bool Tick(bool perform)
        {
            if (perform && Target != null)
            {
                Controller.Unit.Target(Target);
                Target.RequestUpdate();

                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
