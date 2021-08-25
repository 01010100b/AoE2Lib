using AoE2Lib;
using AoE2Lib.Bots.GameElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unary.Managers;
using static Unary.Utils.Mod;

namespace Unary.Operations
{
    class BattleOperation : Operation
    {
        public readonly Dictionary<Unit, int> EnemyPriorities = new Dictionary<Unit, int>();

        public BattleOperation(OperationsManager manager) : base(manager)
        {

        }

        public override void Update()
        {
            if (Units.Count() == 0)
            {
                return;
            }

            if (EnemyPriorities.Count == 0)
            {
                return;
            }

            var enemies = EnemyPriorities.Keys
                .OrderByDescending(e => EnemyPriorities[e])
                .ThenBy(e => e[ObjectData.HITPOINTS])
                .ThenBy(e => e.Id)
                .ToList();
            
            
            var target = enemies[0];
            var backup = enemies.Count > 1 ? enemies[1] : target;

            var hp_remaining = new Dictionary<Unit, double>();
            foreach (var enemy in EnemyPriorities.Keys)
            {
                hp_remaining[enemy] = enemy[ObjectData.HITPOINTS];
            }

            foreach (var unit in Units)
            {
                var attack = unit[ObjectData.BASE_ATTACK];
                var armor = unit[ObjectData.RANGE] > 2 ? target[ObjectData.PIERCE_ARMOR] : target[ObjectData.STRIKE_ARMOR];
                var dmg = Math.Max(1, attack - armor);
                var delay = Manager.Unary.Mod.GetAttackDelay(unit[ObjectData.UPGRADE_TYPE]);

                if (target[ObjectData.RANGE] > 2 && unit[ObjectData.RANGE] > 2)
                {
                    var angle = GetDodgeAngle(unit, target);
                    var pos = unit.Position + (target.Position - unit.Position).Rotate(angle);

                    unit.TargetPosition(pos, UnitAction.MOVE, null, null, 0, unit[ObjectData.RELOAD_TIME] - (int)delay.TotalMilliseconds);
                }

                if (hp_remaining[target] > 0)
                {
                    unit.TargetUnit(target, null, null, null, 0, 0, backup);
                    hp_remaining[target] -= dmg;
                }
                else
                {
                    unit.TargetUnit(backup, null, null, null, 0, 0, target);
                    hp_remaining[backup] -= dmg;
                }
            }

            foreach (var enemy in EnemyPriorities.Keys)
            {
                enemy.RequestUpdate();
            }
        }

        private double GetDodgeAngle(Unit unit, Unit target)
        {
            double angle;
            var tick = unit.Id + Manager.Unary.GameState.Tick;

            if (target[ObjectData.BALLISTICS] == 0)
            {
                if (tick / 5 % 2 == 0)
                {
                    angle = Math.PI / 2;
                }
                else
                {
                    angle = -Math.PI / 2;
                }
            }
            else
            {
                if (tick % 2 == 0)
                {
                    angle = Math.PI / 2;
                }
                else
                {
                    angle = -Math.PI / 2;
                }
            }

            if (target[ObjectData.RANGE] < unit[ObjectData.RANGE] && target.Position.DistanceTo(unit.Position) <= target[ObjectData.RANGE])
            {
                angle = Math.PI;
            }

            if (unit.Position.DistanceTo(target.Position) < unit[ObjectData.RANGE] - 2)
            {
                angle = Math.PI;
            }

            if (unit.Position.DistanceTo(target.Position) > unit[ObjectData.RANGE])
            {
                angle = (Manager.Unary.Rng.NextDouble() * Math.PI / 2) - (Math.PI / 4);
            }

            return angle;
        }
    }
}
