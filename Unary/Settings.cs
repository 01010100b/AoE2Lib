using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unary
{
    public class Settings
    {
        public int MinBuilders { get; set; } = 4;
        public double MaxBuildersPercentage { get; set; } = 0.2;
        public double JobLookAheadMinutes { get; set; } = 3;
        public double KillAnimalRange { get; set; } = 4;
        public double EatAnimalRange { get; set; } = 7;
        public double KillSheepRange { get; set; } = 3;
    }
}
