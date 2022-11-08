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
        public Tile Tile { get; set; } = null;

        protected override bool Tick(bool perform)
        {
            if (perform && Tile != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
