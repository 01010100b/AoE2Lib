using AoE2Lib.Bots;
using AoE2Lib.Bots.GameElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unary.Behaviours
{
    internal class CombatBehaviour : Behaviour
    {
        public Unit Target { get; private set; } = null;
        public Unit Backup { get; private set; } = null;

        private readonly List<Unit> Targets = new();
        private bool FlipSides { get; set; } = false;

        public CombatBehaviour() : base()
        {
            FlipSides = GetHashCode() % 2 == 0;
        }

        protected override bool Tick(bool perform)
        {
            if (!perform)
            {
                return false;
            }

            if (Target != null)
            {
                if (!Target.Targetable)
                {
                    Target = null;
                    Backup = null;
                }
            }

            if (Target == null)
            {
                ChooseTarget();
            }

            if (Target != null)
            {
                DoRangedCombat();
                Controller.Unit.RequestUpdate();
                Target.RequestUpdate();
                Backup.RequestUpdate();

                return true;
            }
            else
            {
                return false;
            }
        }

        private void ChooseTarget()
        {
            Targets.Clear();

            foreach (var enemy in Controller.Unary.GameState.CurrentEnemies.SelectMany(e => e.Units.Where(u => u.Targetable)))
            {
                Targets.Add(enemy);
                enemy.RequestUpdate();
            }

            if (Targets.Count > 0)
            {
                Targets.Sort((a, b) =>
                {
                    if (a[ObjectData.HITPOINTS] < b[ObjectData.HITPOINTS])
                    {
                        return -1;
                    }
                    else if (b[ObjectData.HITPOINTS] < a[ObjectData.HITPOINTS])
                    {
                        return 1;
                    }
                    else
                    {
                        return a.Position.DistanceTo(Controller.Unit.Position).CompareTo(b.Position.DistanceTo(Controller.Unit.Position));
                    }
                });

                Target = Targets[0];
                Backup = Target;

                if (Targets.Count > 1)
                {
                    Backup = Targets[1];
                }
            }
            else
            {
                Target = null;
                Backup = null;
            }
        }

        private void DoRangedCombat()
        {
            var next_attack = Math.Max(1, Controller.Unit[ObjectData.RELOAD_TIME] - 500);
            var ballistics = false;

            if (Controller.Unary.GameState.TryGetTechnology(93, out var tech))
            {
                if (tech.Completed)
                {
                    ballistics = true;
                }
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

            // move towards or away from target

            var range = (double)Controller.Unit[ObjectData.RANGE];

            if (Target.Position.DistanceTo(my_pos) < range - 1)
            {
                delta_pos -= (Target.Position - my_pos).Normalize();
            }
            else
            {
                var closest = Targets[0];
                foreach (var target in Targets.Where(t => t.Targetable))
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
