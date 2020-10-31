using Protos.Expert.Action;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unary.Utils;

namespace Unary.Modules
{
    public class UnitFindModule : Module
    {
        public enum UnitFindType
        {
            MILLITARY, CIVILIAN, BUILDING, RESOURCE
        }

        private readonly List<Command> UnitFindCommands = new List<Command>();

        public void FindUnits(Position position, int player, UnitFindType type)
        {
            if (type == UnitFindType.RESOURCE)
            {
                player = 0;
            }

            var command = new Command();

            command.Messages.Add(new SetGoal() { GoalId = 50, GoalValue = position.X });
            command.Messages.Add(new SetGoal() { GoalId = 51, GoalValue = position.Y });
            command.Messages.Add(new UpSetTargetPoint() { GoalPoint = 50 });
            command.Messages.Add(new SetStrategicNumber() { StrategicNumber = (int)StrategicNumber.FOCUS_PLAYER_NUMBER, Value = player });
            command.Messages.Add(new UpFullResetSearch());

            switch (type)
            {
                case UnitFindType.MILLITARY:

                    command.Messages.Add(new UpFilterDistance() { TypeOp1 = 6, MinDistance = -1, TypeOp2 = 6, MaxDistance = 10 });
                    command.Messages.Add(new UpFilterInclude() { CmdId = (int)CmdId.MILITARY, ActionId = -1, OrderId = -1, OnMainland = -1 });
                    command.Messages.Add(new UpFindRemote() { TypeOp1 = 6, UnitId = -1, TypeOp2 = 6, Count = 35 });
                    command.Messages.Add(new UpFilterInclude() { CmdId = (int)CmdId.MONK, ActionId = -1, OrderId = -1, OnMainland = -1 });
                    command.Messages.Add(new UpFindRemote() { TypeOp1 = 6, UnitId = -1, TypeOp2 = 6, Count = 5 });

                    command.Messages.Add(new UpFilterDistance() { TypeOp1 = 6, MinDistance = 9, TypeOp2 = 6, MaxDistance = -1 });
                    command.Messages.Add(new UpFilterInclude() { CmdId = (int)CmdId.MONK, ActionId = -1, OrderId = -1, OnMainland = -1 });
                    command.Messages.Add(new UpFindRemote() { TypeOp1 = 6, UnitId = -1, TypeOp2 = 6, Count = 5 });
                    command.Messages.Add(new UpFilterInclude() { CmdId = (int)CmdId.MILITARY, ActionId = -1, OrderId = -1, OnMainland = -1 });
                    command.Messages.Add(new UpFindRemote() { TypeOp1 = 6, UnitId = -1, TypeOp2 = 6, Count = 40 });

                    break;

                default: throw new NotImplementedException();
            }
        }

        internal override IEnumerable<Command> RequestUpdate(Bot bot)
        {
            return UnitFindCommands;
        }

        internal override void Update(Bot bot)
        {
            throw new NotImplementedException();
        }
    }
}
