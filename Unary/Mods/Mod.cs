using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unary.Mods
{
    public class Mod
    {
        public readonly Dictionary<int, UnitDef> UnitDefs = new Dictionary<int, UnitDef>();
        public UnitDef Villager { get; set; }
        public UnitDef House { get; set; }
        public UnitDef LumberCamp { get; set; }

        public void LoadDE()
        {
            Villager = new UnitDef() { Id = 83, FoundationId = 83, Class = UnitClass.Civilian };
            House = new UnitDef() { Id = 70, FoundationId = 70, Width = 2, Height = 2, Class = UnitClass.Building };
            LumberCamp = new UnitDef() { Id = 562, FoundationId = 562, Width = 2, Height = 2, Class = UnitClass.Building };

            UnitDefs.Clear();
            UnitDefs.Add(Villager.Id, Villager);
            UnitDefs.Add(House.Id, House);
            UnitDefs.Add(LumberCamp.Id, LumberCamp);
        }
    }
}
