using System;
using System.Collections.Generic;
using System.Text;

namespace AoE2Lib.Bots.GameElements
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

        public int MaxHitpoints { get; private set; } = -1; // 1000
        public int Range { get; private set; } = -1; // 25
        public double Speed { get; private set; } = -1; // 130
        public int Attack { get; private set; } = -1; // 250
        public int MeleeArmor { get; private set; } = 0; // 250
        public int PierceArmor { get; private set; } = 0; // 250
        public TimeSpan ReloadTime { get; private set; } = TimeSpan.MinValue; // 50
        public int TrainSiteId { get; private set; } = -1; // 2000

        public UnitTypeInfo(UnitTypeInfoKey key) : base()
        {
            Key = key;
        }

        internal void Update(int goal0, int goal1, int goal2)
        {
            var player = goal0 % 10;
            goal0 /= 10;

            if (player != Key.Player)
            {
                throw new ArgumentException("Incorrect player: " + player);
            }

            var type = goal0 % 2000;
            goal0 /= 2000;

            if (type != Key.TypeId)
            {
                throw new ArgumentException("Incorrect type id: " + type);
            }

            MaxHitpoints = goal0 % 1000;
            goal0 /= 1000;
            Range = goal0 % 25;

            Speed = (goal1 % 130) / 50d;
            goal1 /= 130;
            Attack = goal1 % 250;
            goal1 /= 250;
            MeleeArmor = goal1 % 250;
            goal1 /= 250;
            PierceArmor = goal1 % 250;

            TrainSiteId = goal2 % 2000;
            goal2 /= 2000;
            ReloadTime = TimeSpan.FromSeconds((goal2 % 50) / 5d);

            ElementUpdated();
        }
    }
}
