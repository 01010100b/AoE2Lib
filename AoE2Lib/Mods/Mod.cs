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
        public Dictionary<int, CivilizationDef> CivilizationDefs { get; set; } = new Dictionary<int, CivilizationDef>();
        public Dictionary<int, UnitDef> UnitDefs => CivilizationDefs[1].UnitDefs;
        public UnitDef Villager => CivilizationDefs[1].Villager;
        public UnitDef TownCenter => CivilizationDefs[1].TownCenter;
        public UnitDef House => CivilizationDefs[1].House;
        public UnitDef LumberCamp => CivilizationDefs[1].LumberCamp;
        public UnitDef Mill => CivilizationDefs[1].Mill;
        public UnitDef Farm => CivilizationDefs[1].Farm;
        public UnitDef GoldCamp => CivilizationDefs[1].GoldCamp;
        public UnitDef StoneCamp => CivilizationDefs[1].StoneCamp;

        public void Load(string file)
        {
            CivilizationDefs.Clear();

            if (!File.Exists(file))
            {
                Log.Static.Warning($"Dat file does not exist: {file}");
                return;
            }

            var dat = new DatFile(file);

            for (int i = 0; i < dat.Civilizations.Count; i++)
            {
                var civ = new CivilizationDef()
                {
                    Id = i
                };

                foreach (var unit in dat.Civilizations[i].Units)
                {
                    var def = new UnitDef()
                    {
                        Id = unit.Id,
                        Name = Encoding.ASCII.GetString(unit.Name.Where(b => b > 0).ToArray()),
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

                    civ.UnitDefs.Add(def.Id, def);
                }

                foreach (var def in civ.UnitDefs.Values)
                {
                    if (def.StackUnitId > 0)
                    {
                        civ.UnitDefs[def.StackUnitId].FoundationId = def.Id;
                    }
                }

                civ.Villager = civ.UnitDefs[83];
                civ.TownCenter = civ.UnitDefs[109];
                civ.House = civ.UnitDefs[70];
                civ.Mill = civ.UnitDefs[68];
                civ.Farm = civ.UnitDefs[50];
                civ.GoldCamp = civ.UnitDefs[584];
                civ.StoneCamp = civ.GoldCamp;

                CivilizationDefs.Add(civ.Id, civ);
            }

            Log.Static.Info($"Mod: Loaded {CivilizationDefs.Values.SelectMany(c => c.UnitDefs).Distinct().Count()} units");
        }
    }
}
