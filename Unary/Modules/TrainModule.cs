using Protos.Expert.Action;
using Protos.Expert.Fact;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unary.Mods;

namespace Unary.Modules
{
    public class TrainModule : Module
    {
        private class TrainCommand : Command
        {
            public UnitDef Unit { get; set; }
            public int MaxCount { get; set; }
            public int Concurrent { get; set; }
            public int CountTotal { get; set; } = -1;
            public int Pending { get; set; }
            public bool CanAfford { get; set; }
        }

        private readonly List<TrainCommand> Commands = new List<TrainCommand>();

        public void Train(UnitDef unit, int max = int.MaxValue, int concurrent = int.MaxValue)
        {
            if (Commands.Select(c => c.Unit.Id).Contains(unit.Id))
            {
                return;
            }

            var command = new TrainCommand();
            command.Unit = unit;
            command.MaxCount = max;
            command.Concurrent = concurrent;

            Commands.Add(command);
        }

        internal override IEnumerable<Command> RequestUpdate(Bot bot)
        {
            foreach (var command in Commands)
            {
                command.Messages.Clear();
                command.Responses.Clear();

                if (command.CountTotal == -1)
                {
                    command.Messages.Add(new UnitTypeCountTotal() { UnitType = command.Unit.Id });
                    command.Messages.Add(new UpPendingObjects() { TypeOp = (int)TypeOp.C, ObjectId = command.Unit.Id });
                    command.Messages.Add(new CanAffordUnit() { UnitType = command.Unit.Id });
                }
                else if (command.CountTotal < command.MaxCount && command.Pending < command.Concurrent)
                {
                    command.Messages.Add(new Train() { UnitType = command.Unit.FoundationId });
                }
                else if (!command.CanAfford)
                {
                    break;
                }

                if (command.Messages.Count > 0)
                {
                    yield return command;
                }
            }
        }

        internal override void Update(Bot bot)
        {
            foreach (var command in Commands.ToList())
            {
                Debug.Assert(command.Messages.Count == command.Responses.Count);

                if (command.CountTotal == -1)
                {
                    command.CountTotal = command.Responses[0].Unpack<UnitTypeCountTotalResult>().Result;
                    command.Pending = command.Responses[1].Unpack<UpPendingObjectsResult>().Result;
                    command.CanAfford = command.Responses[2].Unpack<CanAffordUnitResult>().Result;
                }
                else
                {
                    Commands.Remove(command);
                }
            }
        }
    }
}
