﻿using AoE2Lib.Bots;
using AoE2Lib.Bots.GameElements;
using AoE2Lib.Bots.Modules;
using AoE2Lib.Mods;
using AoE2Lib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quaternary.Modules
{
    internal class BuildModule : Module
    {
        public int MaxBuildRange { get; set; } = 20;
        public int MaxFarmDistance { get; set; } = 5;

        private readonly Random RNG = new Random(Guid.NewGuid().GetHashCode());

        public void BuildNormal(UnitDef building, int max = int.MaxValue, int concurrent = int.MaxValue, int priority = 0)
        {
            var clearance = 1;
            if (building == Bot.Mod.Farm || building == Bot.Mod.LumberCamp)
            {
                clearance = 0;
            }

            var restricted = true;
            if (building == Bot.Mod.Farm)
            {
                restricted = false;
            }

            BuildNormal(building, clearance, restricted, max, concurrent, priority);
        }

        public void BuildNormal(UnitDef building, int clearance, bool restricted, int max = int.MaxValue, int concurrent = int.MaxValue, int priority = 0)
        {
            var d = MaxBuildRange / 2;

            for (int i = 0; i < 10; i++)
            {
                var pos = Bot.GetModule<InfoModule>().MyPosition;
                var dpos = Position.FromPoint(RNG.Next(-d, d), RNG.Next(-d, d));
                pos += dpos;

                var positions = Bot.GetModule<PlacementModule>().GetPlacementPositions(building, pos, clearance, restricted, d).ToList();
                if (positions.Count > 0)
                {
                    Bot.GetModule<UnitsModule>().Build(building, positions[RNG.Next(positions.Count)], max, concurrent, priority);
                    break;
                }
            }
        }

        public void BuildFarm(UnitDef farm, int max = int.MaxValue, int concurrent = int.MaxValue, int priority = 0)
        {
            var pos = Bot.GetModule<InfoModule>().MyPosition;
            var positions = Bot.GetModule<PlacementModule>().GetPlacementPositions(farm, pos, 0, false, MaxFarmDistance).ToList();
            if (positions.Count > 0)
            {
                positions.Sort((a, b) => a.DistanceTo(pos).CompareTo(b.DistanceTo(pos)));
                Bot.GetModule<UnitsModule>().Build(farm, positions[0], max, concurrent, priority);
            }
        }

        protected override IEnumerable<Command> RequestUpdate()
        {
            var my_pos = Bot.GetModule<InfoModule>().MyPosition;
            var pos = new Position(my_pos.X + RNG.Next(-MaxBuildRange, MaxBuildRange), my_pos.Y + RNG.Next(-MaxBuildRange, MaxBuildRange));
            Bot.GetModule<UnitsModule>().FindUnits(pos, 10, Bot.PlayerNumber, AoE2Lib.UnitFindType.BUILDING);

            return Enumerable.Empty<Command>();
        }

        protected override void Update()
        {
            
        }
    }
}