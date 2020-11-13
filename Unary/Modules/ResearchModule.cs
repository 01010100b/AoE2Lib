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
    public class ResearchModule : Module
    {
        private class ResearchCommand : Command
        {
            public TechDef TechDef { get; set; }
            public bool Checked { get; set; }
            public bool CanAfford { get; set; }
            public bool IsAvailable { get; set; }
        }

        private readonly List<ResearchCommand> Commands = new List<ResearchCommand>();

        public void Research(TechDef tech)
        {
            if (Commands.Select(c => c.TechDef.Id).Contains(tech.Id))
            {
                return;
            }

            var command = new ResearchCommand()
            {
                TechDef = tech,
                Checked = false,
                CanAfford = false,
                IsAvailable = false
            };

            Commands.Add(command);
        }

        internal override IEnumerable<Command> RequestUpdate(Bot bot)
        {
            var afford = true;
            foreach (var command in Commands)
            {
                command.Messages.Clear();
                command.Responses.Clear();

                if (command.Checked == false)
                {
                    command.Messages.Add(new CanAffordResearch() { ResearchType = command.TechDef.Id });
                    command.Messages.Add(new ResearchAvailable() { ResearchType = command.TechDef.Id });
                }
                else if (afford && command.IsAvailable)
                {
                    if (command.CanAfford)
                    {
                        command.Messages.Add(new Research() { ResearchType = command.TechDef.Id });
                    }
                    else
                    {
                        afford = false;
                    }
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

                if (command.Checked == false)
                {
                    command.CanAfford = command.Responses[0].Unpack<CanAffordResearchResult>().Result;
                    command.IsAvailable = command.Responses[1].Unpack<ResearchAvailableResult>().Result;
                    command.Checked = true;
                }
                else
                {
                    Commands.Remove(command);
                }
            }
        }
    }
}
