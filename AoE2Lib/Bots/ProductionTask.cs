using AoE2Lib.Bots.GameElements;
using Protos.Expert.Action;
using Protos.Expert.Fact;
using System;
using System.Collections.Generic;
using System.Text;

namespace AoE2Lib.Bots
{
    internal abstract class ProductionTask
    {
        public readonly int Id;
        public readonly int Priority;
        public readonly bool Blocking;
        public readonly int WoodCost;
        public readonly int FoodCost;
        public readonly int GoldCost;
        public readonly int StoneCost;
        public readonly int MaxCount;
        public readonly int MaxPending;

        public ProductionTask(int id, int priority, bool blocking, int wood_cost, int food_cost, int gold_cost, int stone_cost, int max_count, int max_pending)
        {
            Id = id;
            Priority = priority;
            Blocking = blocking;
            WoodCost = wood_cost;
            FoodCost = food_cost;
            GoldCost = gold_cost;
            StoneCost = stone_cost;
            MaxCount = max_count;
            MaxPending = max_pending;
        }

        public abstract Command GetCommand();
    }

    internal class BuildNormalTask : ProductionTask
    {
        public BuildNormalTask(int id, int priority, bool blocking, int wood_cost, int food_cost, int gold_cost, int stone_cost, int max_count, int max_pending)
            : base(id, priority, blocking, wood_cost, food_cost, gold_cost, stone_cost, max_count, max_pending)
        {

        }

        public override Command GetCommand()
        {
            const int GL_BUILD = 100;

            var command = new Command();

            command.Add(new SetGoal() { InConstGoalId = GL_BUILD, InConstValue = 0 });

            command.Add(new UpObjectTypeCountTotal() { InConstObjectId = Id }, ">=", MaxCount,
                new SetGoal() { InConstGoalId = GL_BUILD, InConstValue = -1 });
            command.Add(new UpPendingObjects() { InConstObjectId = Id }, ">=", MaxPending,
                new SetGoal() { InConstGoalId = GL_BUILD, InConstValue = -2 });
            command.Add(new CanBuild() { InConstBuildingId = Id }, "!=", 1,
                new SetGoal() { InConstGoalId = GL_BUILD, InConstValue = -3 });
            command.Add(new UpPendingPlacement() { InConstBuildingId = Id }, "!=", 0,
                new SetGoal() { InConstGoalId = GL_BUILD, InConstValue = -4 });
            command.Add(new UpPendingPlacement() { InSnBuildingId = Bot.SN_PENDING_PLACEMENT }, "!=", 0,
                new SetGoal() { InConstGoalId = GL_BUILD, InConstValue = -5 });

            command.Add(new Goal() { InConstGoalId = GL_BUILD }, "==", 0,
                 new Build() { InConstBuildingId = Id },
                 new SetStrategicNumber() { InConstSnId = Bot.SN_PENDING_PLACEMENT, InConstValue = Id });

            return command;
        }
    }

    internal class BuildLineTask : ProductionTask
    {
        private readonly List<Tile> Tiles;

        public BuildLineTask(int id, List<Tile> tiles, int priority, bool blocking, int wood_cost, int food_cost, int gold_cost, int stone_cost, int max_count, int max_pending)
            : base(id, priority, blocking, wood_cost, food_cost, gold_cost, stone_cost, max_count, max_pending)
        {
            Tiles = tiles;
        }

