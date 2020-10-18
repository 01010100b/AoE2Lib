using AoE2Lib.Mods;
using System;
using System.Collections.Generic;
using System.Text;

namespace AoE2Lib
{
    public class Mod
    {
        public readonly Dictionary<int, UnitDef> UnitDefs = new Dictionary<int, UnitDef>();
        public UnitDef Villager { get; private set; } = null;
        public UnitDef House { get; private set; } = null;
        public UnitDef TownCenter { get; private set; } = null;
        public UnitDef WoodDropsite { get; private set; } = null;
        public UnitDef FoodDropsite { get; private set; } = null;
        public UnitDef GoldDropsite { get; private set; } = null;
        public UnitDef StoneDropsite { get; private set; } = null;
        public UnitDef Farm { get; private set; } = null;

        public void LoadWK()
        {
            UnitDefs.Clear();

            UnitDefs.Add(59, new UnitDef() { TypeId = 59, UnitClass = UnitClass.BerryBush });
            UnitDefs.Add(1059, new UnitDef() { TypeId = 1059, UnitClass = UnitClass.BerryBush });

            UnitDefs.Add(705, new UnitDef() { TypeId = 705, UnitClass = UnitClass.Livestock });
            UnitDefs.Add(1596, new UnitDef() { TypeId = 1596, UnitClass = UnitClass.Livestock });
            UnitDefs.Add(1598, new UnitDef() { TypeId = 1598, UnitClass = UnitClass.Livestock });
            UnitDefs.Add(1600, new UnitDef() { TypeId = 1600, UnitClass = UnitClass.Livestock });
            UnitDefs.Add(1060, new UnitDef() { TypeId = 1060, UnitClass = UnitClass.Livestock });
            UnitDefs.Add(1243, new UnitDef() { TypeId = 1243, UnitClass = UnitClass.Livestock });
            UnitDefs.Add(305, new UnitDef() { TypeId = 305, UnitClass = UnitClass.Livestock });
            UnitDefs.Add(1245, new UnitDef() { TypeId = 1245, UnitClass = UnitClass.Livestock });
            UnitDefs.Add(594, new UnitDef() { TypeId = 594, UnitClass = UnitClass.Livestock });
            UnitDefs.Add(833, new UnitDef() { TypeId = 833, UnitClass = UnitClass.Livestock });
            UnitDefs.Add(1142, new UnitDef() { TypeId = 1142, UnitClass = UnitClass.Livestock });

            UnitDefs.Add(1063, new UnitDef() { TypeId = 1063, UnitClass = UnitClass.Tree });
            UnitDefs.Add(348, new UnitDef() { TypeId = 348, UnitClass = UnitClass.Tree });
            UnitDefs.Add(1052, new UnitDef() { TypeId = 1052, UnitClass = UnitClass.Tree });
            UnitDefs.Add(1051, new UnitDef() { TypeId = 1051, UnitClass = UnitClass.Tree });
            UnitDefs.Add(411, new UnitDef() { TypeId = 411, UnitClass = UnitClass.Tree });
            UnitDefs.Add(414, new UnitDef() { TypeId = 414, UnitClass = UnitClass.Tree });
            UnitDefs.Add(1144, new UnitDef() { TypeId = 1144, UnitClass = UnitClass.Tree });
            UnitDefs.Add(349, new UnitDef() { TypeId = 349, UnitClass = UnitClass.Tree });
            UnitDefs.Add(351, new UnitDef() { TypeId = 351, UnitClass = UnitClass.Tree });
            UnitDefs.Add(350, new UnitDef() { TypeId = 350, UnitClass = UnitClass.Tree });
            UnitDefs.Add(1146, new UnitDef() { TypeId = 1146, UnitClass = UnitClass.Tree });
            UnitDefs.Add(413, new UnitDef() { TypeId = 413, UnitClass = UnitClass.Tree });
            UnitDefs.Add(1347, new UnitDef() { TypeId = 1347, UnitClass = UnitClass.Tree });
            UnitDefs.Add(1348, new UnitDef() { TypeId = 1348, UnitClass = UnitClass.Tree });
            UnitDefs.Add(1249, new UnitDef() { TypeId = 1249, UnitClass = UnitClass.Tree });
            UnitDefs.Add(1248, new UnitDef() { TypeId = 1248, UnitClass = UnitClass.Tree });
            UnitDefs.Add(1349, new UnitDef() { TypeId = 1349, UnitClass = UnitClass.Tree });
            UnitDefs.Add(1350, new UnitDef() { TypeId = 1350, UnitClass = UnitClass.Tree });

            UnitDefs.Add(66, new UnitDef() { TypeId = 66, UnitClass = UnitClass.GoldMine });
            UnitDefs.Add(102, new UnitDef() { TypeId = 102, UnitClass = UnitClass.StoneMine });

            Villager = new UnitDef() { TypeId = 83, UnitClass = UnitClass.Civilian };
            House = new UnitDef() { TypeId = 70, UnitClass = UnitClass.Building };
            TownCenter = new UnitDef() { TypeId = 109, UnitClass = UnitClass.Building };
            WoodDropsite = new UnitDef() { TypeId = 562, UnitClass = UnitClass.Building };
            FoodDropsite = new UnitDef() { TypeId = 68, UnitClass = UnitClass.Building };
            GoldDropsite = new UnitDef() { TypeId = 584, UnitClass = UnitClass.Building };
            StoneDropsite = new UnitDef() { TypeId = 584, UnitClass = UnitClass.Building };
            Farm = new UnitDef() { TypeId = 50, UnitClass = UnitClass.Farm };

            UnitDefs.Add(TownCenter.TypeId, TownCenter);
            UnitDefs[WoodDropsite.TypeId] = WoodDropsite;
            UnitDefs[FoodDropsite.TypeId] = FoodDropsite;
            UnitDefs[GoldDropsite.TypeId] = GoldDropsite;
            UnitDefs[StoneDropsite.TypeId] = StoneDropsite;
            UnitDefs[Farm.TypeId] = Farm;
        }
    }
}
