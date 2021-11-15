using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unary
{
    class Mod
    {
        private static int[] TC_TECHS = { 22, 101, 102, 8, 213, 103, 280, 249 };

        public int Villager { get; private set; } = 83;
        public int TownCenter { get; private set; } = 109;
        public int TownCenterFoundation { get; private set; } = 621;
        public int House { get; private set; } = 70;
        public int Mill { get; private set; } = 68;
        public int MiningCamp { get; private set; } = 584;
        public int LumberCamp { get; private set; } = 562;
        public int Farm { get; private set; } = 50;
        public int Dock { get; private set; } = 45;
        public int FeudalAge { get; private set; } = 101;
        public int CastleAge { get; private set; } = 102;
        public int ImperialAge { get; private set; } = 103;

        private readonly List<int> Sheep = new();
        private readonly List<int> Deer = new();
        private readonly List<int> Boar = new();

        public Mod()
        {
            Sheep.AddRange(new[] { 705, 1596, 1598, 1600, 1060, 1243, 305, 1245, 594, 833, 1142 });
            Deer.AddRange(new[] { 65, 1239, 1026, 1019 });
            Boar.AddRange(new[] { 412, 822, 1139, 48 });
        }

        public void SetAoC()
        {
            throw new NotImplementedException();
        }

        public void SetWK()
        {
            throw new NotImplementedException();
        }

        public void SetDE()
        {
            throw new NotImplementedException();
        }

        public int GetBuildingSize(int type_id)
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

        public IEnumerable<int> GetSheep()
        {
            return Sheep;
        }

        public IEnumerable<int> GetDeer()
        {
            return Deer;
        }

        public IEnumerable<int> GetBoar()
        {
            return Boar;
        }

        public bool IsTownCenterTech(int tech)
        {
            return TC_TECHS.Contains(tech);
        }
    }
}
