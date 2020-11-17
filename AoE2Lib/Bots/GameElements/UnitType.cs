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
        public readonly int FoundationId;
        public CmdId CmdId { get; private set; }
        public bool IsBuilding => CmdId == CmdId.CIVILIAN_BUILDING || CmdId == CmdId.MILITARY_BUILDING;
        public bool Available { get; private set; }
        public int Count { get; private set; }
        public int CountTotal { get; private set; }
        public int Pending { get; private set; }
        public bool CanCreate { get; private set; } // can-train for units, can-build for buildings
        public int WoodCost { get; private set; }
        public int FoodCost { get; private set; }
        public int GoldCost { get; private set; }
        public int StoneCost { get; private set; }

        protected internal UnitType(Bot bot, int id, int foundation) : base(bot)
        {
            Id = id;
            FoundationId = foundation;
        }

        protected override IEnumerable<IMessage> RequestElementUpdate()
        {
            yield return new UpGetObjectTypeData() { TypeOp = (int)TypeOp.C, ObjectTypeId = Id, ObjectData = (int)ObjectData.CMDID, GoalData = 50 };
            yield return new Goal() { GoalId = 50 };
            yield return new UnitAvailable() { UnitType = Id };
            yield return new BuildingAvailable() { BuildingType = Id };
            yield return new UpObjectTypeCount() { TypeOp = (int)TypeOp.C, ObjectId = Id };
            yield return new UpObjectTypeCountTotal() { TypeOp = (int)TypeOp.C, ObjectId = Id };
            yield return new UpPendingObjects() { TypeOp = (int)TypeOp.C, ObjectId = Id };
            yield return new CanTrain() { UnitType = FoundationId };
            yield return new CanBuild() { BuildingType = FoundationId };
            yield return new UpSetupCostData() { ResetCost = 1, GoalId = 100 };
            yield return new UpAddObjectCost() { TypeOp1 = (int)TypeOp.C, ObjectId = FoundationId, TypeOp2 = (int)TypeOp.C, Value = 1 };
            yield return new Goal() { GoalId = 100 };
            yield return new Goal() { GoalId = 101 };
            yield return new Goal() { GoalId = 102 };
            yield return new Goal() { GoalId = 103 };
            yield return new UpObjectTypeCount() { TypeOp = (int)TypeOp.C, ObjectId = FoundationId };
            yield return new UpObjectTypeCountTotal() { TypeOp = (int)TypeOp.C, ObjectId = FoundationId };
            yield return new UpPendingObjects() { TypeOp = (int)TypeOp.C, ObjectId = FoundationId };
        }

        protected override void UpdateElement(IReadOnlyList<Any> responses)
        {
            CmdId = (CmdId)responses[1].Unpack<GoalResult>().Result;
            Available = responses[2].Unpack<UnitAvailableResult>().Result;
            if (IsBuilding)
            {
                Available = responses[3].Unpack<BuildingAvailableResult>().Result;
            }
            
            Count = responses[4].Unpack<UpObjectTypeCountResult>().Result;
            CountTotal = responses[5].Unpack<UpObjectTypeCountTotalResult>().Result;
            Pending = responses[6].Unpack<UpPendingObjectsResult>().Result;
            CanCreate = responses[7].Unpack<CanTrainResult>().Result;
            if (IsBuilding)
            {
                CanCreate = responses[8].Unpack<CanBuildResult>().Result;
            }
            
            FoodCost = responses[11].Unpack<GoalResult>().Result;
            WoodCost = responses[12].Unpack<GoalResult>().Result;
            StoneCost = responses[13].Unpack<GoalResult>().Result;
            GoldCost = responses[14].Unpack<GoalResult>().Result;

            if (Id != FoundationId)
            {
                Count += responses[15].Unpack<UpObjectTypeCountResult>().Result;
                CountTotal += responses[16].Unpack<UpObjectTypeCountTotalResult>().Result;
                Pending += responses[17].Unpack<UpPendingObjectsResult>().Result;
            }
        }
    }
}
