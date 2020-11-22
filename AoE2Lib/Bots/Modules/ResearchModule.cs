using AoE2Lib.Bots.GameElements;
using AoE2Lib.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using static AoE2Lib.Bots.Modules.SpendingModule;

namespace AoE2Lib.Bots.Modules
{
    public class ResearchModule : Module
    {
        public IReadOnlyDictionary<int, Research> Researches => _Researches;
        private readonly Dictionary<int, Research> _Researches = new Dictionary<int, Research>();

        public void Add(int id)
        {
            if (!Researches.ContainsKey(id))
            {
                _Researches.Add(id, new Research(Bot, id));
                Bot.Log.Info($"Bot {Bot.Name} {Bot.PlayerNumber}: ResearchModule: Added research {id}");
            }
        }

        public void Research(int id, int priority = 0)
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

            var command = new SpendingCommand()
            {
                Priority = priority,
                WoodCost = research.WoodCost,
                FoodCost = research.FoodCost,
                GoldCost = research.GoldCost,
                StoneCost = research.StoneCost
            };

            if (research.CanResarch)
            {
                command.Add(new Protos.Expert.Action.Research() { ResearchType = id });
            }

            Bot.GetModule<SpendingModule>().Add(command);
        }

        protected override IEnumerable<Command> RequestUpdate()
        {
            foreach (var research in Researches.Values)
            {
                research.RequestUpdate();
                yield return research.Command;
            }
        }

        protected override void Update()
        {
            foreach (var research in Researches.Values)
            {
                research.Update();
            }
        }
    }
}
