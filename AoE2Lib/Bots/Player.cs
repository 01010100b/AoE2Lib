using AoE2Lib.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace AoE2Lib.Bots
{
    public class Player : GameElement
    {
        public int PlayerNumber { get; private set; } = -1; // 10
        public int Civilization { get; private set; } = -1; // 100
        public int Score { get; private set; } = -1; // 100000
        public int Age { get; private set; } = -1; // 10
        public int CivilianPopulation { get; private set; } = -1; // 1000
        public int MilitaryPopulation { get; private set; } = -1; // 1000
        public int Stance { get; private set; } = -1; // 10

        public Player(int player)
        {
            PlayerNumber = player;
        }

        internal void Update(int goal0, int goal1)
        {
            var number = (goal0 % 10) - 1;
            goal0 /= 10;

            if (number != PlayerNumber)
            {
                throw new ArgumentException("Incorrect player number: " + number);
            }

            Civilization = (goal0 % 100) - 1;
            goal0 /= 100;
            Score = (goal0 % 100000) - 1;
            goal0 /= 100000;
            Age = (goal0 % 10) - 1;

            CivilianPopulation = (goal1 % 1000) - 1;
            goal1 /= 1000;
            MilitaryPopulation = (goal1 % 1000) - 1;
            goal1 /= 1000;
            Stance = (goal1 % 10) - 1;

            ElementUpdated();
        }
    }
}
