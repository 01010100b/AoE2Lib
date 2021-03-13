using AoE2Lib.Bots.GameElements;
using Protos.Expert.Action;
using Protos.Expert.Fact;
using System;
using System.Collections.Generic;
using System.Text;

namespace AoE2Lib.Bots.Modules
{
    public class UnitsModule : Module
    {
        public IReadOnlyDictionary<int, UnitType> UnitTypes => _UnitTypes;
        private readonly Dictionary<int, UnitType> _UnitTypes = new Dictionary<int, UnitType>();

        public IReadOnlyDictionary<int, Unit> Units => _Units;
        private readonly Dictionary<int, Unit> _Units = new Dictionary<int, Unit>();

        private readonly List<Command> Commands = new List<Command>();

        public void AddUnitType(int id)
        {
            if (!UnitTypes.ContainsKey(id))
            {
                _UnitTypes.Add(id, new UnitType(Bot, id));
                Bot.Log.Info($"InfoModule: Added unit {id}");
            }
        }

        public void Build(int id)
        {
            var command = new Command();
            command.Add(new Build() { InConstBuildingId = id });
            Commands.Add(command);
        }

        public void Train(int id)
        {
            var command = new Command();
            command.Add(new Train() { InConstUnitId = id });
            Commands.Add(command);
        }

        protected override IEnumerable<Command> RequestUpdate()
        {
            foreach (var command in Commands)
            {
                yield return command;
            }

            Commands.Clear();

            foreach (var type in UnitTypes.Values)
            {
                type.RequestUpdate();
            }
        }

        protected override void Update()
        {

        }
    }
}
