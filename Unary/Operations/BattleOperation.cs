﻿using AoE2Lib;
using AoE2Lib.Bots;
using AoE2Lib.Bots.GameElements;
using AoE2Lib.Utils;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Protos.Expert;
using Protos.Expert.Action;
using Protos.Expert.Fact;
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
        public readonly Dictionary<Unit, double> EnemyPriorities = new Dictionary<Unit, double>();

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
                .OrderBy(e => e[ObjectData.HITPOINTS])
                .ThenBy(e => e.Id)
                .ToList();
            
            
            var target = enemies[0];
            var backup = enemies.Count > 1 ? enemies[1] : target;

            Manager.Unary.Log.Info($"BattleOperation: {Units.Count()} units");
            Manager.Unary.Log.Info($"BattleOperation: Targeting {target.Id}");

            var hp_remaining = new Dictionary<Unit, double>();
            foreach (var enemy in EnemyPriorities.Keys)
            {
                hp_remaining[enemy] = enemy[ObjectData.HITPOINTS];
            }

            foreach (var unit in Units)
            {
                var attack = unit[ObjectData.BASE_ATTACK];
                var armor = unit[ObjectData.RANGE] > 2 ? target[ObjectData.PIERCE_ARMOR] : target[ObjectData.STRIKE_ARMOR];
                var dmg = 0.8 * Math.Max(1, attack - armor);
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
            var angle = 0d;
            var tick = unit.Id + Manager.Unary.Tick;

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
                angle = 0;
            }

            return angle;
        }
    }
}
