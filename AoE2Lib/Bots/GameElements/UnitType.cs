using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Protos.Expert.Action;
using Protos.Expert.Fact;
using System;
using System.Collections.Generic;
using System.Text;

namespace AoE2Lib.Bots.GameElements
{
    public class UnitType : GameElement
    {
        public readonly int Id;
        public bool IsAvailable { get; private set; }
        public int Count { get; private set; }
        public int CountTotal { get; private set; }
        public bool CanCreate { get; private set; }
        public bool TrainSiteReady { get; private set; }
        public int WoodCost { get; private set; }
        public int FoodCost { get; private set; }
        public int GoldCost { get; private set; }
        public int StoneCost { get; private set; }
        public int Pending => CountTotal - Count;

        internal UnitType(Bot bot, int id) : base(bot)
        {
            Id = id;
        }

        protected override IEnumerable<IMessage> RequestElementUpdate()
        {
            yield return new BuildingAvailable() { InConstBuildingId = Id };
            yield return new BuildingTypeCount() { InConstBuildingId = Id };
            yield return new BuildingTypeCountTotal() { InConstBuildingId = Id };
            yield return new UpCanBuild() { InGoalEscrowState = 0, InConstBuildingId = Id };
            yield return new UpTrainSiteReady() { InConstUnitId = Id };
            yield return new UpSetupCostData() { InConstResetCost = 1, IoGoalId = 100 };
            yield return new UpAddObjectCost() { InConstObjectId = Id, InConstValue = 1 };
            yield return new Goal() { InConstGoalId = 100 };
            yield return new Goal() { InConstGoalId = 101 };
            yield return new Goal() { InConstGoalId = 102 };
            yield return new Goal() { InConstGoalId = 103 };
        }

        protected override void UpdateElement(IReadOnlyList<Any> responses)
        {
            IsAvailable = responses[0].Unpack<BuildingAvailableResult>().Result;
            Count = responses[1].Unpack<BuildingTypeCountResult>().Result;
            CountTotal = responses[2].Unpack<BuildingTypeCountTotalResult>().Result;
            CanCreate = responses[3].Unpack<UpCanBuildResult>().Result;
            TrainSiteReady = responses[4].Unpack<UpTrainSiteReadyResult>().Result;
            FoodCost = responses[7].Unpack<GoalResult>().Result;
            WoodCost = responses[8].Unpack<GoalResult>().Result;
            StoneCost = responses[9].Unpack<GoalResult>().Result;
            GoldCost = responses[10].Unpack<GoalResult>().Result;
        }
    }
}
