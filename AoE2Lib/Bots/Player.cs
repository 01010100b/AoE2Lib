using AoE2Lib.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace AoE2Lib.Bots
{
    public class Player
    {
        public TimeSpan TimeSinceLastUpdate => DateTime.UtcNow - LastUpdate;
        public DateTime LastUpdate { get; private set; } = DateTime.MinValue;

        public int PlayerNumber { get; private set; } = -1; // 10
        public int Civilization { get; private set; } = -1; // 100
        public int Score { get; private set; } = -1; // 100000
        public int Age { get; private set; } = -1; // 10
        public int CivilianPopulation { get; private set; } = -1; // 1000
        public int MilitaryPopulation { get; private set; } = -1; // 1000
        public int Stance { get; private set; } = -1; // 10

        public void Update(int goal0, int goal1)
        {
            var data = goal0;

            PlayerNumber = (data % 10) - 1;
            data /= 10;
            Civilization = (data % 100) - 1;
            data /= 100;
            Score = (data % 100000) - 1;
            data /= 100000;
            Age = (data % 10) - 1;

            data = goal1;

            CivilianPopulation = (data % 1000) - 1;
            data /= 1000;
            MilitaryPopulation = (data % 1000) - 1;
            data /= 1000;
            Stance = (data % 10) - 1;

            LastUpdate = DateTime.UtcNow;
        }
    }
}
