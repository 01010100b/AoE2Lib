using AoE2Lib.Bots;
using AoE2Lib.Bots.GameElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unary.Behaviours
{
    internal abstract class CombatBehaviour : Behaviour
    {
        public Unit Target { get; private set; } = null;
        public Unit Backup { get; private set; } = null;
        public Unit Threat { get; private set; } = null;

        private bool OppositeDirection { get; set; }

        public CombatBehaviour()
        {
            OppositeDirection = GetHashCode() % 2 == 0;
        }

        protected abstract Unit ChooseTarget(out Unit backup);

        protected abstract Position PerformCombat(out bool attack);

        protected Position GetThreatAvoidanceDelta()
        {
            var delta_pos = new Position(0, 0);

            if (Threat == null || !Threat.Targetable || !Threat.Visible)
            {
                return delta_pos;
            }

            var range = Threat[ObjectData.RANGE];
            var my_pos = Controller.Unit.Position;
            var distance = my_pos.DistanceTo(Threat.Position);

            if (distance < range + 1)
            {
                // within threat range

                if (range <= 2)
                {
                    // melee threat

                    delta_pos = GetRunAwayDelta();
                }
                else
                {
                    // ranged threat

                    delta_pos = GetProjectileAvoidanceDelta();
                }

                var my_range = Controller.Unit[ObjectData.RANGE];

                if (my_pos.DistanceTo(Target.Position) < my_range)
                {
                    delta_pos += 0.5 * (my_pos - Target.Position).Normalize();
                }
                else if (my_pos.DistanceTo(Threat.Position) < my_range)
                {
                    delta_pos += 0.5 * (my_pos - Threat.Position).Normalize();
                }
            }

            return delta_pos;
        }

        protected override sealed bool Tick(bool perform)
        {
            if (Target != null && !Target.Targetable)
            {
                Target = null;
                Backup = null;
            }

            if (!perform)
            {
                Target = null;
                Backup = null;

                return false;
            }

            Target = ChooseTarget(out var backup);
            Backup = backup;

            if (Backup == null)
            {
                Backup = Target;
            }

            if (Target != null)
            {
                FindBiggestThreat();
                
                var pos = PerformCombat(out var attack);

                if (!IsAccessible(pos))
                {
                    Controller.Unary.Log.Debug($"Unit {Controller.Unit.Id} is stuck!");
                    OppositeDirection = !OppositeDirection;

                    for (int i = 0; i < 10; i++)
                    {
                        var angle = Controller.Unary.Rng.NextDouble() * 2 * Math.PI;
                        var delta_pos = Position.FromPolar(angle, 1);

                        pos = Controller.Unit.Position + delta_pos;

                        if (IsAccessible(pos))
                        {
                            break;
                        }
                    }

                    if (!IsAccessible(pos))
                    {
                        pos = Target.Position;
                    }
                }

                var next_attack = Math.Max(1, Controller.Unit[ObjectData.RELOAD_TIME] - 500);

                if (attack)
                {
                    Controller.Unit.Target(Target, UnitAction.DEFAULT, null, null, int.MinValue, 0, Backup);
                    Controller.Unit.Target(pos, UnitAction.MOVE, null, UnitStance.NO_ATTACK, 1, next_attack);
                }
                else
                {
                    Controller.Unit.Target(pos, UnitAction.MOVE, null, UnitStance.NO_ATTACK, int.MinValue, next_attack);
                }
                
                Controller.Unit.RequestUpdate();
                Target.RequestUpdate();
                Backup.RequestUpdate();
                Threat.RequestUpdate();

                return true;
            }
            else
            {
                return false;
            }
        }

        private void FindBiggestThreat()
        {
            Threat = null;
            var score = double.MinValue;

            foreach (var threat in Controller.Unary.SitRepManager.Threats)
            {
                var s = 1d / Math.Max(1, threat.Position.DistanceTo(Controller.Unit.Position));

                if (Threat == null || s > score)
                {
                    Threat = threat;
                    score = s;
                }
            }
            
            if (Threat == null)
            {
                Threat = Target;
            }
        }

        private bool IsAccessible(Position position, bool land = true)
        {
            if (Controller.Unary.GameState.Map.TryGetTile(position, out var tile))
            {
                var sitrep = Controller.Unary.SitRepManager[tile];

                if (land)
                {
                    if (!sitrep.IsLandAccessible)
                    {
                        return false;
                    }
                }
                else
                {
                    if (!sitrep.IsWaterAccessible)
                    {
                        return false;
                    }
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        private Position GetRunAwayDelta()
        {
            return (Controller.Unit.Position - Threat.Position).Normalize();
        }

        private Position GetProjectileAvoidanceDelta()
        {
            var settings = Controller.Unary.Settings;
            var ballistics = Threat[ObjectData.BALLISTICS] > 0;
            var my_pos = Controller.Unit.Position;
            var delta_pos = (Threat.Position - my_pos).Normalize().Rotate(Math.PI / 2);
            var bias = settings.CombatMovementBias;
            var tick = Controller.Unary.GameState.Tick;
            var zigzag = tick % bias == 0;

            if (OppositeDirection)
            {
                delta_pos *= -1;
            }

            if (ballistics && tick % 2 == 0)
            {
                delta_pos *= -1;

                if (zigzag)
                {
                    delta_pos *= -1;
                }
            }

            return delta_pos.Normalize();
        }
    }
}
