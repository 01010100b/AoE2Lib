using Protos.Expert.Action;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unary.Utils;

namespace Unary.Modules
{
    public class MicroModule : Module
    {
        private class MicroCommand : Command
        {
            public int UnitId { get; set; }
            public int TargetId { get; set; } = -1;
            public Position TargetPoint { get; set; } = new Position(-1, -1);
            public UnitAction Action { get; set; }
            public UnitStance Stance { get; set; }
        }

        private readonly List<MicroCommand> Commands = new List<MicroCommand>();

        public void TargetObject(int unit, int target, UnitAction action, UnitStance stance)
        {
            if (Commands.Select(c => c.UnitId).Contains(unit))
            {
                return;
            }

            var command = new MicroCommand()
            {
                UnitId = unit,
                TargetId = target,
                Action = action,
                Stance = stance
            };

            Commands.Add(command);
        }

        public void TargetPoint(int unit, Position point, UnitAction action, UnitStance stance)
        {
            if (Commands.Select(c => c.UnitId).Contains(unit))
            {
                return;
            }

            var command = new MicroCommand()
            {
                UnitId = unit,
                TargetPoint = point,
                Action = action,
                Stance = stance
            };

            Commands.Add(command);
        }

        internal override IEnumerable<Command> RequestUpdate(Bot bot)
        {
            foreach (var command in Commands)
            {
                command.Messages.Clear();
                command.Responses.Clear();

                command.Messages.Add(new UpFullResetSearch());
                command.Messages.Add(new UpAddObjectById() { SearchSource = 1, TypeOp = (int)TypeOp.C, OpId = command.UnitId });

                if (command.TargetId > 0)
                {
                    command.Messages.Add(new UpSetTargetById() { TypeOp = (int)TypeOp.C, Id = command.TargetId });
                    command.Messages.Add(new UpTargetObjects() { Target = 1, Action = (int)command.Action, AttackStance = (int)command.Stance, Formation = -1 });
                }
                else
                {
                    command.Messages.Add(new SetGoal() { GoalId = 100, GoalValue = command.TargetPoint.X });
                    command.Messages.Add(new SetGoal() { GoalId = 101, GoalValue = command.TargetPoint.Y });
                    command.Messages.Add(new UpSetTargetPoint() { GoalPoint = 100 });
                    command.Messages.Add(new UpTargetPoint() { GoalPoint = 0, Action = (int)command.Action, AttackStance = (int)command.Stance, Formation = -1 });
                }

                yield return command;
            }
        }

        internal override void Update(Bot bot)
        {
            Commands.Clear();
        }
    }
}
