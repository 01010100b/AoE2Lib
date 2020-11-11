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
            public int Pending { get; set; } = -1;
        }

        private readonly Dictionary<int, TrainCommand> Commands = new Dictionary<int, TrainCommand>();

        public void Train(UnitDef unit, int max = int.MaxValue, int concurrent = int.MaxValue)
        {
            if (!Commands.ContainsKey(unit.Id))
            {
                Commands.Add(unit.Id, new TrainCommand() { Unit = unit});
            }

            var command = Commands[unit.Id];
            command.MaxCount = max;
            command.Concurrent = concurrent;
        }

        internal override IEnumerable<Command> RequestUpdate(Bot bot)
        {
            foreach (var command in Commands.Values)
            {
                command.Messages.Clear();
                command.Responses.Clear();

                if (command.CountTotal == -1)
                {
                    command.Messages.Add(new UnitTypeCountTotal() { UnitType = command.Unit.Id });
                    command.Messages.Add(new UpPendingObjects() { TypeOp = (int)TypeOp.C, ObjectId = command.Unit.Id });
                }
                else if (command.CountTotal < command.MaxCount && command.Pending < command.Concurrent)
                {
                    command.Messages.Add(new Train() { UnitType = command.Unit.FoundationId });
                }

                if (command.Messages.Count > 0)
                {
                    yield return command;
                }
            }
        }

        internal override void Update(Bot bot)
        {
            foreach (var command in Commands.Values.ToList())
            {
                if (command.CountTotal == -1)
                {
                    Debug.Assert(command.Responses.Count == 2);

                    command.CountTotal = command.Responses[0].Unpack<UnitTypeCountTotalResult>().Result;
                    command.Pending = command.Responses[1].Unpack<UpPendingObjectsResult>().Result;
                }
                else
                {
                    Commands.Remove(command.UnitId);
                }
            }
        }
    }
}
