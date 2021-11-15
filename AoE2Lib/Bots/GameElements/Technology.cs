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
        public bool Finished => State == ResearchState.COMPLETE;
        public bool CanResarch { get; private set; }
        public int WoodCost { get; private set; }
        public int FoodCost { get; private set; }
        public int GoldCost { get; private set; }
        public int StoneCost { get; private set; }

        internal Technology(Bot bot, int id) : base(bot)
        {
            Id = id;
        }

        public void Research(int priority = 10, bool blocking = true)
        {
            if (Updated == false || State != ResearchState.AVAILABLE)
            {
                return;
            }
            var prod = new ResearchTask(Id, priority, blocking, WoodCost, FoodCost, GoldCost, StoneCost, -1, -1);

            Bot.GameState.AddProductionTask(prod);
        }

        protected override IEnumerable<IMessage> RequestElementUpdate()
        {
            yield return new UpResearchStatus() { InConstTechId = Id };
            yield return new CanResearch() { InConstTechId = Id };
            yield return new UpSetupCostData() { InConstResetCost = 1, IoGoalId = 100 };
            yield return new UpAddResearchCost() { InConstTechId = Id, InConstValue = 1 };
            yield return new Goal() { InConstGoalId = 100 };
            yield return new Goal() { InConstGoalId = 101 };
            yield return new Goal() { InConstGoalId = 102 };
            yield return new Goal() { InConstGoalId = 103 };
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
