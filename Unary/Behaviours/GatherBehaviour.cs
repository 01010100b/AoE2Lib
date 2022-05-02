using AoE2Lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unary.Behaviours
{
    internal class GatherBehaviour : Behaviour
    {
        public Resource Resource { get; private set; }
        public Controller Dropsite { get; private set; }

        protected override bool Tick(bool perform)
        {
            throw new NotImplementedException();
        }
    }
}
