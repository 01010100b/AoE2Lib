﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unary
{
    public class Settings
    {
        public double CivilianJobLookAheadMinutes { get; set; } = 3;
        public double MilitaryJobLookAheadMinutes { get; set; } = 30;
        public double KillAnimalRange { get; set; } = 4;
        public double EatAnimalRange { get; set; } = 7;
        public double KillSheepRange { get; set; } = 3;
        public int MaxDropsiteDistance { get; set; } = 3;
        // economy
        public double ResourcePaymentExponent { get; set; } = 1;
        // combat
        public double ThreatAvoidanceFactor { get; set; } = 2;
        // scouting
        public double ScoutingTilesPerRegion { get; set; } = 1;
    }
}
