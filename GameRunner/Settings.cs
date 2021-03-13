using System;
using System.Collections.Generic;
using System.Text;

namespace GameRunner
{
    public class Settings
    {
        public string Exe { get; set; } = "";
        public string AiFolder { get; set; } = "";
        public GameType GameType { get; set; }
        public string Scenario { get; set; } = "";
        public MapType MapType { get; set; }
        public MapSize MapSize { get; set; }
        public Difficulty Difficulty { get; set; }
        public StartingResources StartingResources { get; set; }
        public RevealMap RevealMap { get; set; }
        public StartingAge StartingAge { get; set; }
        public VictoryType VictoryType { get; set; }
        public int VictoryValue { get; set; }
        public int PopulationLimit { get; set; }
        public bool TeamsTogether { get; set; }
        public bool LockTeams { get; set; }
        public bool AllTechs { get; set; }
        public bool Recorded { get; set; }
    }
}
