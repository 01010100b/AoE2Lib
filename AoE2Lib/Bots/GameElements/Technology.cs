using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Protos.Expert.Action;
using Protos.Expert.Fact;
using System;
using System.Collections.Generic;
using System.Text;

namespace AoE2Lib.Bots.GameElements
{
    public class Technology : GameElement
    {
        public readonly int Id;
        public ResearchState State { get; private set; }
        public bool Available => State == ResearchState.AVAILABLE;
        public bool Started => State == ResearchState.PENDING || State == ResearchState.COMPLETE;
        public bool Completed => State == ResearchState.COMPLETE;
        public bool CanResarch { get; private set; }
        public int WoodCost { get; private set; }
        public int FoodCost { get; private set; }
        public int GoldCost { get; private set; }
        public int StoneCost { get; private set; }

        internal Technology(Bot bot, int id) : base(bot)
        {
            Id = id;
        }

        public void Research()
        {
            var command = new Command();

            command.Add(new CanResearch() { InConstTechId = Id }, "!=", 0,
                    new Research() { InConstTechId = Id });

            Bot.GameState.AddCommand(command);
        }

        protected override IEnumerable<IMessage> RequestElementUpdate()
        {
            const int GL_FOOD = Bot.GOAL_START;
            const int GL_WOOD = Bot.GOAL_START + 1;
            const int GL_STONE = Bot.GOAL_START + 2;
            const int GL_GOLD = Bot.GOAL_START + 3;

            yield return new UpResearchStatus() { InConstTechId = Id };
            yield return new CanResearch() { InConstTechId = Id };
            yield return new UpSetupCostData() { InConstResetCost = 1, IoGoalId = GL_FOOD };
            yield return new UpAddResearchCost() { InConstTechId = Id, InConstValue = 1 };
            yield return new Goal() { InConstGoalId = GL_FOOD };
            yield return new Goal() { InConstGoalId = GL_WOOD };
            yield return new Goal() { InConstGoalId = GL_STONE };
            yield return new Goal() { InConstGoalId = GL_GOLD };
        }

        protected override void UpdateElement(IReadOnlyList<Any> responses)
        {
            State = (ResearchState)responses[0].Unpack<UpResearchStatusResult>().Result;
            CanResarch = responses[1].Unpack<CanResearchResult>().Result;
            FoodCost = responses[4].Unpack<GoalResult>().Result;
            WoodCost = responses[5].Unpack<GoalResult>().Result;
            StoneCost = responses[6].Unpack<GoalResult>().Result;
            GoldCost = responses[7].Unpack<GoalResult>().Result;
        }
    }
}
