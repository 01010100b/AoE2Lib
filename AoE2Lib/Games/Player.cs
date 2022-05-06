using System;
using System.Collections.Generic;
using System.Text;

namespace AoE2Lib.Games
{
    public class Player
    {
        // for an explanation for these values, see the aoc-auto-game repo: https://github.com/FLWL/aoc-auto-game

        public int PlayerNumber { get; set; }
        public bool IsHuman { get; set; }
        public string AiFile { get; set; }
        public int Civilization { get; set; } = (int)Games.Civilization.RANDOM;
        public Color Color { get; set; }
        public Team Team { get; set; } = Team.NO_TEAM;
        public bool Exists { get; internal set; }
        public bool Alive { get; internal set; }
        public int Score { get; internal set; }
    }
}