        public override Command GetCommand()
        {
            const int GL_WAS_BUILT = 100;
            const int GL_BUILD = 101;
            const int GL_X = 102;
            const int GL_Y = 103;
            const int GL_TOTAL_LOCAL = 104;

            var command = new Command();
            command.Add(new SetGoal() { InConstGoalId = GL_WAS_BUILT, InConstValue = -1 });

            command.Add(new UpFullResetSearch());
            command.Add(new UpFindLocal() { InConstUnitId = 904, InConstCount = 1 });
            command.Add(new UpGetSearchState() { OutGoalState = GL_TOTAL_LOCAL });
            command.Add(new Goal() { InConstGoalId = GL_TOTAL_LOCAL }, ">", 0,
                new UpSetTargetObject() { InConstIndex = 0, InConstSearchSource = 1 },
                new SetGoal() { InConstGoalId = GL_WAS_BUILT, InConstValue = 0 });

            foreach (var tile in Tiles)
            {
                command.Add(new SetGoal() { InConstGoalId = GL_BUILD, InConstValue = 0 });
                command.Add(new SetGoal() { InConstGoalId = GL_X, InConstValue = tile.X });
                command.Add(new SetGoal() { InConstGoalId = GL_Y, InConstValue = tile.Y });

                command.Add(new UpObjectTypeCountTotal() { InConstObjectId = Id }, ">=", MaxCount,
                    new SetGoal() { InConstGoalId = GL_BUILD, InConstValue = -1 });
                command.Add(new UpPendingObjects() { InConstObjectId = Id }, ">=", MaxPending,
                    new SetGoal() { InConstGoalId = GL_BUILD, InConstValue = -2 });
                command.Add(new UpCanBuildLine() { InConstBuildingId = Id, InGoalPoint = GL_X, InGoalEscrowState = 0 }, "!=", 1,
                    new SetGoal() { InConstGoalId = GL_BUILD, InConstValue = -3 });
                command.Add(new UpPendingPlacement() { InConstBuildingId = Id }, "!=", 0,
                    new SetGoal() { InConstGoalId = GL_BUILD, InConstValue = -4 });
                command.Add(new UpPendingPlacement() { InSnBuildingId = Bot.SN_PENDING_PLACEMENT }, "!=", 0,
                    new SetGoal() { InConstGoalId = GL_BUILD, InConstValue = -5 });
                command.Add(new UpPathDistance() { InConstStrict = 1, InGoalPoint = GL_X }, "==", 65353,
                    new SetGoal() { InConstGoalId = GL_BUILD, InConstValue = -6 });

                var icommand = new Command();
                icommand.Add(new Goal() { InConstGoalId = GL_BUILD }, "==", 0,
                     new UpBuildLine() { InConstBuildingId = Id, InGoalPoint1 = GL_X, InGoalPoint2 = GL_X },
                     new SetStrategicNumber() { InConstSnId = Bot.SN_PENDING_PLACEMENT, InConstValue = Id },
                     new SetGoal() { InConstGoalId = GL_WAS_BUILT, InConstValue = 1 });

                command.Add(new Goal() { InConstGoalId = GL_WAS_BUILT }, "==", 0, icommand.Messages.ToArray());
            }

            return command;
        }
    }

    internal class ResearchTask : ProductionTask
    {
        public ResearchTask(int id, int priority, bool blocking, int wood_cost, int food_cost, int gold_cost, int stone_cost, int max_count, int max_pending)
            : base(id, priority, blocking, wood_cost, food_cost, gold_cost, stone_cost, max_count, max_pending)
        {

        }

        public override Command GetCommand()
        {
            var command = new Command();

            command.Add(new CanResearch() { InConstTechId = Id }, "!=", 0,
                    new Research() { InConstTechId = Id });

            return command;
        }
    }

    internal class TrainTask : ProductionTask
    {
        public TrainTask(int id, int priority, bool blocking, int wood_cost, int food_cost, int gold_cost, int stone_cost, int max_count, int max_pending)
            : base(id, priority, blocking, wood_cost, food_cost, gold_cost, stone_cost, max_count, max_pending)
        {

        }

        public override Command GetCommand()
        {
            const int GL_TRAINED = 100;

            var command = new Command();

            command.Add(new SetGoal() { InConstGoalId = GL_TRAINED, InConstValue = 0 });

            command.Add(new UpObjectTypeCountTotal() { InConstObjectId = Id }, ">=", MaxCount,
                new SetGoal() { InConstGoalId = GL_TRAINED, InConstValue = -1 });
            command.Add(new UpPendingObjects() { InConstObjectId = Id }, ">=", MaxPending,
                new SetGoal() { InConstGoalId = GL_TRAINED, InConstValue = -2 });
            command.Add(new CanTrain() { InConstUnitId = Id }, "!=", 1,
                new SetGoal() { InConstGoalId = GL_TRAINED, InConstValue = -3 });

            command.Add(new Goal() { InConstGoalId = GL_TRAINED }, "==", 0,
                new Train() { InConstUnitId = Id });

            return command;
        }
    }
}
