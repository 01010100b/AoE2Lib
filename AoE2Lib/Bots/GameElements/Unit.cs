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
    public class Unit : GameElement
    {
        public readonly int Id;
        public int this[ObjectData data] { get { return GetData(data); } }
        public int PlayerNumber => GetData(ObjectData.PLAYER);
        public bool Targetable { get; private set; } = false;
        public bool Visible { get; private set; } = false;
        public TimeSpan LastSeenGameTime { get; private set; } = TimeSpan.Zero;
        public Position Position => Position.FromPrecise(GetData(ObjectData.PRECISE_X), GetData(ObjectData.PRECISE_Y));
        public Position Velocity { get; private set; } = Position.Zero;

        private readonly Dictionary<ObjectData, int> Data = new Dictionary<ObjectData, int>();

        internal Unit(Bot bot, int id) : base(bot)
        {
            Id = id;
        }

        public int GetData(ObjectData data)
        {
            if (Data.TryGetValue(data, out int value))
            {
                return value;
            }
            else
            {
                return -1;
            }
        }

        public void Target(Unit target, UnitAction? action = null, UnitFormation? formation = null, UnitStance? stance = null, int min_next_attack = int.MinValue, int max_next_attack = int.MaxValue, Unit backup = null)
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

            const int GL_CHECKS = 100;
            const int GL_TEMP = 101;
            const int GL_TARGET_ID = 102;

            var op_add = Bot.GameVersion == GameVersion.AOC ? 1 : 25;
            var op_g_min = Bot.GameVersion == GameVersion.AOC ? 14 : 14;

            command.Add(new SetGoal() { InConstGoalId = GL_CHECKS, InConstValue = 0 });
            command.Add(new SetGoal() { InConstGoalId = GL_TEMP, InConstValue = -1 });
            command.Add(new SetGoal() { InConstGoalId = GL_TARGET_ID, InConstValue = -1 });

            // check 1: unit exists

            command.Add(new UpSetTargetById() { InConstId = Id });
            command.Add(new UpGetObjectData() { InConstObjectData = (int)ObjectData.ID, OutGoalData = GL_TEMP });
            command.Add(new Goal() { InConstGoalId = GL_TEMP }, "==", Id, new UpModifyGoal() { IoGoalId = GL_CHECKS, MathOp = op_add, InOpValue = 1 });

            // check 2: next_attack >= min_next_attack

            command.Add(new UpGetObjectData() { InConstObjectData = (int)ObjectData.NEXT_ATTACK, OutGoalData = GL_TEMP });
            command.Add(new Goal() { InConstGoalId = GL_TEMP }, ">=", min_next_attack, new UpModifyGoal() { IoGoalId = GL_CHECKS, MathOp = op_add, InOpValue = 1 });

            // check 3: next_attack <= max_next_attack

            command.Add(new Goal() { InConstGoalId = GL_TEMP }, "<=", max_next_attack, new UpModifyGoal() { IoGoalId = GL_CHECKS, MathOp = op_add, InOpValue = 1 });

            // check 4: target exists as GL_TARGET_ID

            command.Add(new UpSetTargetById() { InConstId = backup.Id });
            command.Add(new UpGetObjectData() { InConstObjectData = (int)ObjectData.ID, OutGoalData = GL_TEMP });
            command.Add(new Goal() { InConstGoalId = GL_TEMP }, "==", backup.Id, new SetGoal() { InConstGoalId = GL_TARGET_ID, InConstValue = backup.Id });
            command.Add(new UpSetTargetById() { InConstId = target.Id });
            command.Add(new UpGetObjectData() { InConstObjectData = (int)ObjectData.ID, OutGoalData = GL_TEMP });
            command.Add(new Goal() { InConstGoalId = GL_TEMP }, "==", target.Id, new SetGoal() { InConstGoalId = GL_TARGET_ID, InConstValue = target.Id });
            command.Add(new Goal() { InConstGoalId = GL_TARGET_ID }, "!=", -1, new UpModifyGoal() { IoGoalId = GL_CHECKS, MathOp = op_add, InOpValue = 1 });

            // check 5: unit is not already targeting GL_TARGET_ID

            command.Add(new UpSetTargetById() { InConstId = Id });
            command.Add(new UpGetObjectData() { InConstObjectData = (int)ObjectData.TARGET_ID, OutGoalData = GL_TEMP });
            command.Add(new UpModifyGoal() { IoGoalId = GL_TEMP, MathOp = op_g_min, InOpValue = GL_TARGET_ID });
            command.Add(new Goal() { InConstGoalId = GL_TEMP }, "!=", 0, new UpModifyGoal() { IoGoalId = GL_CHECKS, MathOp = op_add, InOpValue = 1 });

            // run if all checks passed

            command.Add(new Goal() { InConstGoalId = GL_CHECKS }, "==", 5,
                new UpSetTargetById() { InGoalId = GL_TARGET_ID },
                new UpFullResetSearch(),
                new UpAddObjectById() { InConstSearchSource = 1, InConstId = Id },
                new UpTargetObjects()
                {
                    InConstTarget = 1,
                    InConstTargetAction = action.HasValue ? (int)action.Value : (int)UnitAction.DEFAULT,
                    InConstFormation = formation.HasValue ? (int)formation.Value : -1,
                    InConstAttackStance = stance.HasValue ? (int)stance.Value : -1
                }
            );

            Bot.GameState.AddCommand(command);
        }

        public void Target(Position position, UnitAction? action = null, UnitFormation? formation = null, UnitStance? stance = null, int min_next_attack = int.MinValue, int max_next_attack = int.MaxValue)
        {
            RequestUpdate();

            if (!Bot.GameState.Map.IsOnMap(position.PointX, position.PointY))
            {
                throw new ArgumentOutOfRangeException(nameof(position));
            }

            if (Updated == false)
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

            const int GL_CHECKS = 100;
            const int GL_TEMP = 101;
            const int GL_PRECISE_X = 102;
            const int GL_PRECISE_Y = 103;

            var op_add = Bot.GameVersion == GameVersion.AOC ? 1 : 25;

            var command = new Command();

            command.Add(new SetGoal() { InConstGoalId = GL_CHECKS, InConstValue = 0 });
            command.Add(new SetGoal() { InConstGoalId = GL_TEMP, InConstValue = -1 });

            command.Add(new SetGoal() { InConstGoalId = GL_PRECISE_X, InConstValue = position.PreciseX });
            command.Add(new SetGoal() { InConstGoalId = GL_PRECISE_Y, InConstValue = position.PreciseY });

            // check 1: unit exists

            command.Add(new UpSetTargetById() { InConstId = Id });
            command.Add(new UpGetObjectData() { InConstObjectData = (int)ObjectData.ID, OutGoalData = GL_TEMP });
            command.Add(new Goal() { InConstGoalId = GL_TEMP }, "==", Id, 
                new UpModifyGoal() { IoGoalId = GL_CHECKS, MathOp = op_add, InOpValue = 1 });

            // check 2: next_attack >= min_next_attack

            command.Add(new UpGetObjectData() { InConstObjectData = (int)ObjectData.NEXT_ATTACK, OutGoalData = GL_TEMP });
            command.Add(new Goal() { InConstGoalId = GL_TEMP }, ">=", min_next_attack, 
                new UpModifyGoal() { IoGoalId = GL_CHECKS, MathOp = op_add, InOpValue = 1 });

            // check 3: next_attack <= max_next_attack

            command.Add(new Goal() { InConstGoalId = GL_TEMP }, "<=", max_next_attack, 
                new UpModifyGoal() { IoGoalId = GL_CHECKS, MathOp = op_add, InOpValue = 1 });

            // run if all checks pass

            command.Add(new Goal() { InConstGoalId = GL_CHECKS }, "==", 3,
                new SetStrategicNumber() { InConstSnId = (int)StrategicNumber.TARGET_POINT_ADJUSTMENT, InConstValue = 6 },
                new UpFullResetSearch(),
                new UpAddObjectById() { InConstSearchSource = 1, InConstId = Id },
                new UpTargetPoint()
                {
                    InGoalPoint = GL_PRECISE_X,
                    InConstTargetAction = action.HasValue ? (int)action.Value : -1,
                    InConstFormation = formation.HasValue ? (int)formation.Value : -1,
                    InConstAttackStance = stance.HasValue ? (int)stance.Value : -1
                }
            );

            Bot.GameState.AddCommand(command);
        }

        protected override IEnumerable<IMessage> RequestElementUpdate()
        {
            yield return new UpSetTargetById() { InConstId = Id };
            yield return new UpGetObjectData() { InConstObjectData = (int)ObjectData.POINT_X, OutGoalData = 100 };
            yield return new UpGetObjectData() { InConstObjectData = (int)ObjectData.POINT_Y, OutGoalData = 101 };
            yield return new UpPointExplored() { InGoalPoint = 100 };
            yield return new UpObjectDataList();
        }

        protected override void UpdateElement(IReadOnlyList<Any> responses)
        {
            var visible = responses[3].Unpack<UpPointExploredResult>().Result == 15;
            var data = responses[4].Unpack<UpObjectDataListResult>().Result.ToArray();
            var id = data[(int)ObjectData.ID];
            var player = data[(int)ObjectData.PLAYER];

            Targetable = true;
            Visible = true;

            if (id != Id)
            {
                Targetable = false;
                Visible = false;

                return;
            }

            if (player > 0 && (visible == false || data[(int)ObjectData.GARRISONED] == 1))
            {
                if (data[(int)ObjectData.CATEGORY] != 80)
                {
                    Visible = false;
                }
                
                return;
            }

            if (visible)
            {
                LastSeenGameTime = Bot.GameState.GameTime;
            }

            var old_pos = Position;
            var old_tick = LastUpdateTick;

            for (int i = 0; i < data.Length; i++)
            {
                Data[(ObjectData)i] = data[i];
            }

            var new_pos = Position;
            var new_tick = Bot.GameState.Tick;
            var time = (new_tick - old_tick) * Bot.GameState.GameTimePerTick.TotalSeconds;
            var v = (new_pos - old_pos) / Math.Max(0.00001, time);
            if (v.Norm > this[ObjectData.SPEED] / 80d)
            {
                v /= v.Norm;
                v *= this[ObjectData.SPEED] / 100d;
            }

            Velocity += v;
            Velocity /= 2;
        }
    }
}
