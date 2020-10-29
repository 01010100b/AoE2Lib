using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace Unary.GameElements
{
    public class UnitTypeInfo : GameElement
    {
        public struct UnitTypeInfoKey
        {
            public readonly int Player;
            public readonly int TypeId;

            public UnitTypeInfoKey(int player, int type)
            {
                Player = player;
                TypeId = type;
            }
        }

        public readonly UnitTypeInfoKey Key;

        public int MaxHitpoints { get; private set; } = -1;
        public int Range { get; private set; } = -1;
        public double Speed { get; private set; } = -1;
        public int Attack { get; private set; } = -1;
        public int MeleeArmor { get; private set; } = 0;
        public int PierceArmor { get; private set; } = 0;
        public TimeSpan ReloadTime { get; private set; } = TimeSpan.MinValue;
        public int TrainSiteId { get; private set; } = -1;

        public UnitTypeInfo(UnitTypeInfoKey key) : base()
        {
            Key = key;
        }

        protected override void UpdateElement(IEnumerable<IMessage> responses)
        {
            throw new NotImplementedException();
        }
    }
}
