using System;
using System.Collections.Generic;
using System.Text;

namespace AoE2Lib.Mods
{
    public class Mod
    {
        public readonly Dictionary<int, UnitDef> UnitDefs = new Dictionary<int, UnitDef>();
        public UnitDef Villager { get; set; }
        public UnitDef House { get; set; }

        public void LoadDE()
        {
            UnitDefs.Clear();

            Villager = new UnitDef()
            {
                Id = 83,
                FoundationId = 83,
                CollisionX = 0.2,
                CollisionY = 0.2,
                CmdId = CmdId.VILLAGER
            };
            UnitDefs.Add(Villager.Id, Villager);

            House = new UnitDef() 
            { 
                Id = 70, 
                FoundationId = 70, 
                CollisionX = 1, 
                CollisionY = 1, 
                TerrainTable = 4,
                CmdId = CmdId.CIVILIAN_BUILDING
            };
            UnitDefs.Add(House.Id, House);
        }
    }
}
