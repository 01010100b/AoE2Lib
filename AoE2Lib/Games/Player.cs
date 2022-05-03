using System;
using System.Collections.Generic;
using System.Text;

namespace AoE2Lib.Games
{
    public class Player
    {
        public int PlayerNumber { get; set; }
        public bool IsHuman { get; set; }
        public string AiFile { get; set; }
        public int Civilization { get; set; }
        public int Color { get; set; }
        public int Team { get; set; }
        public bool Exists { get; internal set; }
        public bool Alive { get; internal set; }
        public int Score { get; internal set; }
    }
}
