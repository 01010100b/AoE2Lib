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
