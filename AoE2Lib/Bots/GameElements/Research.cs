using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Protos.Expert.Action;
using Protos.Expert.Fact;
using System;
using System.Collections.Generic;
using System.Text;

namespace AoE2Lib.Bots.GameElements
{
    public class Research : GameElement
    {
        public readonly int Id;
        public ResearchState State { get; private set; }
        public bool CanResarch { get; private set; }
        public int WoodCost { get; private set; }
        public int FoodCost { get; private set; }
        public int GoldCost { get; private set; }
        public int StoneCost { get; private set; }

        protected internal Research(Bot bot, int id) : base(bot)
        {
            Id = id;
        }

        protected override IEnumerable<IMessage> RequestElementUpdate()
        {
            yield return new ResearchAvailable() { ResearchType = Id };
            yield return new ResearchCompleted() { ResearchType = Id };
            yield return new CanResearch() { ResearchType = Id };
            yield return new UpSetupCostData() { ResetCost = 1, GoalId = 100 };
            yield return new UpAddResearchCost() { TypeOp1 = TypeOp.C, TechId = Id, TypeOp2 = TypeOp.C, Value = 1 };
            yield return new Goal() { GoalId = 100 };
            yield return new Goal() { GoalId = 101 };
            yield return new Goal() { GoalId = 102 };
            yield return new Goal() { GoalId = 103 };
        }

        protected override void UpdateElement(IReadOnlyList<Any> responses)
        {
            var available = responses[0].Unpack<ResearchAvailableResult>().Result;
            var completed = responses[1].Unpack<ResearchCompletedResult>().Result;
            if (completed)
            {
                State = ResearchState.COMPLETE;
            }
            else if (available)
            {
                State = ResearchState.AVAILABLE;
            }
            else
            {
                State = ResearchState.UNAVAILABLE;
            }

            CanResarch = responses[2].Unpack<CanResearchResult>().Result;
            FoodCost = responses[5].Unpack<GoalResult>().Result;
            WoodCost = responses[6].Unpack<GoalResult>().Result;
            StoneCost = responses[7].Unpack<GoalResult>().Result;
            GoldCost = responses[8].Unpack<GoalResult>().Result;
        }
    }
}
