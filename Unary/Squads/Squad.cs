using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unary.Behaviours;

namespace Unary.Squads
{
    internal abstract class Squad
    {
        public abstract int Priority { get; }

        protected Unary Unary { get; private set; }
        protected readonly List<Controller> Controllers = new List<Controller>();

        protected abstract void Tick();

        internal void TickInternal(Unary unary, IEnumerable<Controller> controllers)
        {
            Unary = unary;
            Controllers.Clear();
            Controllers.AddRange(controllers);
            Tick();
        }
    }
}
