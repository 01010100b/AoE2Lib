using AoE2Lib.Bots;
using AoE2Lib.Bots.GameElements;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unary.Behaviours
{
    internal class CombatRangedBehaviour : CombatBehaviour
    {
        private bool OppositeDirection { get; set; } = false;

        public CombatRangedBehaviour() : base()
        {
            OppositeDirection = GetHashCode() % 2 == 0;
        }

        protected override Unit ChooseTarget(out Unit backup)
        {
            var pos = Controller.Unit.Position;
            var targets = Controller.Unary.GameState.CurrentEnemies
                .SelectMany(p => p.Units)
                .Where(u => u.Targetable)
                .ToList();

            if (targets.Count > 0)
            {
                targets.Sort((a, b) => a.Position.DistanceTo(pos).CompareTo(b.Position.DistanceTo(pos)));

                var target = targets[0];
                backup = targets[0];

                if (targets.Count > 1)
                {
                    backup = targets[1];
                }

                return target;
            }
            else
            {
                backup = null;

                return null;
            }
        }

        protected override void DoCombat()
        {
            var settings = Controller.Unary.Settings;

            // move perpendicular to unit->target vector

            var my_pos = Controller.Unit.Position;
            var delta_pos = 2 * (Target.Position - my_pos).Normalize().Rotate(Math.PI / 2);
            var bias = settings.CombatRangedMovementBias;
            var ballistics = Target[ObjectData.BALLISTICS] > 0;
            
            if (ballistics)
            {
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

            // move away from target or closest threat

            var range = Controller.Unit[ObjectData.RANGE] * settings.CombatRangedMinRangeFraction;

            if (Target.Position.DistanceTo(my_pos) < range)
            {
                delta_pos -= (Target.Position - my_pos).Normalize();
            }
            else
            {
                var closest = Target;
                foreach (var unit in NearbyUnits.Where(u => u.Targetable && u.Player.IsEnemy))
                {
                    if (unit.Position.DistanceTo(my_pos) < closest.Position.DistanceTo(my_pos))
                    {
                        closest = unit;
                    }
                }

                if (closest.Position.DistanceTo(my_pos) < range)
                {
                    delta_pos -= (closest.Position - my_pos).Normalize();
                }
            }

            // check for obstruction

            var move_position = my_pos + delta_pos;

            if (!IsAccessible(move_position))
            {
                Controller.Unary.Log.Debug($"Unit {Controller.Unit.Id} is stuck!");

                var rng = Controller.Unary.Rng;
                var home_pos = Controller.Unary.GameState.MyPosition;

                OppositeDirection = !OppositeDirection;
                delta_pos *= -1;
                move_position = my_pos + delta_pos;

                if (!IsAccessible(move_position))
                {
                    move_position = Target.Position;
                }
            }

            // perform step

            var next_attack = Math.Max(1, Controller.Unit[ObjectData.RELOAD_TIME] - 500);

            if (Controller.Unary.Rng.NextDouble() < settings.CombatRangedShootChance)
            {
                Controller.Unit.Target(Target, UnitAction.DEFAULT, null, null, int.MinValue, 0, Backup);
                Controller.Unit.Target(move_position, UnitAction.MOVE, null, UnitStance.NO_ATTACK, 1, next_attack);
            }
            else
            {
                Controller.Unit.Target(move_position, UnitAction.MOVE, null, UnitStance.NO_ATTACK, int.MinValue, next_attack);
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

        private void AssignTargetScores(List<KeyValuePair<Unit, double>> targets)
        {
            var speed = Controller.Unit[ObjectData.SPEED] / 100d;
            var range = Controller.Unit[ObjectData.RANGE];
            var pierce = true;// Controller.Unary.Mod.DoesPierceDamage(Controller.Unit[ObjectData.UPGRADE_TYPE]);
            var attack = Controller.Unit[ObjectData.BASE_ATTACK];

            for (int i = 0; i < targets.Count; i++)
            {
                var target = targets[i].Key;
                var ttk = 1d;

                // walk time to target in range

                var distance = target.Position.DistanceTo(Controller.Unit.Position);

                if (distance > range)
                {
                    var d = distance - range;
                    var walk = d / Math.Max(0.01, speed);

                    ttk += walk;
                }

                // time to shoot target to 0 hp

                var hp = target[ObjectData.HITPOINTS];
                var armor = pierce ? target[ObjectData.PIERCE_ARMOR] : target[ObjectData.STRIKE_ARMOR];
                var bonus = 0;// Controller.Unary.Mod.GetBonusDamage(Controller.Unit[ObjectData.UPGRADE_TYPE], target[ObjectData.UPGRADE_TYPE]);
                var dmg = Math.Min(1, Math.Max(0, attack - armor) + bonus);
                var hits = (hp / dmg) + 1;
                var reload = Controller.Unit[ObjectData.RELOAD_TIME] / 1000d;
                var kill = hits * reload;

                ttk += kill;

                var score = 1 / ttk;

                targets[i] = new KeyValuePair<Unit, double>(target, score);
            }
        }
    }
}
