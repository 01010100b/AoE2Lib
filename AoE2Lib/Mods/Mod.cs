using System;
using System.Collections.Generic;
using System.Text;

namespace AoE2Lib.Mods
{
    public class Mod
    {
        public readonly Dictionary<int, UnitDef> UnitDefs = new Dictionary<int, UnitDef>();
        public UnitDef Villager { get; set; }
        public UnitDef TownCenter { get; set; }
        public UnitDef House { get; set; }
        public UnitDef LumberCamp { get; set; }
        public UnitDef Mill { get; set; }
        public UnitDef Farm { get; set; }
        public UnitDef GoldCamp { get; set; }
        public UnitDef StoneCamp { get; set; }

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

            TownCenter = new UnitDef()
            {
                Id = 109,
                FoundationId = 621,
                CollisionX = 2,
                CollisionY = 2,
                CmdId = CmdId.CIVILIAN_BUILDING
            };
            UnitDefs.Add(TownCenter.Id, TownCenter);

            House = new UnitDef() 
            { 
                Id = 70, 
                FoundationId = 70, 
                CollisionX = 1, 
                CollisionY = 1, 
                CmdId = CmdId.CIVILIAN_BUILDING
            };
            UnitDefs.Add(House.Id, House);

            LumberCamp = new UnitDef()
            {
                Id = 562,
                FoundationId = 562,
                CollisionX = 1,
                CollisionY = 1,
                CmdId = CmdId.CIVILIAN_BUILDING
            };
            UnitDefs.Add(LumberCamp.Id, LumberCamp);

            Mill = new UnitDef()
            {
                Id = 68,
                FoundationId = 68,
                CollisionX = 1,
                CollisionY = 1,
                CmdId = CmdId.CIVILIAN_BUILDING
            };
            UnitDefs.Add(Mill.Id, Mill);

            Farm = new UnitDef()
            {
                Id = 50,
                FoundationId = 50,
                CollisionX = 1.5,
                CollisionY = 1.5,
                CmdId = CmdId.CIVILIAN_BUILDING
            };
            UnitDefs.Add(Farm.Id, Farm);

            GoldCamp = new UnitDef()
            {
                Id = 584,
                FoundationId = 584,
                CollisionX = 1,
                CollisionY = 1,
                CmdId = CmdId.CIVILIAN_BUILDING
            };
            UnitDefs.Add(GoldCamp.Id, GoldCamp);

            StoneCamp = GoldCamp;
        }
    }
}
