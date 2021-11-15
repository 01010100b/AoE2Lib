using AoE2Lib.Bots.GameElements;
using Protos.Expert.Action;
using Protos.Expert.Fact;
using System;
using System.Collections.Generic;
using System.Linq;
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
            const int GL_CONTROL = 101;
            const int GL_X = 102;
            const int GL_Y = 103;

            var command = new Command();
            command.Add(new SetGoal() { InConstGoalId = GL_WAS_BUILT, InConstValue = 0 });

            foreach (var tile in Tiles.Where(t => t.Explored))
            {
                command.Add(new SetGoal() { InConstGoalId = GL_CONTROL, InConstValue = 0 });
                command.Add(new SetGoal() { InConstGoalId = GL_X, InConstValue = tile.X });
                command.Add(new SetGoal() { InConstGoalId = GL_Y, InConstValue = tile.Y });

                command.Add(new UpObjectTypeCountTotal() { InConstObjectId = Id }, ">=", MaxCount,
                    new SetGoal() { InConstGoalId = GL_CONTROL, InConstValue = -1 });
                command.Add(new UpPendingObjects() { InConstObjectId = Id }, ">=", MaxPending,
                    new SetGoal() { InConstGoalId = GL_CONTROL, InConstValue = -2 });
                command.Add(new UpCanBuildLine() { InConstBuildingId = Id, InGoalPoint = GL_X, InGoalEscrowState = 0 }, "!=", 1,
                    new SetGoal() { InConstGoalId = GL_CONTROL, InConstValue = -3 });
                command.Add(new UpPendingPlacement() { InConstBuildingId = Id }, "!=", 0,
                    new SetGoal() { InConstGoalId = GL_CONTROL, InConstValue = -4 });

                var buildcommand = new Command();
                buildcommand.Add(new Goal() { InConstGoalId = GL_CONTROL }, "==", 0,
                     new UpBuildLine() { InConstBuildingId = Id, InGoalPoint1 = GL_X, InGoalPoint2 = GL_X },
                     new SetGoal() { InConstGoalId = GL_WAS_BUILT, InConstValue = 1 });

                command.Add(new Goal() { InConstGoalId = GL_WAS_BUILT }, "==", 0, buildcommand);
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
