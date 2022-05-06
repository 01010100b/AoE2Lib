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
    public class Unit : GameElement
    {
        public readonly int Id;
        public int this[ObjectData data] => GetData(data);
        public Player Player => GetPlayer();
        public bool Targetable { get; private set; } = false;
        public bool IsBuilding => this[ObjectData.CMDID] == (int)CmdId.CIVILIAN_BUILDING || this[ObjectData.CMDID] == (int)CmdId.MILITARY_BUILDING;
        public TimeSpan LastSeenGameTime { get; private set; } = TimeSpan.MinValue;
        public Position Position => Position.FromPrecise(this[ObjectData.PRECISE_X], this[ObjectData.PRECISE_Y]);
        public Tile Tile => GetTile();

        private readonly Dictionary<ObjectData, int> Data = new Dictionary<ObjectData, int>();

        internal Unit(Bot bot, int id) : base(bot)
        {
            Id = id;
        }

        public void Train(UnitType type, int max_count, int max_pending)
        {
            throw new NotSupportedException("game crash in aimodule dll");

            // up-target-point training variant crashes the game, action_id string conflict in aoe2-ai-module

            RequestUpdate();

            if (Updated == false)
            {
                return;
            }

            if (this[ObjectData.PLAYER] != Bot.PlayerNumber)
            {
                return;
            }


            if (type.Updated == false || type.Available == false || type.CountTotal >= max_count || type.Pending > max_pending)
            {
                return;
            }

            if (this[ObjectData.PROGRESS_TYPE] != 0 && this[ObjectData.PROGRESS_TYPE] != 102)
            {
                return;
            }

            if (type.TrainSite[ObjectData.BASE_TYPE] != this[ObjectData.BASE_TYPE])
            {
                return;
            }

            var command = new Command();

            const int GL_CHECKS = Bot.GOAL_START;
            const int GL_TEMP = Bot.GOAL_START + 1;

            command.Add(new SetGoal() { InConstGoalId = GL_CHECKS, InConstValue = 0 });
            command.Add(new SetGoal() { InConstGoalId = GL_TEMP, InConstValue = -1 });

            command.Add(new UpSetTargetById() { InConstId = Id });
            command.Add(new UpGetObjectData() { InConstObjectData = (int)ObjectData.ID, OutGoalData = GL_TEMP });
            command.Add(new Goal() { InConstGoalId = GL_TEMP }, "!=", Id,
                new SetGoal() { InConstGoalId = GL_CHECKS, InConstValue = -1 });

            command.Add(new UpObjectTypeCountTotal() { InConstObjectId = Id }, ">=", max_count,
                new SetGoal() { InConstGoalId = GL_CHECKS, InConstValue = -2 });
            command.Add(new UpPendingObjects() { InConstObjectId = Id }, ">=", max_pending,
                new SetGoal() { InConstGoalId = GL_CHECKS, InConstValue = -3 });
            command.Add(new CanAffordUnit() { InConstUnitId = Id }, "!=", 1,
                new SetGoal() { InConstGoalId = GL_CHECKS, InConstValue = -4 });

            command.Add(new Goal() { InConstGoalId = GL_CHECKS }, "==", 0,
                new UpFullResetSearch(),
                new UpAddObjectById() { InConstSearchSource = 1, InConstId = Id },
                new UpTargetPoint()
                {
                    InGoalPoint = 0,
                    InConstTargetAction = (int)UnitAction.TRAIN,
                    InConstFormation = type.Id,
                    InConstAttackStance = -1
                });

            Bot.GameState.AddCommand(command);
        }

        public void Target(Unit target, UnitAction action = UnitAction.DEFAULT, UnitFormation? formation = null, UnitStance? stance = null, int min_next_attack = int.MinValue, int max_next_attack = int.MaxValue, Unit backup = null)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            if (backup == null)
            {
                backup = target;
            }

            RequestUpdate();
            target.RequestUpdate();
            backup.RequestUpdate();

            if (Updated == false || target.Updated == false || backup.Updated == false)
            {
                return;
            }

            var command = new Command();

            const int GL_CHECKS = Bot.GOAL_START;
            const int GL_TEMP = Bot.GOAL_START + 1;
            const int GL_TARGET_ID = Bot.GOAL_START + 2;

            var op_g_min = 14;

            command.Add(new SetGoal() { InConstGoalId = GL_CHECKS, InConstValue = 0 });
            command.Add(new SetGoal() { InConstGoalId = GL_TEMP, InConstValue = -1 });
            command.Add(new SetGoal() { InConstGoalId = GL_TARGET_ID, InConstValue = -1 });

            // check 1: unit exists

            command.Add(new UpSetTargetById() { InConstId = Id });
            command.Add(new UpGetObjectData() { InConstObjectData = (int)ObjectData.ID, OutGoalData = GL_TEMP });
            command.Add(new Goal() { InConstGoalId = GL_TEMP }, "!=", Id, 
                new SetGoal() { InConstGoalId = GL_CHECKS, InConstValue = -1});

            // check 2: next_attack >= min_next_attack

            command.Add(new UpGetObjectData() { InConstObjectData = (int)ObjectData.NEXT_ATTACK, OutGoalData = GL_TEMP });
            command.Add(new Goal() { InConstGoalId = GL_TEMP }, "<", min_next_attack,
                new SetGoal() { InConstGoalId = GL_CHECKS, InConstValue = -2 });

            // check 3: next_attack <= max_next_attack

            command.Add(new Goal() { InConstGoalId = GL_TEMP }, ">", max_next_attack,
                new SetGoal() { InConstGoalId = GL_CHECKS, InConstValue = -3 });

            // check 4: target exists as GL_TARGET_ID

            command.Add(new UpSetTargetById() { InConstId = backup.Id });
            command.Add(new UpGetObjectData() { InConstObjectData = (int)ObjectData.ID, OutGoalData = GL_TEMP });
            command.Add(new Goal() { InConstGoalId = GL_TEMP }, "==", backup.Id, 
                new SetGoal() { InConstGoalId = GL_TARGET_ID, InConstValue = backup.Id });
            command.Add(new UpSetTargetById() { InConstId = target.Id });
            command.Add(new UpGetObjectData() { InConstObjectData = (int)ObjectData.ID, OutGoalData = GL_TEMP });
            command.Add(new Goal() { InConstGoalId = GL_TEMP }, "==", target.Id, 
                new SetGoal() { InConstGoalId = GL_TARGET_ID, InConstValue = target.Id });
            command.Add(new Goal() { InConstGoalId = GL_TARGET_ID }, "==", -1,
                new SetGoal() { InConstGoalId = GL_CHECKS, InConstValue = -4 });

            // check 5: unit is not already targeting GL_TARGET_ID

            command.Add(new UpSetTargetById() { InConstId = Id });
            command.Add(new UpGetObjectData() { InConstObjectData = (int)ObjectData.TARGET_ID, OutGoalData = GL_TEMP });
            command.Add(new UpModifyGoal() { IoGoalId = GL_TEMP, MathOp = op_g_min, InOpValue = GL_TARGET_ID });
            command.Add(new Goal() { InConstGoalId = GL_TEMP }, "==", 0,
                new SetGoal() { InConstGoalId = GL_CHECKS, InConstValue = -5 });

            // run if all checks passed

            command.Add(new Goal() { InConstGoalId = GL_CHECKS }, "==", 0,
                new UpSetTargetById() { InGoalId = GL_TARGET_ID },
                new UpFullResetSearch(),
                new UpAddObjectById() { InConstSearchSource = 1, InConstId = Id },
                new UpTargetObjects()
                {
                    InConstTarget = 1,
                    InConstTargetAction = (int)action,
                    InConstFormation = formation.HasValue ? (int)formation.Value : -1,
                    InConstAttackStance = stance.HasValue ? (int)stance.Value : -1
                }
            );

            Bot.GameState.AddCommand(command);
        }

        public void Target(Position position, UnitAction action = UnitAction.MOVE, UnitFormation? formation = null, UnitStance? stance = null, int min_next_attack = int.MinValue, int max_next_attack = int.MaxValue)
        {
            RequestUpdate();

            if (!Bot.GameState.Map.IsOnMap(position))
            {
                return;
            }

            if (Updated == false)
            {
                return;
            }

            if (action == UnitAction.TRAIN)
            {
                return;
            }

            if (action == UnitAction.MOVE && Position == position)
            {
                return;
            }

            if (action == UnitAction.MOVE && this[ObjectData.PRECISE_MOVE_X] == position.PreciseX && this[ObjectData.PRECISE_MOVE_Y] == position.PreciseY)
            {
                return;
            }

            const int GL_CHECKS = Bot.GOAL_START;
            const int GL_TEMP = Bot.GOAL_START + 1;
            const int GL_PRECISE_X = Bot.GOAL_START + 2;
            const int GL_PRECISE_Y = Bot.GOAL_START + 3;

            var command = new Command();

            command.Add(new SetGoal() { InConstGoalId = GL_CHECKS, InConstValue = 0 });
            command.Add(new SetGoal() { InConstGoalId = GL_TEMP, InConstValue = -1 });

            command.Add(new SetGoal() { InConstGoalId = GL_PRECISE_X, InConstValue = position.PreciseX });
            command.Add(new SetGoal() { InConstGoalId = GL_PRECISE_Y, InConstValue = position.PreciseY });

            // check 1: unit exists

            command.Add(new UpSetTargetById() { InConstId = Id });
            command.Add(new UpGetObjectData() { InConstObjectData = (int)ObjectData.ID, OutGoalData = GL_TEMP });
            command.Add(new Goal() { InConstGoalId = GL_TEMP }, "!=", Id,
                new SetGoal() { InConstGoalId = GL_CHECKS, InConstValue = -1 });

            // check 2: next_attack >= min_next_attack

            command.Add(new UpGetObjectData() { InConstObjectData = (int)ObjectData.NEXT_ATTACK, OutGoalData = GL_TEMP });
            command.Add(new Goal() { InConstGoalId = GL_TEMP }, "<", min_next_attack,
                new SetGoal() { InConstGoalId = GL_CHECKS, InConstValue = -2 });

            // check 3: next_attack <= max_next_attack

            command.Add(new Goal() { InConstGoalId = GL_TEMP }, ">", max_next_attack,
                new SetGoal() { InConstGoalId = GL_CHECKS, InConstValue = -3 });

            // run if all checks pass

            command.Add(new Goal() { InConstGoalId = GL_CHECKS }, "==", 0,
                new SetStrategicNumber() { InConstSnId = (int)StrategicNumber.TARGET_POINT_ADJUSTMENT, InConstValue = 6 },
                new UpFullResetSearch(),
                new UpAddObjectById() { InConstSearchSource = 1, InConstId = Id },
                new UpTargetPoint()
                {
                    InGoalPoint = GL_PRECISE_X,
                    InConstTargetAction = (int)action,
                    InConstFormation = formation.HasValue ? (int)formation.Value : -1,
                    InConstAttackStance = stance.HasValue ? (int)stance.Value : -1
                }
            );

            Bot.GameState.AddCommand(command);
        }

        protected override IEnumerable<IMessage> RequestElementUpdate()
        {
            const int GL_X = Bot.GOAL_START;
            const int GL_Y = Bot.GOAL_START + 1;

            yield return new UpSetTargetById() { InConstId = Id };
            yield return new UpGetObjectData() { InConstObjectData = (int)ObjectData.POINT_X, OutGoalData = GL_X };
            yield return new UpGetObjectData() { InConstObjectData = (int)ObjectData.POINT_Y, OutGoalData = GL_Y };
            yield return new UpPointExplored() { InGoalPoint = GL_X };
            yield return new UpObjectDataList();
        }

        protected override void UpdateElement(IReadOnlyList<Any> responses)
        {
            var visible = responses[3].Unpack<UpPointExploredResult>().Result == 15;
            var data = responses[4].Unpack<UpObjectDataListResult>().Result.ToArray();
            var id = data[(int)ObjectData.ID];

            Targetable = true;

            if (id != Id)
            {
                Targetable = false;

                return;
            }

            if (LastSeenGameTime >= TimeSpan.Zero && !visible)
            {
                return;
            }

            LastSeenGameTime = Bot.GameState.GameTime;

            for (int i = 0; i < data.Length; i++)
            {
                Data[(ObjectData)i] = data[i];
            }
        }

        private int GetData(ObjectData data)
        {
            if (Data.TryGetValue(data, out int value))
            {
                return value;
            }
            else
            {
                return -2;
            }
        }

        private Player GetPlayer()
        {
            if (Bot.GameState.TryGetPlayer(this[ObjectData.PLAYER], out var player))
            {
                return player;
            }
            else
            {
                throw new Exception($"Player {this[ObjectData.PLAYER]} not found or valid.");
            }
        }

        private Tile GetTile()
        {
            if (Bot.GameState.Map.TryGetTile(Position, out var tile))
            {
                return tile;
            }
            else
            {
                return default;
            }
        }


    }
}
