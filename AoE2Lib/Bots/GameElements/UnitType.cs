using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Protos.Expert.Action;
using Protos.Expert.Fact;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace AoE2Lib.Bots.GameElements
{
    public class UnitType : GameElement
    {
        private static readonly ObjectData[] OBJECT_DATAS = new[] { ObjectData.BASE_TYPE, ObjectData.CMDID, ObjectData.TRAIN_SITE };

        public readonly int Id;
        public int this[ObjectData data] => GetData(data);
        public bool IsBuilding => this[ObjectData.CMDID] == (int)CmdId.CIVILIAN_BUILDING || this[ObjectData.CMDID] == (int)CmdId.MILITARY_BUILDING;
        public UnitType TrainSite => GetTrainSite();
        public bool Available { get; private set; } = false;
        public int Count { get; private set; } = 0;
        public int CountTotal { get; private set; } = 0;
        public int Pending => CountTotal - Count;
        public int WoodCost { get; private set; } = 0;
        public int FoodCost { get; private set; } = 0;
        public int GoldCost { get; private set; } = 0;
        public int StoneCost { get; private set; } = 0;

        private readonly Dictionary<ObjectData, int> Data = new Dictionary<ObjectData, int>();

        internal UnitType(Bot bot, int id) : base(bot)
        {
            Id = id;
        }

        public void Train(int max_count = int.MaxValue, int max_pending = int.MaxValue, int priority = 10, bool blocking = true)
        {
            if (Updated == false || Available == false || CountTotal >= max_count || Pending > max_pending)
            {
                return;
            }

            max_count = Math.Min(30000, max_count);
            max_pending = Math.Min(30000, max_pending);

            const int GL_ERROR = Bot.GOAL_START;

            var command = new Command();

            command.Add(new SetGoal() { InConstGoalId = GL_ERROR, InConstValue = 0 });

            command.Add(new UpObjectTypeCountTotal() { InConstObjectId = Id }, ">=", max_count,
                new SetGoal() { InConstGoalId = GL_ERROR, InConstValue = -1 });
            command.Add(new UpPendingObjects() { InConstObjectId = Id }, ">=", max_pending,
                new SetGoal() { InConstGoalId = GL_ERROR, InConstValue = -2 });
            command.Add(new CanTrain() { InConstUnitId = Id }, "!=", 1,
                new SetGoal() { InConstGoalId = GL_ERROR, InConstValue = -3 });

            command.Add(new Goal() { InConstGoalId = GL_ERROR }, "==", 0,
                new Train() { InConstUnitId = Id });

            Bot.GameState.AddCommand(command);
        }

        public void Build(IEnumerable<Tile> tiles, int max_count = int.MaxValue, int max_pending = int.MaxValue)
        {
            if (Updated == false || Available == false || CountTotal >= max_count || Pending > max_pending)
            {
                return;
            }

            max_count = Math.Min(30000, max_count);
            max_pending = Math.Min(30000, max_pending);

            const int GL_ERROR = Bot.GOAL_START;
            const int GL_X = Bot.GOAL_START + 1;
            const int GL_Y = Bot.GOAL_START + 2;
            const int GL_WAS_BUILT = Bot.GOAL_START + 3;

            var command = new Command();
            command.Add(new SetGoal() { InConstGoalId = GL_WAS_BUILT, InConstValue = 0 });

            foreach (var tile in tiles.Where(t => t.Explored))
            {
                command.Add(new SetGoal() { InConstGoalId = GL_ERROR, InConstValue = 0 });
                command.Add(new SetGoal() { InConstGoalId = GL_X, InConstValue = tile.X });
                command.Add(new SetGoal() { InConstGoalId = GL_Y, InConstValue = tile.Y });

                command.Add(new UpObjectTypeCountTotal() { InConstObjectId = Id }, ">=", max_count,
                    new SetGoal() { InConstGoalId = GL_ERROR, InConstValue = -1 });
                command.Add(new UpPendingObjects() { InConstObjectId = Id }, ">=", max_pending,
                    new SetGoal() { InConstGoalId = GL_ERROR, InConstValue = -2 });
                command.Add(new UpCanBuildLine() { InConstBuildingId = Id, InGoalPoint = GL_X, InGoalEscrowState = 0 }, "!=", 1,
                    new SetGoal() { InConstGoalId = GL_ERROR, InConstValue = -3 });
                command.Add(new UpPendingPlacement() { InConstBuildingId = Id }, "!=", 0,
                    new SetGoal() { InConstGoalId = GL_ERROR, InConstValue = -4 });

                var buildcommand = new Command();
                buildcommand.Add(new Goal() { InConstGoalId = GL_ERROR }, "==", 0,
                     new UpBuildLine() { InConstBuildingId = Id, InGoalPoint1 = GL_X, InGoalPoint2 = GL_X },
                     new SetGoal() { InConstGoalId = GL_WAS_BUILT, InConstValue = 1 });

                command.Add(new Goal() { InConstGoalId = GL_WAS_BUILT }, "==", 0, buildcommand);
            }

            Bot.GameState.AddCommand(command);
        }

        public void Build(int max_count, int max_pending)
        {
            if (Updated == false || Available == false || CountTotal >= max_count || Pending > max_pending)
            {
                return;
            }

            max_count = Math.Min(30000, max_count);
            max_pending = Math.Min(30000, max_pending);

            const int GL_ERROR = Bot.GOAL_START;

            var command = new Command();

            command.Add(new SetGoal() { InConstGoalId = GL_ERROR, InConstValue = 0 });

            command.Add(new UpObjectTypeCountTotal() { InConstObjectId = Id }, ">=", max_count,
                new SetGoal() { InConstGoalId = GL_ERROR, InConstValue = -1 });

            command.Add(new UpPendingObjects() { InConstObjectId = Id }, ">=", max_pending,
                new SetGoal() { InConstGoalId = GL_ERROR, InConstValue = -2 });

            command.Add(new CanBuild() { InConstBuildingId = Id }, "!=", 1,
                new SetGoal() { InConstGoalId = GL_ERROR, InConstValue = -3 });

            command.Add(new UpPendingPlacement() { InConstBuildingId = Id }, "!=", 0,
                new SetGoal() { InConstGoalId = GL_ERROR, InConstValue = -4 });

            command.Add(new Goal() { InConstGoalId = GL_ERROR }, "==", 0,
                new Build() { InConstBuildingId = Id });

            Bot.GameState.AddCommand(command);
        }

        protected override IEnumerable<IMessage> RequestElementUpdate()
        {
            const int GL_FOOD = Bot.GOAL_START;
            const int GL_WOOD = Bot.GOAL_START + 1;
            const int GL_STONE = Bot.GOAL_START + 2;
            const int GL_GOLD = Bot.GOAL_START + 3;

            foreach (var data in OBJECT_DATAS)
            {
                yield return new UpGetObjectTypeData() { InConstTypeId = Id, InConstObjectData = (int)data, OutGoalData = GL_FOOD };
                yield return new Goal() { InConstGoalId = GL_FOOD };
            }

            yield return new BuildingAvailable() { InConstBuildingId = Id };
            yield return new BuildingTypeCount() { InConstBuildingId = Id };
            yield return new BuildingTypeCountTotal() { InConstBuildingId = Id };
            yield return new UpSetupCostData() { InConstResetCost = 1, IoGoalId = GL_FOOD };
            yield return new UpAddObjectCost() { InConstObjectId = Id, InConstValue = 1 };
            yield return new Goal() { InConstGoalId = GL_FOOD };
            yield return new Goal() { InConstGoalId = GL_WOOD };
            yield return new Goal() { InConstGoalId = GL_STONE };
            yield return new Goal() { InConstGoalId = GL_GOLD };
        }

        protected override void UpdateElement(IReadOnlyList<Any> responses)
        {
            var index = 0;
            foreach (var data in OBJECT_DATAS)
            {
                var val = responses[index + 1].Unpack<GoalResult>().Result;
                Data[data] = val;
                index += 2;
            }

            Available = responses[index + 0].Unpack<BuildingAvailableResult>().Result;
            Count = responses[index + 1].Unpack<BuildingTypeCountResult>().Result;
            CountTotal = responses[index + 2].Unpack<BuildingTypeCountTotalResult>().Result;
            FoodCost = responses[index + 5].Unpack<GoalResult>().Result;
            WoodCost = responses[index + 6].Unpack<GoalResult>().Result;
            StoneCost = responses[index + 7].Unpack<GoalResult>().Result;
            GoldCost = responses[index + 8].Unpack<GoalResult>().Result;
        }

        private int GetData(ObjectData data)
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

        private UnitType GetTrainSite()
        {
            if (Bot.GameState.TryGetUnitType(this[ObjectData.TRAIN_SITE], out var site))
            {
                return site;
            }
            else
            {
                return default;
            }
        }
    }
}
