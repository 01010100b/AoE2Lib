using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unary.Mods
{
    public class Mod
    {
        public readonly List<UnitDef> UnitDefs = new List<UnitDef>();
        public UnitDef Villager { get; set; }

        public void LoadWK()
        {
            Villager = new UnitDef() { Id = 83, FoundationId = 83 };

            UnitDefs.Clear();
            UnitDefs.Add(Villager);
        }
    }
}
