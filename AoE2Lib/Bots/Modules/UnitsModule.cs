using AoE2Lib.Bots.GameElements;
using Protos.Expert.Action;
using Protos.Expert.Fact;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AoE2Lib.Bots.Modules
{
    public class UnitsModule : Module
    {
        public IReadOnlyDictionary<int, UnitType> UnitTypes => _UnitTypes;
        private readonly Dictionary<int, UnitType> _UnitTypes = new Dictionary<int, UnitType>();
        public IReadOnlyDictionary<int, Unit> Units => _Units;
        private readonly Dictionary<int, Unit> _Units = new Dictionary<int, Unit>();
        public bool AutoUpdateUnits { get; set; } = true;

        private readonly List<Command> CreateCommands = new List<Command>();

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
            CreateCommands.Add(command);
        }

        public void Train(int id)
        {
            var command = new Command();
            command.Add(new Train() { InConstUnitId = id });
            CreateCommands.Add(command);
        }

        protected override IEnumerable<Command> RequestUpdate()
        {
            foreach (var command in CreateCommands)
            {
                yield return command;
            }

            CreateCommands.Clear();

            foreach (var type in UnitTypes.Values)
            {
                type.RequestUpdate();
            }

            if (AutoUpdateUnits && Units.Count > 0)
            {
                var units = Units.Values.Where(u => u.Updated == false || u.Targetable == true).ToList();
                units.Sort((a, b) => a.LastUpdateGameTime.CompareTo(b.LastUpdateGameTime));

                for (int i = 0; i < Math.Min(units.Count, 20); i++)
                {
                    units[i].RequestUpdate();
                }
            }
        }

        protected override void Update()
        {

        }
    }
}
