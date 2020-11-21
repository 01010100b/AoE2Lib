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
        public bool IsBuilding => UnitDef.IsBuilding;
        public bool IsAvailable { get; private set; }
        public int Count { get; private set; }
        public int CountTotal { get; private set; }
        public int Pending { get; private set; }
        public bool CanCreate { get; private set; } // can-train for units, can-build for buildings
        public int WoodCost { get; private set; }
        public int FoodCost { get; private set; }
        public int GoldCost { get; private set; }
        public int StoneCost { get; private set; }

        protected internal UnitType(Bot bot, UnitDef unit) : base(bot)
        {
            Id = unit.Id;
            UnitDef = unit;
        }

        protected override IEnumerable<IMessage> RequestElementUpdate()
        {
            yield return new UnitAvailable() { UnitType = Id };
            yield return new BuildingAvailable() { BuildingType = Id };
            yield return new UpObjectTypeCount() { TypeOp = (int)TypeOp.C, ObjectId = Id };
            yield return new UpObjectTypeCountTotal() { TypeOp = (int)TypeOp.C, ObjectId = Id };
            yield return new UpPendingObjects() { TypeOp = (int)TypeOp.C, ObjectId = Id };
            yield return new CanTrain() { UnitType = UnitDef.FoundationId };
            yield return new CanBuild() { BuildingType = UnitDef.FoundationId };
            yield return new UpSetupCostData() { ResetCost = 1, GoalId = 100 };
            yield return new UpAddObjectCost() { TypeOp1 = (int)TypeOp.C, ObjectId = UnitDef.FoundationId, TypeOp2 = (int)TypeOp.C, Value = 1 };
            yield return new Goal() { GoalId = 100 };
            yield return new Goal() { GoalId = 101 };
            yield return new Goal() { GoalId = 102 };
            yield return new Goal() { GoalId = 103 };
            yield return new UpObjectTypeCount() { TypeOp = (int)TypeOp.C, ObjectId = UnitDef.FoundationId };
            yield return new UpObjectTypeCountTotal() { TypeOp = (int)TypeOp.C, ObjectId = UnitDef.FoundationId };
            yield return new UpPendingObjects() { TypeOp = (int)TypeOp.C, ObjectId = UnitDef.FoundationId };
        }

        protected override void UpdateElement(IReadOnlyList<Any> responses)
        {
            IsAvailable = responses[0].Unpack<UnitAvailableResult>().Result;
            if (IsBuilding)
            {
                IsAvailable = responses[1].Unpack<BuildingAvailableResult>().Result;
            }
            
            Count = responses[2].Unpack<UpObjectTypeCountResult>().Result;
            CountTotal = responses[3].Unpack<UpObjectTypeCountTotalResult>().Result;
            Pending = responses[4].Unpack<UpPendingObjectsResult>().Result;
            CanCreate = responses[5].Unpack<CanTrainResult>().Result;
            if (IsBuilding)
            {
                CanCreate = responses[6].Unpack<CanBuildResult>().Result;
            }
            
            FoodCost = responses[9].Unpack<GoalResult>().Result;
            WoodCost = responses[10].Unpack<GoalResult>().Result;
            StoneCost = responses[11].Unpack<GoalResult>().Result;
            GoldCost = responses[12].Unpack<GoalResult>().Result;

            if (Id != UnitDef.FoundationId)
            {
                Count += responses[13].Unpack<UpObjectTypeCountResult>().Result;
                CountTotal += responses[14].Unpack<UpObjectTypeCountTotalResult>().Result;
                Pending += responses[15].Unpack<UpPendingObjectsResult>().Result;
            }
        }
    }
}
