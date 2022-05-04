using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unary.Behaviours
{
    internal abstract class Behaviour
    {
        public Controller Controller { get; internal set; } = null;
        
        protected TimeSpan LastPerformedGameTime { get; private set; } = TimeSpan.Zero;

        // return true if the subsequent behaviours should be blocked from unit control
        // if perform is false then no unit control
        protected abstract bool Tick(bool perform);
        
        internal bool TickInternal(bool perform)
        {
            var performed = Tick(perform);

            if (performed)
            {
                LastPerformedGameTime = Controller.Unary.GameState.GameTime;
            }

            return performed;
        }
    }
}
