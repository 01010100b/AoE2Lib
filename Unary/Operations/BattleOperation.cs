using AoE2Lib;
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

            Manager.Unary.Log.Info($"BattleOperation: Targeting {target.Id}");

            var hp_remaining = new Dictionary<Unit, double>();
            foreach (var enemy in EnemyPriorities.Keys)
            {
                hp_remaining[enemy] = enemy[ObjectData.HITPOINTS];
            }

            foreach (var unit in Units)
            {
                var command = new Command();

                if (unit[ObjectData.RANGE] > 2)
                {
                    Dodge(command, unit, target);
                }

                var attack = unit[ObjectData.BASE_ATTACK];
                var armor = unit[ObjectData.RANGE] > 2 ? target[ObjectData.PIERCE_ARMOR] : target[ObjectData.STRIKE_ARMOR];
                var dmg = 0.8 * Math.Max(1, attack - armor);

                if (hp_remaining[target] > 0)
                {
                    Attack(command, unit, target, backup);
                    hp_remaining[target] -= dmg;
                }
                else
                {
                    Attack(command, unit, backup, target);
                    hp_remaining[backup] -= dmg;
                }

                Manager.Unary.ExecuteCommand(command);
            }

            foreach (var enemy in EnemyPriorities.Keys)
            {
                enemy.RequestUpdate();
            }
        }

        private void Attack(Command command, Unit unit, Unit target, Unit backup)
        {
            const int GL_CHECKS = 100;
            const int GL_TEMP = 101;
            const int GL_TARGET_ID = 102;

            var op_add = Manager.Unary.GameVersion == GameVersion.AOC ? 1 : 25;
            var op_g_min = Manager.Unary.GameVersion == GameVersion.AOC ? 14 : 14;
            var delay = 500;
            if (Manager.Unary.Mod.Objects.TryGetValue(unit.GetData(ObjectData.UPGRADE_TYPE), out ObjectDef def))
            {
                delay = def.AttackDelay;
            }

            command.Add(new SetGoal() { InConstGoalId = GL_CHECKS, InConstValue = 0 });
            command.Add(new SetGoal() { InConstGoalId = GL_TEMP, InConstValue = -1 });
            command.Add(new SetGoal() { InConstGoalId = GL_TARGET_ID, InConstValue = -1 });

            // check 1: unit exists

            command.Add(new UpSetTargetById() { InConstId = unit.Id });
            command.Add(new UpGetObjectData() { InConstObjectData = (int)ObjectData.ID, OutGoalData = GL_TEMP });
            command.Add(new Goal() { InConstGoalId = GL_TEMP }, "==", unit.Id, new UpModifyGoal() { IoGoalId = GL_CHECKS, MathOp = op_add, InOpValue = 1 });

            // check 2: unit is not currently in attack delay

            command.Add(new UpGetObjectData() { InConstObjectData = (int)ObjectData.NEXT_ATTACK, OutGoalData = GL_TEMP });
            command.Add(new Goal() { InConstGoalId = GL_TEMP }, "<", unit[ObjectData.RELOAD_TIME] - delay, new UpModifyGoal() { IoGoalId = GL_CHECKS, MathOp = op_add, InOpValue = 1 });
            
            // check 3: unit is ready to attack

            command.Add(new Goal() { InConstGoalId = GL_TEMP }, "<=", 0, new UpModifyGoal() { IoGoalId = GL_CHECKS, MathOp = op_add, InOpValue = 1 });

            // check 4: target exists as GL_TARGET_ID
            
            command.Add(new UpSetTargetById() { InConstId = backup.Id });
            command.Add(new UpGetObjectData() { InConstObjectData = (int)ObjectData.ID, OutGoalData = GL_TEMP });
            command.Add(new Goal() { InConstGoalId = GL_TEMP }, "==", backup.Id, new SetGoal() { InConstGoalId = GL_TARGET_ID, InConstValue = backup.Id });
            command.Add(new UpSetTargetById() { InConstId = target.Id });
            command.Add(new UpGetObjectData() { InConstObjectData = (int)ObjectData.ID, OutGoalData = GL_TEMP });
            command.Add(new Goal() { InConstGoalId = GL_TEMP }, "==", target.Id, new SetGoal() { InConstGoalId = GL_TARGET_ID, InConstValue = target.Id });
            command.Add(new Goal() { InConstGoalId = GL_TARGET_ID }, "!=", -1, new UpModifyGoal() { IoGoalId = GL_CHECKS, MathOp = op_add, InOpValue = 1 });

            // check 5: unit is not already targeting GL_TARGET_ID

            command.Add(new UpSetTargetById() { InConstId = unit.Id });
            command.Add(new UpGetObjectData() { InConstObjectData = (int)ObjectData.TARGET_ID, OutGoalData = GL_TEMP });
            command.Add(new UpModifyGoal() { IoGoalId = GL_TEMP, MathOp = op_g_min, InOpValue = GL_TARGET_ID });
            command.Add(new Goal() { InConstGoalId = GL_TEMP }, "!=", 0, new UpModifyGoal() { IoGoalId = GL_CHECKS, MathOp = op_add, InOpValue = 1 });

            // run if all checks passed

            command.Add(new Goal() { InConstGoalId = GL_CHECKS }, "==", 5,
                new UpSetTargetById() { InGoalId = GL_TARGET_ID },
                new UpFullResetSearch(),
                new UpAddObjectById() { InConstSearchSource = 1, InConstId = unit.Id },
                new UpTargetObjects() { InConstTarget = 1, InConstTargetAction = (int)UnitAction.DEFAULT, InConstFormation = -1, InConstAttackStance = -1 },
                new ChatToAll() { InTextString = $"Attacking {target.Id} with {unit.Id} on tick {Manager.Unary.Tick}" }
            );
        }

        private void Dodge(Command command, Unit unit, Unit target)
        {
            const int GL_CHECKS = 100;
            const int GL_TEMP = 101;
            const int GL_PRECISE_X = 102;
            const int GL_PRECISE_Y = 103;

            var op_add = Manager.Unary.GameVersion == GameVersion.AOC ? 1 : 25;
            var delay = 500;
            if (Manager.Unary.Mod.Objects.TryGetValue(unit.GetData(ObjectData.UPGRADE_TYPE), out ObjectDef def))
            {
                delay = def.AttackDelay;
            }

            command.Add(new SetGoal() { InConstGoalId = GL_CHECKS, InConstValue = 0 });
            command.Add(new SetGoal() { InConstGoalId = GL_TEMP, InConstValue = -1 });

            var best_pos = target.Position;
            var best_cost = double.MaxValue;
            for (int i = 0; i < 10; i++)
            {
                var dx = (Manager.Unary.Rng.NextDouble() * 4) - 2;
                var dy = (Manager.Unary.Rng.NextDouble() * 4) - 2;
                var pos = unit.Position + new Position(dx, dy);

                if (Manager.Unary.MapModule.IsOnMap(pos) && pos.DistanceTo(target.Position) < unit[ObjectData.RANGE] && pos.DistanceTo(unit.Position) > 1)
                {
                    var cost = 0d;

                    cost += -1 * pos.DistanceTo(target.Position);

                    var angle = (pos - unit.Position).AngleFrom(target.Position - unit.Position);
                    cost += -1 * Math.Abs(Math.Sin(angle));

                    if (cost < best_cost)
                    {
                        best_pos = pos;
                        best_cost = cost;
                    }
                }
            }

            command.Add(new SetGoal() { InConstGoalId = GL_PRECISE_X, InConstValue = best_pos.PreciseX });
            command.Add(new SetGoal() { InConstGoalId = GL_PRECISE_Y, InConstValue = best_pos.PreciseY });

            // check 1: unit exists

            command.Add(new UpSetTargetById() { InConstId = unit.Id });
            command.Add(new UpGetObjectData() { InConstObjectData = (int)ObjectData.ID, OutGoalData = GL_TEMP });
            command.Add(new Goal() { InConstGoalId = GL_TEMP }, "==", unit.Id, new UpModifyGoal() { IoGoalId = GL_CHECKS, MathOp = op_add, InOpValue = 1 });

            // check 2: unit is not currently in attack delay

            command.Add(new UpGetObjectData() { InConstObjectData = (int)ObjectData.NEXT_ATTACK, OutGoalData = GL_TEMP });
            command.Add(new Goal() { InConstGoalId = GL_TEMP }, "<", unit[ObjectData.RELOAD_TIME] - delay, new UpModifyGoal() { IoGoalId = GL_CHECKS, MathOp = op_add, InOpValue = 1 });

            // check 3: unit is ready to dodge

            command.Add(new Goal() { InConstGoalId = GL_TEMP }, ">=", 0, new UpModifyGoal() { IoGoalId = GL_CHECKS, MathOp = op_add, InOpValue = 1 });

            // run if all checks pass

            command.Add(new Goal() { InConstGoalId = GL_CHECKS }, "==", 3,
                new SetStrategicNumber() { InConstSnId = (int)AoE2Lib.StrategicNumber.TARGET_POINT_ADJUSTMENT, InConstValue = 6 },
                new UpFullResetSearch(),
                new UpAddObjectById() { InConstSearchSource = 1, InConstId = unit.Id },
                new UpTargetPoint() { InGoalPoint = GL_PRECISE_X, InConstTargetAction = (int)UnitAction.MOVE, InConstFormation = (int)UnitFormation.LINE, InConstAttackStance = (int)UnitStance.NO_ATTACK });
                
        }
    }
}
