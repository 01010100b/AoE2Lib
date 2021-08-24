using AoE2Lib.Bots.GameElements;
using AoE2Lib.Utils;
using Protos.Expert;
using System;
using System.Collections.Generic;
using System.Text;

namespace AoE2Lib.Bots.Modules
{
    public class ResearchModule : Module
    {
        public IReadOnlyDictionary<int, Technology> Researches => _Researches;
        private readonly Dictionary<int, Technology> _Researches = new Dictionary<int, Technology>();
        private readonly List<Command> ResearchCommands = new List<Command>();

        public void Add(int id)
        {
            if (!Researches.ContainsKey(id))
            {
                _Researches.Add(id, new Technology(Bot, id));
                Bot.Log.Info($"ResearchModule: Added research {id}");
            }
        }

        public void Research(int id)
        {
            Add(id);

            var research = Researches[id];
            research.RequestUpdate();
            if (!research.Updated)
            {
                return;
            }
            else if (research.State != ResearchState.AVAILABLE)
            {
                return;
            }
            else if (!research.CanResarch)
            {
                return;
            }

            Bot.Log.Info($"research {id}");

            var command = new Command();
            command.Add(new Protos.Expert.Fact.CanResearch() { InConstTechId = id }, "!=", 0,
                new Protos.Expert.Action.Research() { InConstTechId = id });
            ResearchCommands.Add(command);
        }

        protected override IEnumerable<Command> RequestUpdate()
        {
            foreach (var research in Researches.Values)
            {
                research.RequestUpdate();
            }

            foreach (var command in ResearchCommands)
            {
                yield return command;
            }

            ResearchCommands.Clear();
        }

        protected override void Update()
        {

        }
    }
}
