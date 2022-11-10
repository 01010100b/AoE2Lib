using AoE2Lib.Bots;
using AoE2Lib.Bots.GameElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unary.Behaviours
{
    internal class FightingBehaviour : Behaviour
    {
        public Unit Target { get; set; } = null;
        public Unit Backup { get; set; } = null;
        public Unit Threat { get; set; } = null;

        private bool Reversed { get; set; }

        public FightingBehaviour() : base()
        {
            Reversed = GetHashCode() % 2 == 0;
        }

        public override int GetPriority() => 1000;

        protected override bool Tick(bool perform)
        {
            if (perform && Target != null)
            {
                var range = Unit[ObjectData.RANGE];
                var distance = Unit.Position.DistanceTo(Target.Position);

                if (distance > range + 5)
                {
                    MoveTo(Target.Position);
                }
                else
                {
                    if (range > 2)
                    {
                        CombatRanged();
                    }
                }

                Unit.RequestUpdate();
                Target.RequestUpdate();
                Backup?.RequestUpdate();
                Threat?.RequestUpdate();

                return true;
            }
            else
            {
                return false;
            }
        }

        private void CombatRanged()
        {
            var range = Unit[ObjectData.RANGE];
            var distance = Unit.Position.DistanceTo(Target.Position);
            var delta_pos = Position.Zero;
            
            if (distance > range + 1)
            {
                delta_pos = (Target.Position - Unit.Position).Normalize();
            }
            else if (distance < range)
            {
                delta_pos = -1 * (Target.Position - Unit.Position).Normalize();
            }

            var pos = GetMovementPosition(delta_pos);
            Perform(pos, true);
        }

        private void Perform(Position move, bool attack)
        {
            var max_next_attack = Unit[ObjectData.RELOAD_TIME] - 500;
            var min_next_attack = attack ? 1 : 0;

            Unit.Target(move, stance: UnitStance.NO_ATTACK, min_next_attack: min_next_attack, max_next_attack: max_next_attack);
            
            if (attack)
            {
                Unit.Target(Target, min_next_attack: 0, max_next_attack: 0, backup: Backup);
            }
        }

        private Position GetMovementPosition(Position delta_pos)
        {
            var speed = Unit[ObjectData.SPEED] / 100d;
            var pos_mul = 2 * Unary.GameState.GameTimePerTick.TotalSeconds * speed;
            var avoid_pos = Position.Zero;

            if (Threat != null)
            {
                avoid_pos = GetThreatAvoidanceDelta();
            }

            var pos = Unit.Position + ((delta_pos + avoid_pos).Normalize() * pos_mul);

            if (!CanMove(pos))
            {
                Reversed = !Reversed;

                if (Threat != null)
                {
                    avoid_pos = GetThreatAvoidanceDelta();
                }

                pos = Unit.Position + ((delta_pos + avoid_pos).Normalize() * pos_mul);
            }

            if (!CanMove(pos))
            {
                for (int i = 0; i < 10; i++)
                {
                    avoid_pos = Position.FromPolar(Unary.Rng.NextDouble() * 2 * Math.PI, 1);
                    pos = Unit.Position + ((delta_pos + avoid_pos).Normalize() * pos_mul);

                    if (CanMove(pos))
                    {
                        break;
                    }
                }
            }

            return pos;
        }

        private Position GetThreatAvoidanceDelta()
        {
            var range = Threat[ObjectData.RANGE];
            var distance = Unit.Position.DistanceTo(Threat.Position);

            if (distance < range + 1)
            {
                var pos = (Threat.Position - Unit.Position).Normalize();
                pos = pos.Rotate(Math.PI / 2);

                if (Reversed)
                {
                    pos *= -1;
                }

                var ballistics = Threat[ObjectData.BALLISTICS] == 1;

                if (ballistics && ShouldRareTick(2))
                {
                    pos *= -1;
                }

                return Math.Max(0, Unary.Settings.ThreatAvoidanceFactor) * pos.Normalize();
            }
            else
            {
                return Position.Zero;
            }
        }

        private bool CanMove(Position position)
        {
            if (Unary.GameState.Map.TryGetTile(position, out var tile))
            {
                if (Unary.MapManager.CanPass(Unit, tile))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
