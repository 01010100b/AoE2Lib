using System;
using System.Collections.Generic;
using System.Text;

namespace AoE2Lib.Mods
{
    public class CivilizationDef
    {
        public int Id { get; set; }
        public Dictionary<int, UnitDef> UnitDefs { get; set; } = new Dictionary<int, UnitDef>();
        public UnitDef Villager { get; set; }
        public UnitDef TownCenter { get; set; }
        public UnitDef House { get; set; }
        public UnitDef LumberCamp { get; set; }
        public UnitDef Mill { get; set; }
        public UnitDef Farm { get; set; }
        public UnitDef GoldCamp { get; set; }
        public UnitDef StoneCamp { get; set; }
    }
}
