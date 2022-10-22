using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unary
{
    public class Settings
    {
        public double CivilianJobLookAheadMinutes { get; set; } = 3;
        public double KillAnimalRange { get; set; } = 4;
        public double EatAnimalRange { get; set; } = 7;
        public double KillSheepRange { get; set; } = 3;
        public int MaxDropsiteDistance { get; set; } = 3;
    }
}
