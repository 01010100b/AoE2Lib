using AoE2Lib.Bots.Modules;
using AoE2Lib.Utils;
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

        public void TargetUnit(Unit target, UnitAction action, UnitFormation formation, UnitStance stance)
        {
            Bot.MicroModule.TargetUnit(this, target, action, formation, stance);
        }

        public void TargetPosition(Position target, UnitAction action, UnitFormation formation, UnitStance stance)
        {
            Bot.MicroModule.TargetPosition(this, target, action, formation, stance);
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

            if (player != 0 && data[(int)ObjectData.CATEGORY] != 80 && (visible == false || data[(int)ObjectData.GARRISONED] == 1))
            {
                Visible = false;
                
                return;
            }

            if (player != 0 && visible == false)
            {
                return;
            }

            var info = Bot.InfoModule;
            var old_pos = Position;
            var old_tick = LastUpdateTick;

            for (int i = 0; i < data.Length; i++)
            {
                Data[(ObjectData)i] = data[i];
            }

            var new_pos = Position;
            var new_tick = Bot.Tick;
            var time = (new_tick - old_tick) * info.GameTimePerTick.TotalSeconds;
            var v = (new_pos - old_pos) / Math.Max(0.001, time);
            if (v.Norm > this[ObjectData.SPEED] / 100d)
            {
                v /= v.Norm;
            }

            Velocity += v;
            Velocity /= 2;
        }
    }
}
