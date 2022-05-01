using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unary.Behaviours
{
    internal abstract class Behaviour
    {
        public Controller Controller { get; internal set; }

        // return true if the subsequent behaviours should be blocked from unit control
        // if perform is false then no unit control
        protected internal abstract bool Tick(bool perform);
    }
}
