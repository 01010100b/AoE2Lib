using AoE2Lib.Bots.GameElements;
using AoE2Lib.Utils;
using Protos.Expert.Action;
using Protos.Expert.Fact;
using System;
using System.Collections.Generic;
using System.Text;

namespace AoE2Lib.Bots.Modules
{
    public class MicroModule : Module
    {
        private readonly List<Command> Commands = new List<Command>();

        public void TargetUnit(Unit unit, Unit target, UnitAction? action, UnitFormation? formation, UnitStance? stance, int min_next_attack = int.MinValue, int max_next_attack = int.MaxValue, Unit backup = null)
        {
            if (backup == null)
            {
                backup = target;
            }

            var command = new Command();

            const int GL_CHECKS = 100;
            const int GL_TEMP = 101;
            const int GL_TARGET_ID = 102;

            var op_add = Bot.GameVersion == GameVersion.AOC ? 1 : 25;
            var op_g_min = Bot.GameVersion == GameVersion.AOC ? 14 : 14;

            command.Add(new SetGoal() { InConstGoalId = GL_CHECKS, InConstValue = 0 });
            command.Add(new SetGoal() { InConstGoalId = GL_TEMP, InConstValue = -1 });
            command.Add(new SetGoal() { InConstGoalId = GL_TARGET_ID, InConstValue = -1 });

            // check 1: unit exists

            command.Add(new UpSetTargetById() { InConstId = unit.Id });
            command.Add(new UpGetObjectData() { InConstObjectData = (int)ObjectData.ID, OutGoalData = GL_TEMP });
            command.Add(new Goal() { InConstGoalId = GL_TEMP }, "==", unit.Id, new UpModifyGoal() { IoGoalId = GL_CHECKS, MathOp = op_add, InOpValue = 1 });

            // check 2: next_attack >= min_next_attack

            command.Add(new UpGetObjectData() { InConstObjectData = (int)ObjectData.NEXT_ATTACK, OutGoalData = GL_TEMP });
            command.Add(new Goal() { InConstGoalId = GL_TEMP }, ">=", min_next_attack, new UpModifyGoal() { IoGoalId = GL_CHECKS, MathOp = op_add, InOpValue = 1 });

            // check 3: next_attack <= max_next_attack

            command.Add(new Goal() { InConstGoalId = GL_TEMP }, "<=", max_next_attack, new UpModifyGoal() { IoGoalId = GL_CHECKS, MathOp = op_add, InOpValue = 1 });

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
                new UpTargetObjects() { InConstTarget = 1, 
                    InConstTargetAction = action.HasValue ? (int)action.Value : (int)UnitAction.DEFAULT, 
                    InConstFormation = formation.HasValue ? (int)formation.Value : -1, 
                    InConstAttackStance = stance.HasValue ? (int)stance.Value : -1 }
            );

            Commands.Add(command);
        }

        public void TargetPosition(Unit unit, Position position, UnitAction? action, UnitFormation? formation, UnitStance? stance, int min_next_attack = int.MinValue, int max_next_attack = int.MaxValue)
        {
            if (action == UnitAction.MOVE && unit.Position == position)
            {
                return;
            }

            if (action == UnitAction.MOVE && unit[ObjectData.PRECISE_MOVE_X] == position.PreciseX && unit[ObjectData.PRECISE_MOVE_Y] == position.PreciseY)
            {
                return;
            }

            position = new Position(Math.Max(0, Math.Min(Bot.GameState.Map.Width, position.X)), Math.Max(0, Math.Min(Bot.GameState.Map.Height, position.Y)));

            const int GL_CHECKS = 100;
            const int GL_TEMP = 101;
            const int GL_PRECISE_X = 102;
            const int GL_PRECISE_Y = 103;

            var op_add = Bot.GameVersion == GameVersion.AOC ? 1 : 25;

            var command = new Command();

            command.Add(new SetGoal() { InConstGoalId = GL_CHECKS, InConstValue = 0 });
            command.Add(new SetGoal() { InConstGoalId = GL_TEMP, InConstValue = -1 });

            command.Add(new SetGoal() { InConstGoalId = GL_PRECISE_X, InConstValue = position.PreciseX });
            command.Add(new SetGoal() { InConstGoalId = GL_PRECISE_Y, InConstValue = position.PreciseY });

            // check 1: unit exists

            command.Add(new UpSetTargetById() { InConstId = unit.Id });
            command.Add(new UpGetObjectData() { InConstObjectData = (int)ObjectData.ID, OutGoalData = GL_TEMP });
            command.Add(new Goal() { InConstGoalId = GL_TEMP }, "==", unit.Id, new UpModifyGoal() { IoGoalId = GL_CHECKS, MathOp = op_add, InOpValue = 1 });

            // check 2: next_attack >= min_next_attack

            command.Add(new UpGetObjectData() { InConstObjectData = (int)ObjectData.NEXT_ATTACK, OutGoalData = GL_TEMP });
            command.Add(new Goal() { InConstGoalId = GL_TEMP }, ">=", min_next_attack, new UpModifyGoal() { IoGoalId = GL_CHECKS, MathOp = op_add, InOpValue = 1 });

            // check 3: next_attack <= max_next_attack

            command.Add(new Goal() { InConstGoalId = GL_TEMP }, "<=", max_next_attack, new UpModifyGoal() { IoGoalId = GL_CHECKS, MathOp = op_add, InOpValue = 1 });

            // run if all checks pass

            command.Add(new Goal() { InConstGoalId = GL_CHECKS }, "==", 3,
                new SetStrategicNumber() { InConstSnId = (int)StrategicNumber.TARGET_POINT_ADJUSTMENT, InConstValue = 6 },
                new UpFullResetSearch(),
                new UpAddObjectById() { InConstSearchSource = 1, InConstId = unit.Id },
                new UpTargetPoint() { InGoalPoint = GL_PRECISE_X, 
                    InConstTargetAction = action.HasValue ? (int)action.Value : -1, 
                    InConstFormation = formation.HasValue ? (int)formation.Value : -1, 
                    InConstAttackStance = stance.HasValue ? (int)stance.Value : -1 }
            );

            Commands.Add(command);
        }

        protected override IEnumerable<Command> RequestUpdate()
        {
            foreach (var command in Commands)
            {
                yield return command;
            }

            Commands.Clear();
        }

        protected override void Update()
        {
            
        }
    }
}
