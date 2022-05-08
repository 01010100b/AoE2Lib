using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unary
{
    public class Settings
    {
        public double AnimalKillRange { get; set; } = 4;
        public double VillagerRetaskDistanceCost { get; set; } = 0.1;
        public double DropsiteDistanceCost { get; set; } = 0.1;
        public int MaxHunters { get; set; } = 7;
        public int MaxWaitingFarmers { get; set; } = 3;
        // new
        public double MaxEatingGroupSize { get; set; } = 7;
        public double MaxEatingRange { get; set; } = 20;
        public double KillSheepRange { get; set; } = 3;
        public double DropsiteMaxResourceRange { get; set; } = 7;
        public double CombatRangedShootChance { get; set; } = 0.8;
        public double CombatRangedMinRangeFraction { get; set; } = 0.8;
        public int CombatRangedMovementBias { get; set; } = 5;
        public double CombatRangedFocusRange { get; set; } = -1;
    }
}
