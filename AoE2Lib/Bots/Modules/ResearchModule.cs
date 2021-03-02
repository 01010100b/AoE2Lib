using AoE2Lib.Bots.GameElements;
using AoE2Lib.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace AoE2Lib.Bots.Modules
{
    public class ResearchModule : Module
    {
        public IReadOnlyDictionary<int, Research> Researches => _Researches;
        private readonly Dictionary<int, Research> _Researches = new Dictionary<int, Research>();
        private readonly List<Command> ResearchCommands = new List<Command>();

        public void Add(int id)
        {
            if (!Researches.ContainsKey(id))
            {
                _Researches.Add(id, new Research(Bot, id));
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

            var command = new Command();
            command.Add(new Protos.Expert.Action.Research() { ResearchType = id });
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
