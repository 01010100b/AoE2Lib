using AoE2Lib.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using YTY.AocDatLib;

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

        public void LoadFromDat(DatFile dat)
        {
            UnitDefs.Clear();

            foreach (var unit in dat.Civilizations.SelectMany(c => c.Units))
            {
                if (!UnitDefs.ContainsKey(unit.Id))
                {
                    var def = new UnitDef()
                    {
                        Id = unit.Id,
                        FoundationId = unit.Id,
                        CollisionX = unit.CollisionSizeX,
                        CollisionY = unit.CollisionSizeY,
                        HillMode = unit.HillMode,
                        PlacementTerrain1 = unit.PlacementTerrain0,
                        PlacementTerrain2 = unit.PlacementTerrain1,
                        PlacementSideTerrain1 = unit.PlacementSideTerrain0,
                        PlacementSideTerrain2 = unit.PlacementSideTerrain1,
                        TerrainTable = unit.TerrainRestriction,
                        CmdId = (CmdId)unit.InterfaceKind,
                        StackUnitId = unit.StackUnitId
                    };

                    UnitDefs.Add(def.Id, def);
                }
            }

            foreach (var def in UnitDefs.Values)
            {
                if (def.StackUnitId > 0)
                {
                    UnitDefs[def.StackUnitId].FoundationId = def.Id;
                }
            }

            Villager = UnitDefs[83];
            TownCenter = UnitDefs[109];
            House = UnitDefs[70];
            Mill = UnitDefs[68];
            Farm = UnitDefs[50];
            GoldCamp = UnitDefs[584];
            StoneCamp = GoldCamp;

            Log.Static.Info($"Mod: Loaded {UnitDefs.Count} units");
        }
    }
}
