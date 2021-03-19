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
            command.Add(new UpAddObjectById() { InConstSearchSource = 1, InConstId = unit.Id });
            command.Add(new UpSetTargetById() { InConstId = unit.Id });
            command.Add(new UpSetTargetById() { InConstId = target.Id });
            command.Add(new UpTargetObjects() { InConstTarget = 1, InConstTargetAction = (int)action, InConstFormation = (int)formation, InConstAttackStance = (int)stance });

            Commands.Add(command);
        }

        public void TargetPosition(Unit unit, Position target, UnitAction action, UnitFormation formation, UnitStance stance)
        {
            if (unit.Position.DistanceTo(target) < 0.1)
            {
                return;
            }

            var command = new Command();

            command.Add(new UpFullResetSearch());
            command.Add(new UpAddObjectById() { InConstSearchSource = 1, InConstId = unit.Id });
            command.Add(new SetGoal() { InConstGoalId = 100, InConstValue = target.PreciseX });
            command.Add(new SetGoal() { InConstGoalId = 101, InConstValue = target.PreciseY });
            command.Add(new SetStrategicNumber() { InConstSnId = (int)StrategicNumber.TARGET_POINT_ADJUSTMENT, InConstValue = 6 });
            command.Add(new UpTargetPoint() { InGoalPoint = 100, InConstTargetAction = (int)action, InConstFormation = (int)formation, InConstAttackStance = (int)stance });

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
