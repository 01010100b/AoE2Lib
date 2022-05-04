using AoE2Lib.Bots;
using AoE2Lib.Bots.GameElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unary.Behaviours
{
    internal class CombatRangedBehaviour : CombatBehaviour
    {
        private bool FlipSides { get; set; } = false;

        public CombatRangedBehaviour() : base()
        {
            FlipSides = GetHashCode() % 101 <= 50;
        }

        protected override void ChooseTarget(out Unit target, out Unit backup)
        {
            var pos = Controller.Unit.Position;
            var targets = AllTargets.ToList();
            targets.Sort((a, b) => a.Position.DistanceTo(pos).CompareTo(b.Position.DistanceTo(pos)));

            target = targets[0];
            backup = targets[0];

            if (targets.Count > 1)
            {
                backup = targets[1];
            }
        }

        protected override void DoCombat()
        {
            var next_attack = Math.Max(1, Controller.Unit[ObjectData.RELOAD_TIME] - 500);
            var ballistics = false;

            if (Target[ObjectData.BALLISTICS] > 0)
            {
                ballistics = true;
            }

            // move perpendicular to unit->target vector

            var my_pos = Controller.Unit.Position;
            var delta_pos = (Target.Position - my_pos).Normalize();
            delta_pos = 2 * delta_pos.Rotate(Math.PI / 2);

            if (FlipSides)
            {
                delta_pos *= -1;
            }

            // if ballistics then zig-zag

            if (ballistics && Controller.Unary.GameState.Tick % 2 == 0)
            {
                delta_pos *= -1;
            }

            // move away from target or closest threat

            var range = (double)Controller.Unit[ObjectData.RANGE];

            if (Target.Position.DistanceTo(my_pos) < range - 1)
            {
                delta_pos -= (Target.Position - my_pos).Normalize();
            }
            else
            {
                var closest = Target;
                foreach (var target in AllTargets.Where(t => t.Targetable))
                {
                    if (target.Position.DistanceTo(my_pos) < closest.Position.DistanceTo(my_pos))
                    {
                        closest = target;
                    }
                }

                if (closest.Position.DistanceTo(my_pos) < range - 1)
                {
                    delta_pos -= (closest.Position - my_pos).Normalize();
                }
            }

            // perform step

            var move_position = Controller.Unit.Position + delta_pos;

            if (Controller.Unary.Rng.NextDouble() < 0.8)
            {
                Controller.Unit.Target(Target, UnitAction.DEFAULT, null, null, int.MinValue, 0, Backup);
                Controller.Unit.Target(move_position, UnitAction.MOVE, null, UnitStance.NO_ATTACK, 1, next_attack);
            }
            else
            {
                Controller.Unit.Target(move_position, UnitAction.MOVE, null, UnitStance.NO_ATTACK, int.MinValue, next_attack);
            }
        }
    }
}
