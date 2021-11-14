using AoE2Lib;
using AoE2Lib.Bots.GameElements;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unary.UnitControllers.MilitaryControllers
{
    class DeerPusherController : MilitaryController
    {
        public Unit Deer { get; private set; } = null;

        public DeerPusherController(Unit unit, Unary unary) : base(unit, unary)
        {

        }

        protected override void MilitaryTick()
        {
            if (Deer == null || !Deer.Targetable)
            {
                ChooseDeer();

                if (Deer == null)
                {
                    new IdlerController(Unit, Unary);
                }
            }
            else
            {
                PushDeer();
            }

            if (Deer != null)
            {
                Deer.RequestUpdate();
            }
        }

        private void ChooseDeer()
        {
            Unary.Log.Debug($"Choosing deer {Unit.Id}");

            var deer = Unary.GameState.Gaia.Units.Where(u => u.Targetable && u[ObjectData.CLASS] == (int)UnitClass.PreyAnimal && u[ObjectData.HITPOINTS] > 0).ToList();
            deer.Sort((a, b) => a.Position.DistanceTo(Unary.GameState.MyPosition).CompareTo(b.Position.DistanceTo(Unary.GameState.MyPosition)));

            if (deer.Count > 0)
            {
                Deer = deer[0];
            }
            else
            {
                Unary.Log.Debug($"Failed to choose deer {Unit.Id}");
            }
        }

        private void PushDeer()
        {
            var best_distance = int.MaxValue;
            var best_tile = Deer.Tile;

            foreach (var tile in Deer.Tile.GetNeighbours(true).Where(t => Unary.MapManager.CanReach(t)))
            {
                var distance = Unary.MapManager.GetPathDistance(tile);

                if (distance <= best_distance)
                {
                    best_distance = distance;
                    best_tile = tile;
                }
            }

            var dpos = best_tile.Center - Deer.Position;
            dpos = dpos.Normalize();
            dpos *= -1;
            dpos += Deer.Position;

            Unit.Target(dpos);

            Debug.WriteLine($"Pushing deer at {Deer.Position} from {dpos} pusher at {Unit.Position} with precise move {Unit[ObjectData.PRECISE_MOVE_X]},{Unit[ObjectData.PRECISE_MOVE_Y]}");
        }
    }
}
