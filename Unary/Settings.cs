using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unary
{
    public class Settings
    {
        public double MaxEatingGroupSize { get; set; } = 7;
        public double MaxEatingRange { get; set; } = 20;
        public double KillSheepRange { get; set; } = 3;
        public double DropsiteMaxResourceRange { get; set; } = 7;
        public double CombatShootChance { get; set; } = 0.9;
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
