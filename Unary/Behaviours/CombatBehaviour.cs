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
        public IEnumerable<Unit> NearbyUnits => _NearbyUnits;

        private readonly List<Unit> _NearbyUnits = new();
        private bool OppositeDirection { get; set; }

        public CombatBehaviour()
        {
            OppositeDirection = GetHashCode() % 2 == 0;
        }

        protected abstract Unit ChooseTarget(out Unit backup);

        protected abstract void DoCombat();

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
                    // just run away

                    delta_pos = (my_pos - Threat.Position).Normalize();
                }
                else
                {
                    // avoid projectiles

                    var settings = Controller.Unary.Settings;
                    var ballistics = Threat[ObjectData.BALLISTICS] > 0;

                    delta_pos = (Threat.Position - my_pos).Normalize().Rotate(Math.PI / 2);

                    if (ballistics)
                    {
                        // enemy has ballistics, do zigzag

                        var bias = settings.CombatRangedMovementBias;
                        var zigzag = Controller.Unary.GameState.Tick % bias % 2 == 0;

                        if (zigzag)
                        {
                            delta_pos *= -1;
                        }
                    }

                    if (OppositeDirection)
                    {
                        delta_pos *= -1;
                    }

                    var my_range = Controller.Unit[ObjectData.RANGE];
                    var min_range = my_range * settings.CombatRangedMinRangeFraction;

                    if (my_pos.DistanceTo(Target.Position) < min_range)
                    {
                        delta_pos += 0.5 * (my_pos - Target.Position).Normalize();
                    }
                    else if (my_pos.DistanceTo(Threat.Position) < min_range)
                    {
                        delta_pos += 0.5 * (my_pos - Threat.Position).Normalize();
                    }
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
                _NearbyUnits.Clear();

                return false;
            }

            if (ShouldRareTick(13))
            {
                Controller.Unit.RequestUpdate();
                _NearbyUnits.Clear();

                foreach (var unit in Controller.Unary.GameState.GetPlayers().SelectMany(p => p.Units).Where(u => u.Targetable))
                {
                    _NearbyUnits.Add(unit);
                }
            }

            Target = ChooseTarget(out var backup);
            Backup = backup;

            if (Backup == null)
            {
                Backup = Target;
            }

            if (Target != null)
            {
                //DoCombat();

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
                foreach (var t in tile.GetNeighbours(true).Append(tile))
                {
                    var sitrep = Controller.Unary.SitRepManager[t];

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
                }

                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
