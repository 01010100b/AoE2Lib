using AoE2Lib.Bots;
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
        public TimeSpan TimeSinceLastPerformed => Controller.Unary.GameState.GameTime - LastPerformedGameTime;
        protected internal abstract int Priority { get; }
        
        private TimeSpan LastPerformedGameTime { get; set; } = TimeSpan.Zero;

        // return true if subsequent behaviours should be blocked from unit control
        // if perform is false then no unit control
        protected abstract bool Tick(bool perform);

        protected void MoveTo(Position position)
        {
            if (Controller.Unary.GameState.Map.IsOnMap(position))
            {
                Controller.Unit.Target(position);
            }
        }

        protected bool ShouldRareTick(int rate) => Controller.Unary.ShouldRareTick(this, rate);
        
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
