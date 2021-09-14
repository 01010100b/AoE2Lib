using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unary
{
    class Mod
    {
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

        public int GetUnitSize(int type_id)
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
    }
}
