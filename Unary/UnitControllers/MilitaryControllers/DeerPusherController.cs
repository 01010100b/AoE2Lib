using AoE2Lib;
using AoE2Lib.Bots;
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
            if (Deer == null || !Deer.Targetable || Deer[ObjectData.HITPOINTS] == 0)
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
            var deer = Unary.OldEconomyManager.GetDeer().ToList();
            deer.Sort((a, b) => a.Position.DistanceTo(Unary.GameState.MyPosition).CompareTo(b.Position.DistanceTo(Unary.GameState.MyPosition)));

            if (deer.Count > 0)
            {
                Deer = deer[0];
            }
            else
            {
                Deer = null;
            }
        }

        private void PushDeer()
        {
            var best_pos = Deer.Position;
            var best_distance = Unary.OldMapManager.GetPathDistance(Deer.Tile);

            foreach (var tile in Deer.Tile.GetNeighbours(true).Where(t => Unary.OldMapManager.CanReach(t)))
            {
                var distance = Unary.OldMapManager.GetPathDistance(tile);

                if (distance <= best_distance)
                {
                    best_distance = distance;

                    var dpos = tile.Center - Deer.Position;
                    dpos = dpos.Normalize();
                    dpos *= -1;
                    dpos *= 0.9;
                    dpos += Deer.Position;

                    if (Unary.GameState.Map.IsOnMap(dpos))
                    {
                        var t = Unary.GameState.Map.GetTile(dpos);

                        if (Unary.OldMapManager.CanReach(t))
                        {
                            best_pos = dpos;
                            best_distance = distance;
                        }
                    }
                }
            }

            Unit.Target(best_pos);
        }
    }
}
