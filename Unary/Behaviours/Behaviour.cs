using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unary.Behaviours
{
    internal abstract class Behaviour
    {
        public Controller Controller { get; private set; }
        public int LastPerformedTick { get; private set; } = 0;

        protected abstract bool Perform();

        internal bool PerformInternal(Controller controller)
        {
            Controller = controller;
            var performed = Perform();

            if (performed)
            {
                LastPerformedTick = controller.Unary.GameState.Tick;
            }

            return performed;
        }
    }
}
