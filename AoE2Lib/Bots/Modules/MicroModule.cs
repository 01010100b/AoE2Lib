using AoE2Lib.Bots.GameElements;
using AoE2Lib.Utils;
using Protos.Expert.Action;
using System;
using System.Collections.Generic;
using System.Text;

namespace AoE2Lib.Bots.Modules
{
    public class MicroModule : Module
    {
        private readonly List<Command> Commands = new List<Command>();

        public void TargetUnit(Unit unit, Unit target, UnitAction action, UnitFormation formation, UnitStance stance)
        {
            var command = new Command();

            command.Add(new UpFullResetSearch());
            command.Add(new UpAddObjectById() { SearchSource = 1, TypeOp = TypeOp.C, OpId = unit.Id });
            command.Add(new UpSetTargetById() { TypeOp = TypeOp.C, Id = unit.Id });
            command.Add(new UpSetTargetById() { TypeOp = TypeOp.C, Id = target.Id });
            command.Add(new UpTargetObjects() { Target = 1, Action = (int)action, Formation = (int)formation, AttackStance = (int)stance });

            Commands.Add(command);
        }

        public void TargetPosition(Unit unit, Position target, UnitAction action, UnitFormation formation, UnitStance stance)
        {
            var command = new Command();

            command.Add(new UpFullResetSearch());
            command.Add(new UpAddObjectById() { SearchSource = 1, TypeOp = TypeOp.C, OpId = unit.Id });
            command.Add(new SetGoal() { GoalId = 100, GoalValue = target.PreciseX });
            command.Add(new SetGoal() { GoalId = 101, GoalValue = target.PreciseY });
            command.Add(new SetStrategicNumber() { StrategicNumber = 292, Value = 6 });
            command.Add(new UpTargetPoint() { GoalPoint = 100, Action = (int)action, Formation = (int)formation, AttackStance = (int)stance });

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
