using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YTY.AocDatLib;

namespace Unary
{
    class Mod
    {
        private const int PIERCE = 3;
        private const int MELEE = 4;

        private static readonly int[] TC_TECHS = { 22, 101, 102, 8, 213, 103, 280, 249 };

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
        private readonly Dictionary<int, DatUnit> Units = new();
        private readonly Dictionary<int, HashSet<int>> TerrainAllowances = new();
        private readonly Dictionary<int, Dictionary<int, DatUnit>> CivUnits = new();

        public Mod(DatFile datfile)
        {
            DatFile = datfile;

            foreach (var unit in DatFile.Civilizations.SelectMany(c => c.Units))
            {
                Units[unit.Id] = unit;
            }

            for (int table = 0; table < DatFile.TerrainRestrictions.Count; table++)
            {
                TerrainAllowances[table] = new HashSet<int>();

                for (int terrain = 0; terrain < DatFile.TerrainRestrictions[table].AccessibleDamageMultiplier.Count; terrain++)
                {
                    var dmg = DatFile.TerrainRestrictions[table].AccessibleDamageMultiplier[terrain];

                    if (dmg > 0.5)
                    {
                        TerrainAllowances[table].Add(terrain);
                    }
                }
            }

            for (int civ = 0; civ < DatFile.Civilizations.Count; civ++)
            {
                CivUnits.Add(civ, new());
                var units = CivUnits[civ];
                var c = DatFile.Civilizations[civ];

                foreach (var unit in c.Units)
                {
                    units.Add(unit.Id, unit);
                }
            }
        }

        public IEnumerable<int> GetUnits(int civ) => CivUnits[civ].Values.Where(u => u.TrainLocationId >= 0).Select(u => (int)u.Id);
        public double GetUnitWidth(int civ, int unit) => CivUnits[civ][unit].CollisionSizeX * 2;
        public double GetUnitHeight(int civ, int unit) => CivUnits[civ][unit].CollisionSizeY * 2;

        public bool BlocksPassage(int civ, int unit)
        {
            var def = CivUnits[civ][unit];

            switch((int)def.ObstructionType)
            {
                case 2:
                case 3:
                case 5:
                case 10: return true;
                default: return false;
            }
        }

        public bool CanPassTerrain(int civ, int unit, int terrain)
        {
            var table = CivUnits[civ][unit].TerrainRestriction;

            return TerrainAllowances[table].Contains(terrain);
        }

        public DatUnit GetUnitDef(int type_id) => Units[type_id];

        public Civilization GetCivilizationDef(int civ_id) => DatFile.Civilizations[civ_id];

        public IEnumerable<KeyValuePair<int, HashSet<int>>> GetTerrainTables() => TerrainAllowances;

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

        public bool IsTownCenterTech(int tech)
        {
            return TC_TECHS.Contains(tech);
        }
    }
}
