using AoE2Lib.Bots;
using AoE2Lib.Bots.GameElements;
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
        protected Unary Unary => Controller.Unary;
        protected Unit Unit => Controller.Unit;

        public abstract int GetPriority();

        // return true if subsequent behaviours should be blocked from unit control
        // if perform is false then no unit control
        protected abstract bool Tick(bool perform);

        protected void MoveTo(Position position)
        {
            if (Unary.GameState.Map.IsOnMap(position))
            {
                Unit.Target(position, stance: UnitStance.NO_ATTACK);
            }
        }

        protected bool ShouldRareTick(int rate) => Unary.ShouldRareTick(this, rate);

        internal bool TickInternal(bool perform) => Tick(perform);
    }
}
