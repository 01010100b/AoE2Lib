using AoE2Lib.Bots.GameElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unary.Jobs;

namespace Unary.Behaviours
{
    internal class GatherBehaviour : Behaviour
    {
        public Unit Target { get; set; } = null;
        public Tile Tile { get; set; } = null;

        public override int GetPriority() => 500;

        protected override bool Tick(bool perform)
        {
            if (perform && Target != null)
            {
                Controller.Unit.Target(Target);
                Target.RequestUpdate();

                if (!Target.Targetable)
                {
                    Target = null;
                    Tile = null;
                }

                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
