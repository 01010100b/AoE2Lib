using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Protos.Expert.Action;
using Protos.Expert.Fact;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AoE2Lib.Bots.GameElements
{
    public class UnitType : GameElement
    {
        private static readonly ObjectData[] OBJECT_DATAS = new[] { ObjectData.BASE_TYPE, ObjectData.CMDID };

        public readonly int Id;
        public int this[ObjectData data] => GetData(data);
        public UnitType BaseType => Bot.GameState.GetUnitType(this[ObjectData.BASE_TYPE]);
        public bool IsBuilding => this[ObjectData.CMDID] == (int)CmdId.CIVILIAN_BUILDING || this[ObjectData.CMDID] == (int)CmdId.MILITARY_BUILDING;
        public bool IsAvailable { get; private set; } = false;
        public int Count { get; private set; } = 0;
        public int CountTotal { get; private set; } = 0;
        public bool CanBuild { get; private set; } = false;
        public bool TrainSiteReady { get; private set; } = false;
        public int WoodCost { get; private set; } = 0;
        public int FoodCost { get; private set; } = 0;
        public int GoldCost { get; private set; } = 0;
        public int StoneCost { get; private set; } = 0;
        public int Pending => CountTotal - Count;

        private readonly Dictionary<ObjectData, int> Data = new Dictionary<ObjectData, int>();

        internal UnitType(Bot bot, int id) : base(bot)
        {
            Id = id;
        }

        public int GetData(ObjectData data)
        {
            if (Data.TryGetValue(data, out int val))
            {
                return val;
            }
            else
            {
                return -2;
            }
        }

        public void Train(int max_count = 10000, int max_pending = 10000, int priority = 10, bool blocking = true)
        {
            if (Updated == false || IsAvailable == false || CountTotal >= max_count || Pending > max_pending)
            {
                return;
            }

            var prod = new TrainTask(Id, priority, blocking, WoodCost, FoodCost, GoldCost, StoneCost, max_count, max_pending);
            Bot.GameState.AddProductionTask(prod);
        }

        public void BuildNormal(int max_count = 10000, int max_pending = 10000, int priority = 10, bool blocking = true)
        {
            if (Updated == false || IsAvailable == false || CountTotal >= max_count || Pending > max_pending)
            {
                return;
            }

            var prod = new BuildNormalTask(Id, priority, blocking, WoodCost, FoodCost, GoldCost, StoneCost, max_count, max_pending);

            Bot.GameState.AddProductionTask(prod);
        }

        public void BuildLine(IEnumerable<Tile> tiles, int max_count = 10000, int max_pending = 10000, int priority = 10, bool blocking = true)
        {
            if (Updated == false || IsAvailable == false || CountTotal >= max_count || Pending > max_pending)
            {
                return;
            }

            var good_tiles = new List<Tile>();
            foreach (var tile in tiles.Where(t => t.Explored))
            {
                if (Bot.GameState.Map.TryCanReach(tile.X, tile.Y, out bool can_reach))
                {
                    if (can_reach)
                    {
                        good_tiles.Add(tile);
                    }
                }
                else
                {
                    break;
                }
            }

            if (good_tiles.Count == 0)
            {
                return;
            }

            var prod = new BuildLineTask(Id, good_tiles, priority, blocking, WoodCost, FoodCost, GoldCost, StoneCost, max_count, max_pending);
            Bot.GameState.AddProductionTask(prod);
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

            foreach (var data in OBJECT_DATAS)
            {
                yield return new UpGetObjectTypeData() { InConstTypeId = Id, InConstObjectData = (int)data, OutGoalData = 100 };
                yield return new Goal() { InConstGoalId = 100 };
            }
        }

        protected override void UpdateElement(IReadOnlyList<Any> responses)
        {
            IsAvailable = responses[0].Unpack<BuildingAvailableResult>().Result;
            Count = responses[1].Unpack<BuildingTypeCountResult>().Result;
            CountTotal = responses[2].Unpack<BuildingTypeCountTotalResult>().Result;
            CanBuild = responses[3].Unpack<UpCanBuildResult>().Result;
            TrainSiteReady = responses[4].Unpack<UpTrainSiteReadyResult>().Result;
            FoodCost = responses[7].Unpack<GoalResult>().Result;
            WoodCost = responses[8].Unpack<GoalResult>().Result;
            StoneCost = responses[9].Unpack<GoalResult>().Result;
            GoldCost = responses[10].Unpack<GoalResult>().Result;

            var index = 11;
            foreach (var data in OBJECT_DATAS)
            {
                var val = responses[index + 1].Unpack<GoalResult>().Result;
                Data[data] = val;
            }

            Bot.GameState.GetUnitType(this[ObjectData.BASE_TYPE]);
        }
    }
}
