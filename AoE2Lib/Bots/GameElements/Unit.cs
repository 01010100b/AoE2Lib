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
        public int PlayerNumber => GetData(ObjectData.PLAYER);
        public Position Position => Position.FromPrecise(GetData(ObjectData.PRECISE_X), GetData(ObjectData.PRECISE_Y));

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

        protected override IEnumerable<IMessage> RequestElementUpdate()
        {
            yield return new UpSetTargetById() { InConstId = Id };
            yield return new UpObjectDataList();
        }

        protected override void UpdateElement(IReadOnlyList<Any> responses)
        {
            var v = responses[1].Unpack<UpObjectDataListResult>().Result.ToArray();
            for (int i = 0; i < v.Length; i++)
            {
                Data[(ObjectData)i] = v[i];
            }
        }
    }
}
