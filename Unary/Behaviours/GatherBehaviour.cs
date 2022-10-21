using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unary.Behaviours
{
    internal class GatherBehaviour : Behaviour
    {
        public override int GetPriority() => 200;

        protected override bool Tick(bool perform)
        {
            throw new NotImplementedException();
        }
    }
}
