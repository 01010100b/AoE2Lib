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

        protected internal abstract bool Perform();
    }
}
