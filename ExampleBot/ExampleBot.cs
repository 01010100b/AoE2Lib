using AoE2Lib;
using AoE2Lib.Bots;
using AoE2Lib.Bots.Modules;
using AoE2Lib.Utils;
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
            var info = InfoModule;
            var map = MapModule;
            var units = UnitsModule;
            var foundations = units.Units.Values.Count(u => u.PlayerNumber == PlayerNumber && u[ObjectData.STATUS] == 0 && u[ObjectData.CATEGORY] == 80);

            Debug.WriteLine($"Tick {Tick} Game time {info.GameTime}");
            Debug.WriteLine($"Wood {info.WoodAmount} Food {info.FoodAmount} Gold {info.GoldAmount} Stone {info.StoneAmount}");
            Debug.WriteLine($"Explored {map.Tiles.Count(t => t.Explored):N0} tiles of {map.Width * map.Height:N0}");
            Debug.WriteLine($"I have {units.Units.Values.Count(u => u.PlayerNumber == PlayerNumber):N0} units");
            Debug.WriteLine($"Gaia has {units.Units.Values.Count(u => u.PlayerNumber == 0):N0} units");
            Debug.WriteLine($"I have {foundations} foundations");
            Debug.WriteLine("");

            if (Tick > 3)
            {
                var pos = info.MyPosition + new Position(Rng.Next(-10, 10), Rng.Next(-10, 10));
                units.Build(70, pos, 1000, 3);
            }

            
            info.StrategicNumbers[StrategicNumber.CAP_CIVILIAN_BUILDERS] = -1;
            info.StrategicNumbers[StrategicNumber.DISABLE_BUILDER_ASSISTANCE] = 1;

            return Enumerable.Empty<Command>();
        }
    }
}
