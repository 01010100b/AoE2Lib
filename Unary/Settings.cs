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
        public double CombatShootChance { get; set; } = 0.9;
        public double CombatMinRangeFraction { get; set; } = 0.8;
        public int CombatMovementBias { get; set; } = 5;

        public Settings Copy()
        {
            var settings = new Settings();

            foreach (var prop in typeof(Settings).GetProperties())
            {
                prop.SetValue(settings, prop.GetValue(this));
            }

            return settings;
        }
    }
}
