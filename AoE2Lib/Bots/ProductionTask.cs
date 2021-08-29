using Protos.Expert.Action;
using Protos.Expert.Fact;
using System;
using System.Collections.Generic;
using System.Text;

namespace AoE2Lib.Bots
{
    internal class ProductionTask3
    {
        public int Priority { get; set; }
        public bool Blocking { get; set; }
        public int WoodCost { get; set; }
        public int FoodCost { get; set; }
        public int GoldCost { get; set; }
        public int StoneCost { get; set; }
        public int Id { get; set; }
        public int MaxCount { get; set; }
        public int MaxPending { get; set; }
        public bool IsTech { get; set; }
        public bool IsBuilding { get; set; }
        public List<Position> BuildPositions = new List<Position>();
        

        public Command GetCommand(Bot bot)
        {
            if (IsTech)
            {
                bot.Log.Info($"Researching {Id}");

                var command = new Command();
                command.Add(new CanResearch() { InConstTechId = Id }, "!=", 0,
                    new Research() { InConstTechId = Id });

                return command;
            }
            else if (IsBuilding)
            {
                bot.Log.Info($"Building {Id}");

                const int GL_BUILT = 100;
                const int GL_POINT_X = 101;
                const int GL_POINT_Y = 102;

                var op_add = bot.GameVersion == GameVersion.AOC ? 1 : 25;

                var command = new Command();
                command.Add(new SetGoal() { InConstGoalId = GL_BUILT, InConstValue = 0 });
                command.Add(new UpObjectTypeCountTotal() { InConstObjectId = Id }, ">=", MaxCount,
                    new SetGoal() { InConstGoalId = GL_BUILT, InConstValue = 2 });
                command.Add(new UpPendingObjects() { InConstObjectId = Id }, ">=", MaxPending,
                    new SetGoal() { InConstGoalId = GL_BUILT, InConstValue = 2 });
                command.Add(new UpPendingPlacement() { InConstBuildingId = Id }, "!=", 0,
                    new SetGoal() { InConstGoalId = GL_BUILT, InConstValue = 2 });
                command.Add(new UpPendingPlacement() { InSnBuildingId = Bot.SN_PENDING_PLACEMENT}, "!=", 0,
                    new SetGoal() { InConstGoalId = GL_BUILT, InConstValue = 2 });

                if (BuildPositions.Count == 0)
                {
                    command.Add(new CanBuild() { InConstBuildingId = Id }, "!=", 0,
                        new UpModifyGoal() { IoGoalId = GL_BUILT, MathOp = op_add, InOpValue = 1 });
                    command.Add(new Goal() { InConstGoalId = GL_BUILT }, "==", 1,
                        new Build() { InConstBuildingId = Id },
                        new SetStrategicNumber() { InConstSnId = Bot.SN_PENDING_PLACEMENT, InConstValue = Id },
                        new UpModifyGoal() { IoGoalId = GL_BUILT, MathOp = op_add, InOpValue = 1 });
                }
                else
                {
                    foreach (var pos in BuildPositions)
                    {
                        command.Add(new SetGoal() { InConstGoalId = GL_POINT_X, InConstValue = pos.PointX });
                        command.Add(new SetGoal() { InConstGoalId = GL_POINT_Y, InConstValue = pos.PointY });
                        command.Add(new UpCanBuildLine() { InConstBuildingId = Id, InGoalEscrowState = 0, InGoalPoint = GL_POINT_X }, "!=", 0,
                            new UpModifyGoal() { IoGoalId = GL_BUILT, MathOp = op_add, InOpValue = 1 });
                        command.Add(new Goal() { InConstGoalId = GL_BUILT }, "==", 1,
                            new UpBuildLine() { InConstBuildingId = Id, InGoalPoint1 = GL_POINT_X, InGoalPoint2 = GL_POINT_X },
                            new SetStrategicNumber() { InConstSnId = Bot.SN_PENDING_PLACEMENT, InConstValue = Id },
                            new UpModifyGoal() { IoGoalId = GL_BUILT, MathOp = op_add, InOpValue = 1 });

                    }
                }

                return command;
            }
            else
            {
                bot.Log.Info($"Training {Id}");

                const int GL_TRAINING = 100;

                var command = new Command();

                command.Add(new SetGoal() { InConstGoalId = GL_TRAINING, InConstValue = 0 });

                command.Add(new UpObjectTypeCountTotal() { InConstObjectId = Id }, ">=", MaxCount,
                    new SetGoal() { InConstGoalId = GL_TRAINING, InConstValue = 1 });
                command.Add(new UpPendingObjects() { InConstObjectId = Id }, ">=", MaxPending,
                    new SetGoal() { InConstGoalId = GL_TRAINING, InConstValue = 2 });
                command.Add(new CanTrain() { InConstUnitId = Id }, "==", 0,
                    new SetGoal() { InConstGoalId = GL_TRAINING, InConstValue = 3 });
                /*command.Add(new UnitAvailable() { InConstUnitId = Id }, "==", 0,
                    new SetGoal() { InConstGoalId = GL_TRAINING, InConstValue = 4 });
                command.Add(new CanAffordUnit() { InConstUnitId = Id }, "==", 0,
                    new SetGoal() { InConstGoalId = GL_TRAINING, InConstValue = 5 });
                command.Add(new UpTrainSiteReady() { InConstUnitId = Id }, "==", 0,
                    new SetGoal() { InConstGoalId = GL_TRAINING, InConstValue = 6 });*/

                command.Add(new Goal() { InConstGoalId = GL_TRAINING }, "==", 0,
                    new Train() { InConstUnitId = Id });

                return command;
            }
        }

        
    }

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
            const int GL_BUILT = 100;

            var command = new Command();

            command.Add(new SetGoal() { InConstGoalId = GL_BUILT, InConstValue = 0 });

            command.Add(new UpObjectTypeCountTotal() { InConstObjectId = Id }, ">=", MaxCount,
                new SetGoal() { InConstGoalId = GL_BUILT, InConstValue = -1 });
            command.Add(new UpPendingObjects() { InConstObjectId = Id }, ">=", MaxPending,
                new SetGoal() { InConstGoalId = GL_BUILT, InConstValue = -2 });
            command.Add(new CanBuild() { InConstBuildingId = Id }, "!=", 1,
                new SetGoal() { InConstGoalId = GL_BUILT, InConstValue = -3 });
            command.Add(new UpPendingPlacement() { InConstBuildingId = Id }, "!=", 0,
                new SetGoal() { InConstGoalId = GL_BUILT, InConstValue = -4 });
            command.Add(new UpPendingPlacement() { InSnBuildingId = Bot.SN_PENDING_PLACEMENT }, "!=", 0,
                new SetGoal() { InConstGoalId = GL_BUILT, InConstValue = -5 });

            command.Add(new Goal() { InConstGoalId = GL_BUILT }, "==", 0,
                 new Build() { InConstBuildingId = Id },
                 new SetStrategicNumber() { InConstSnId = Bot.SN_PENDING_PLACEMENT, InConstValue = Id },
                 new SetGoal() { InConstGoalId = GL_BUILT, InConstValue = 1 });

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
