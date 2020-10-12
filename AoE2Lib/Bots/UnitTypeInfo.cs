using System;
using System.Collections.Generic;
using System.Text;

namespace AoE2Lib.Bots
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
        public bool Heresy { get; private set; } = false; // 2
        public bool Faith { get; private set; } = false; // 2
        public bool Redemption { get; private set; } = false; // 2
        public bool Atonement { get; private set; } = false; // 2
        public bool Theocracy { get; private set; } = false; // 2
        public bool Ballistics { get; private set; } = false; // 2

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

            PierceArmor = goal2 % 250;
            goal2 /= 250;
            var timer = goal2 % 50;
            goal2 /= 50;
            ReloadTime = TimeSpan.FromSeconds(timer / 5d);
            TrainSiteId = goal2 % 2000;
            goal2 /= 2000;
            Heresy = (goal2 % 2) == 1;
            goal2 /= 2;
            Faith = (goal2 % 2) == 1;
            goal2 /= 2;
            Redemption = (goal2 % 2) == 1;
            goal2 /= 2;
            Atonement = (goal2 % 2) == 1;
            goal2 /= 2;
            Theocracy = (goal2 % 2) == 1;
            goal2 /= 2;
            Ballistics = (goal2 % 2) == 1;

            ElementUpdated();
        }
    }
}
