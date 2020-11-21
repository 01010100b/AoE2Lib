using AoE2Lib.Mods;
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
        public readonly UnitDef UnitDef;
        public bool IsAvailable { get; private set; }
        public int Count { get; private set; }
        public int CountTotal { get; private set; }
        public bool CanCreate { get; private set; }
        public int WoodCost { get; private set; }
        public int FoodCost { get; private set; }
        public int GoldCost { get; private set; }
        public int StoneCost { get; private set; }
        public bool IsBuilding => UnitDef.IsBuilding;
        public int Pending => CountTotal - Count;

        protected internal UnitType(Bot bot, UnitDef unit) : base(bot)
        {
            Id = unit.Id;
            UnitDef = unit;
        }

        protected override IEnumerable<IMessage> RequestElementUpdate()
        {
            if (IsBuilding)
            {
                return RequestBuilding();
            }
            else
            {
                return RequestUnit();
            }
        }

        private IEnumerable<IMessage> RequestUnit()
        {
            yield return new UnitAvailable() { UnitType = Id };
            yield return new UnitTypeCount() { UnitType = Id };
            yield return new UnitTypeCount() { UnitType = UnitDef.FoundationId };
            yield return new UnitTypeCountTotal() { UnitType = Id };
            yield return new UnitTypeCountTotal() { UnitType = UnitDef.FoundationId };
            yield return new CanTrain() { UnitType = Id };
            yield return new UpSetupCostData() { ResetCost = 1, GoalId = 100 };
            yield return new UpAddObjectCost() { TypeOp1 = TypeOp.C, ObjectId = UnitDef.FoundationId, TypeOp2 = TypeOp.C, Value = 1 };
            yield return new Goal() { GoalId = 100 };
            yield return new Goal() { GoalId = 101 };
            yield return new Goal() { GoalId = 102 };
            yield return new Goal() { GoalId = 103 };
        }

        private IEnumerable<IMessage> RequestBuilding()
        {
            yield return new BuildingAvailable() { BuildingType = Id };
            yield return new BuildingTypeCount() { BuildingType = Id };
            yield return new BuildingTypeCount() { BuildingType = UnitDef.FoundationId };
            yield return new BuildingTypeCountTotal() { BuildingType = Id };
            yield return new BuildingTypeCountTotal() { BuildingType = UnitDef.FoundationId };
            yield return new CanBuild() { BuildingType = Id };
            yield return new UpSetupCostData() { ResetCost = 1, GoalId = 100 };
            yield return new UpAddObjectCost() { TypeOp1 = TypeOp.C, ObjectId = UnitDef.FoundationId, TypeOp2 = TypeOp.C, Value = 1 };
            yield return new Goal() { GoalId = 100 };
            yield return new Goal() { GoalId = 101 };
            yield return new Goal() { GoalId = 102 };
            yield return new Goal() { GoalId = 103 };
        }

        protected override void UpdateElement(IReadOnlyList<Any> responses)
        {
            if (IsBuilding)
            {
                UpdateBuilding(responses);
            }
            else
            {
                UpdateUnit(responses);
            }
        }

        private void UpdateUnit(IReadOnlyList<Any> responses)
        {
            IsAvailable = responses[0].Unpack<UnitAvailableResult>().Result;
            Count = responses[1].Unpack<UnitTypeCountResult>().Result;
            if (Id != UnitDef.FoundationId)
            {
                Count += responses[2].Unpack<UnitTypeCountResult>().Result;
            }
            CountTotal = responses[3].Unpack<UnitTypeCountTotalResult>().Result;
            if (Id != UnitDef.FoundationId)
            {
                CountTotal += responses[4].Unpack<UnitTypeCountTotalResult>().Result;
            }
            CanCreate = responses[5].Unpack<CanTrainResult>().Result;
            FoodCost = responses[8].Unpack<GoalResult>().Result;
            WoodCost = responses[9].Unpack<GoalResult>().Result;
            StoneCost = responses[10].Unpack<GoalResult>().Result;
            GoldCost = responses[11].Unpack<GoalResult>().Result;
        }

        private void UpdateBuilding(IReadOnlyList<Any> responses)
        {
            IsAvailable = responses[0].Unpack<BuildingAvailableResult>().Result;
            Count = responses[1].Unpack<BuildingTypeCountResult>().Result;
            if (Id != UnitDef.FoundationId)
            {
                Count += responses[2].Unpack<BuildingTypeCountResult>().Result;
            }
            CountTotal = responses[3].Unpack<BuildingTypeCountTotalResult>().Result;
            if (Id != UnitDef.FoundationId)
            {
                CountTotal += responses[4].Unpack<BuildingTypeCountTotalResult>().Result;
            }
            CanCreate = responses[5].Unpack<CanBuildResult>().Result;
            FoodCost = responses[8].Unpack<GoalResult>().Result;
            WoodCost = responses[9].Unpack<GoalResult>().Result;
            StoneCost = responses[10].Unpack<GoalResult>().Result;
            GoldCost = responses[11].Unpack<GoalResult>().Result;
        }
    }
}
