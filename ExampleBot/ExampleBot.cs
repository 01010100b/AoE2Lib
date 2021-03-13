using AoE2Lib.Bots;
using AoE2Lib.Bots.Modules;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace ExampleBot
{
    class ExampleBot : Bot
    {
        public override string Name => "ExampleBot";

        protected override IEnumerable<Command> Update()
        {
            var info = GetModule<InfoModule>();
            var map = GetModule<MapModule>();
            var units = GetModule<UnitsModule>();

            Debug.WriteLine($"Tick {Tick} Game time {info.GameTime}");
            Debug.WriteLine($"Wood {info.WoodAmount} Food {info.FoodAmount} Gold {info.GoldAmount} Stone {info.StoneAmount}");
            Debug.WriteLine($"Explored {map.GetTiles().Count(t => t.Explored):N0} tiles of {map.Width * map.Height:N0}");
            Debug.WriteLine($"I have {units.Units.Values.Count(u => u.PlayerNumber == PlayerNumber):N0} units");
            Debug.WriteLine($"Gaia has {units.Units.Values.Count(u => u.PlayerNumber == 0):N0} units");
            Debug.WriteLine("");

            return Enumerable.Empty<Command>();
        }
    }
}
