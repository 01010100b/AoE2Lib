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
        public IReadOnlyDictionary<ObjectData, int> Data => _Data;
        private readonly Dictionary<ObjectData, int> _Data = new Dictionary<ObjectData, int>();

        public int PlayerNumber => GetData(ObjectData.PLAYER);
        public Position Position => Position.FromPrecise(GetData(ObjectData.PRECISE_X), GetData(ObjectData.PRECISE_Y));

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

        protected override IEnumerable<IMessage> RequestElementUpdate()
        {
            yield return new UpSetTargetById() { InConstId = Id };

            foreach (var data in System.Enum.GetValues(typeof(ObjectData)).Cast<ObjectData>())
            {
                if (data != ObjectData.LOCKED)
                {
                    yield return new UpObjectData() { InConstObjectData = (int)data };
                }
            }
        }

        protected override void UpdateElement(IReadOnlyList<Any> responses)
        {
            var i = 1;
            foreach (var data in System.Enum.GetValues(typeof(ObjectData)).Cast<ObjectData>())
            {
                if (data != ObjectData.LOCKED)
                {
                    var value = responses[i].Unpack<UpObjectDataResult>().Result;
                    _Data[data] = value;

                    i++;
                }
            }
        }
    }
}
