using System;
using System.Collections.Generic;
using System.Text;

namespace AoE2Lib.Bots
{
    public class UnitTypeInfo : GameElement
    {
        public int PlayerNumber { get; private set; } = -1; // 10
        public int TypeId { get; private set; } = -1; // 2000
        public int MaxHitpoints { get; private set; } = -1; // 1000
        public int Range { get; private set; } = -1; // 25
        public double Speed { get; private set; } = -1; // 130
        public int MeleeAttack { get; private set; } = -1; // 250
        public int PierceAttack { get; private set; } = -1; // 250
        public int MeleeArmor { get; private set; } = 0; // 250
        public int PierceArmor { get; private set; } = 0; // 250
        public TimeSpan ReloadTime { get; private set; } = TimeSpan.MinValue; // 50

        public UnitTypeInfo(int player, int type)
        {
            PlayerNumber = player;
            TypeId = type;
        }

        internal void Update(int goal0, int goal1, int goal2)
        {
            var player = (goal0 % 10) - 1;
            goal0 /= 10;

            if (player != PlayerNumber)
            {
                throw new ArgumentException("Incorrect player: " + player);
            }

            var type = (goal0 % 2000) - 1;
            goal0 /= 2000;

            if (type != TypeId)
            {
                throw new ArgumentException("Incorrect type id: " + type);
            }

            MaxHitpoints = (goal0 % 1000) - 1;
            goal0 /= 1000;
            Range = (goal0 % 25) - 1;

            Speed = ((goal1 % 130) - 1) / 50d;
            goal1 /= 130;
            MeleeAttack = (goal1 % 250) - 1;
            goal1 /= 250;
            PierceAttack = (goal1 % 250) - 1;
            goal1 /= 250;
            MeleeArmor = (goal1 % 250);

            PierceArmor = (goal2 % 250);
            goal2 /= 250;
            var timer = (goal2 % 50) - 1;
            ReloadTime = TimeSpan.FromSeconds(timer / 5d);

            ElementUpdated();
        }
    }
}
