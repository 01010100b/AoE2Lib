using AoE2Lib;
using AoE2Lib.Bots;
using AoE2Lib.Bots.GameElements;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Unary.Managers.MilitaryManager;

namespace Unary.UnitControllers.MilitaryControllers
{
    class ScoutController : MilitaryController
    {
        public Position AttractorPosition { get; set; } = Position.Zero;
        public double AttractorRadius { get; set; } = 0;
        public double ExploredFraction { get; set; } = 0.7;
        public ScoutingState State { get; private set; } = null;

        private TimeSpan LastDistanceChangeGameTime { get; set; } = TimeSpan.MinValue;
        private double LastDistance { get; set; } = 0;

        public ScoutController(Unit scout, Unary unary) : base(scout, unary)
        {

        }

        protected override void MilitaryTick()
        {
            var deer = Unary.GameState.Gaia.Units.Where(u => u.Targetable && u[ObjectData.CLASS] == (int)UnitClass.PreyAnimal && u[ObjectData.HITPOINTS] > 0).Count();

            if (deer > 0)
            {
                if (Unary.UnitsManager.GetControllers<DeerPusherController>().Count == 0)
                {
                    new DeerPusherController(Unit, Unary);

                    return;
                }
            }

            if (State == null)
            {
                FindState();
            }
            
            if (State != null)
            {
                ExploreState();
            }
        }

        private void FindState()
        {
            var los = 4;

            ScoutingState best = null;
            var best_cost = double.MaxValue;
            var my_pos = Unit.Position;

            foreach (var state in Unary.MilitaryManager.GetScoutingStatesForLos(los))
            {
                if (state.LastAttemptGameTime > TimeSpan.Zero)
                {
                    continue;
                }
                
                var total = 0d;
                var explored = 0d;

                foreach (var tile in Unary.GameState.Map.GetTilesInRange(state.Tile.Position, los))
                {
                    total++;
                    if (tile.Explored)
                    {
                        explored++;
                    }
                }

                explored /= Math.Max(1, total);
                if (explored < ExploredFraction)
                {
                    
                    var cost = state.Tile.Position.DistanceTo(my_pos) 
                        + Math.Abs(AttractorRadius - state.Tile.Position.DistanceTo(AttractorPosition));
                    
                    if (best == null || cost < best_cost)
                    {
                        best = state;
                        best_cost = cost;
                    }
                }
            }

            if (best != null)
            {
                State = best;
                Unary.Log.Info($"Scouting {best.Tile.Position}");
            }
        }

        private void ExploreState()
        {
            State.LastAttemptGameTime = Unary.GameState.GameTime;
            var target_pos = State.Tile.Center;
            var distance = target_pos.DistanceTo(Unit.Position);

            if (Math.Abs(distance - LastDistance) > 1)
            {
                LastDistanceChangeGameTime = Unary.GameState.GameTime;
                LastDistance = distance;
            }

            var time = Unary.GameState.GameTime - LastDistanceChangeGameTime;

            if (target_pos.DistanceTo(Unit.Position) > 1 && time < TimeSpan.FromSeconds(3))
            {
                Unit.Target(target_pos, UnitAction.MOVE);
            }
            else
            {
                State = null;
            }
        }
    }
}
