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
        public readonly Dictionary<int, TechDef> TechDefs = new Dictionary<int, TechDef>();

        public UnitDef TownCenter { get; set; }
        public UnitDef Villager { get; set; }
        public UnitDef House { get; set; }
        public UnitDef LumberCamp { get; set; }
        public UnitDef Mill { get; set; }
        public UnitDef Farm { get; set; }

        public void LoadDE()
        {
            UnitDefs.Clear();

            TownCenter = new UnitDef() { Id = 109, FoundationId = 621, Class = UnitClass.Building, Width = 4, Height = 4 };
            UnitDefs.Add(TownCenter.Id, TownCenter);

            Villager = new UnitDef() { Id = 83, FoundationId = 83, Class = UnitClass.Civilian };
            UnitDefs.Add(Villager.Id, Villager);

            House = new UnitDef() { Id = 70, FoundationId = 70, Width = 2, Height = 2, Class = UnitClass.Building };
            UnitDefs.Add(House.Id, House);

            LumberCamp = new UnitDef() { Id = 562, FoundationId = 562, Width = 2, Height = 2, Class = UnitClass.Building };
            UnitDefs.Add(LumberCamp.Id, LumberCamp);

            Mill = new UnitDef() { Id = 68, FoundationId = 68, Width = 2, Height = 2, Class = UnitClass.Building };
            UnitDefs.Add(Mill.Id, Mill);

            Farm = new UnitDef() { Id = 50, FoundationId = 50, Width = 3, Height = 3, Class = UnitClass.Farm };
            UnitDefs.Add(Farm.Id, Farm);

            TechDefs.Clear();

            var loom = new TechDef() { Id = 22 };
            TechDefs.Add(loom.Id, loom);
        }
    }
}
