using AoE2Lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using YTY.AocDatLib;

namespace Unary.Mods
{
    class Mod
    {
        private const int PIERCE = 3;
        private const int MELEE = 4;

        private static readonly int[] TC_TECHS = { 22, 101, 102, 8, 213, 103, 280, 249 };

        public static string GetString(byte[] bytes) => Encoding.ASCII.GetString(bytes, 0, bytes.Length - 1);

        public IEnumerable<CivInfo> Civs => CivInfos.Values;
        public int Villager { get; private set; } = 83;
        public int TownCenter { get; private set; } = 109;
        public int TownCenterFoundation { get; private set; } = 621;
        public int House { get; private set; } = 70;
        public int Mill { get; private set; } = 68;
        public int GoldMiningCamp { get; private set; } = 584;
        public int StoneMiningCamp { get; private set; } = 584;
        public int LumberCamp { get; private set; } = 562;
        public int Farm { get; private set; } = 50;
        public int Dock { get; private set; } = 45;
        public int FeudalAge { get; private set; } = 101;
        public int CastleAge { get; private set; } = 102;
        public int ImperialAge { get; private set; } = 103;

        private readonly DatFile DatFile;
        private readonly Dictionary<int, HashSet<int>> TerrainPassability = new();
        private readonly Dictionary<int, CivInfo> CivInfos = new();
        private readonly Dictionary<int, HashSet<KeyValuePair<int, Effect>>> TechUnitEffects = new();

        public Mod(DatFile datfile)
        {
            DatFile = datfile;
            Load();
        }

        public CivInfo GetCivInfo(int civ) => CivInfos[civ];
        public IEnumerable<KeyValuePair<int, Effect>> GetUnitEffects(int unit) => TechUnitEffects.ContainsKey(unit) ? TechUnitEffects[unit] : Enumerable.Empty<KeyValuePair<int, Effect>>();

        public bool IsTerrainPassable(int table, int terrain) => TerrainPassability[table].Contains(terrain);

        public int GetBuildingSizeOld(int type_id)
        {
            switch (type_id)
            {
                case 1665: // donjon
                case 70: // house
                case 463:
                case 464:
                case 465:
                case 68: // mill
                case 129:
                case 130:
                case 131:
                case 584: // mining camp
                case 585:
                case 586:
                case 587:
                case 562: // lumber camp
                case 563:
                case 564:
                case 565: return 2;
                case 45: // dock
                case 133:
                case 47:
                case 51:
                case 1189: // harbor
                case 50: // farm
                case 103: // blacksmith
                case 18:
                case 19:
                case 104: // monastery
                case 30:
                case 31:
                case 32:
                case 12: // barracks
                case 498:
                case 132:
                case 20:
                case 87: // archery range
                case 10:
                case 14:
                case 101: // stable
                case 86:
                case 153:
                case 1251: return 3; // krepost
                case 109: // town center
                case 71:
                case 141:
                case 142:
                case 621:
                case 84: // market
                case 116:
                case 137:
                case 209: // university
                case 210:
                case 49: // siege workshop
                case 82: return 4; // castle
                case 276: // wonder
                case 734: // feitoria
                case 1021: return 5;
                default: return 1;
            }
        }

        public bool IsTownCenterTechOld(int tech)
        {
            return TC_TECHS.Contains(tech);
        }

        private void Load()
        {
            // terrain tables

            for (int table = 0; table < DatFile.TerrainRestrictions.Count; table++)
            {
                TerrainPassability[table] = new HashSet<int>();

                for (int terrain = 0; terrain < DatFile.TerrainRestrictions[table].AccessibleDamageMultiplier.Count; terrain++)
                {
                    var dmg = DatFile.TerrainRestrictions[table].AccessibleDamageMultiplier[terrain];

                    if (dmg > 0)
                    {
                        TerrainPassability[table].Add(terrain);
                    }
                }
            }

            // tech unit effects

            var units_by_class = new Dictionary<int, HashSet<int>>();

            foreach (var unit in DatFile.Civilizations[0].Units)
            {
                var cls = (int)unit.UnitClass;

                if (!units_by_class.ContainsKey(cls))
                {
                    units_by_class.Add(cls, new());
                }

                units_by_class[cls].Add(unit.Id);
            }

            for (int tech_id = 0; tech_id < DatFile.Researches.Count; tech_id++)
            {
                var tech = DatFile.Researches[tech_id];

                if (tech.EffectId < 0)
                {
                    continue;
                }

                foreach (var effect in DatFile.Technologies[tech.EffectId].Effects)
                {
                    var effected_unit = GetEffectedUnit(effect);
                    var effected_class = GetEffectedClass(effect);

                    if (effected_unit >= 0)
                    {
                        if (!TechUnitEffects.ContainsKey(effected_unit))
                        {
                            TechUnitEffects.Add(effected_unit, new());
                        }

                        TechUnitEffects[effected_unit].Add(new(tech_id, effect));
                    }

                    if (effected_class >= 0 && units_by_class.ContainsKey(effected_class))
                    {
                        foreach (var unit_id in units_by_class[effected_class])
                        {
                            if (!TechUnitEffects.ContainsKey(unit_id))
                            {
                                TechUnitEffects.Add(unit_id, new());
                            }

                            TechUnitEffects[unit_id].Add(new(tech_id, effect));
                        }
                    }
                }
            }

            for (int civ = 0; civ < DatFile.Civilizations.Count; civ++)
            {
                var info = new CivInfo(this, DatFile, civ);
                CivInfos.Add(civ, info);
            }
        }

        private int GetEffectedUnit(Effect effect)
        {
            return (int)effect.Command switch
            {
                0 or 4 or 5 or 10 or 14 or 15 => effect.Arg1,
                2 or 3 or 12 or 13 => effect.Arg1,
                _ => -1
            };
        }

        private int GetEffectedClass(Effect effect)
        {
            return (int)effect.Command switch
            {
                0 or 4 or 5 or 10 or 14 or 15 => effect.Arg2,
                _ => -1
            };
        }
    }
}
