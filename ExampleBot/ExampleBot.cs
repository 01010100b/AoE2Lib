﻿using AoE2Lib;
using AoE2Lib.Bots;
using AoE2Lib.Bots.Modules;
using AoE2Lib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExampleBot
{
    class ExampleBot : Bot
    {
        public override string Name => "ExampleBot";
        public override int Id => 27421;

        private readonly Random RNG = new Random(Guid.NewGuid().GetHashCode());

        protected override IEnumerable<Command> Update()
        {
            // set strategic numbers
            SetStrategicNumbers();

            // get scout and send it to a random point on the map
            var scout = GetModule<UnitsModule>().Units.Values.FirstOrDefault(u => u.PlayerNumber == PlayerNumber && u.Speed > 1);

            if (scout == null)
            {
                Log.Debug($"Bot {Name} {PlayerNumber}: No scout");
            }
            else
            {
                // do it on average every 10 ticks
                if (RNG.NextDouble() < 0.1)
                {
                    // get the map and a random position
                    var map = GetModule<MapModule>();
                    var x = RNG.Next(map.Width);
                    var y = RNG.Next(map.Height);

                    // move the scout to the target position
                    scout.TargetPosition(Position.FromPoint(x, y), UnitAction.MOVE, UnitFormation.LINE, UnitStance.AGGRESSIVE);

                    Log.Info($"Bot {Name} {PlayerNumber}: Sending scout {scout.Id} to {x} {y}");
                }
            }

            // research loom
            GetModule<ResearchModule>().Research(22);

            // train a vill
            GetModule<UnitsModule>().Train(Mod.Villager);

            // get a position within about 10-14 tiles of the tc
            var pos = GetModule<InfoModule>().MyPosition;
            var dpos = Position.FromPoint(RNG.Next(-10, 10), RNG.Next(-10, 10));
            pos += dpos;

            // get the list of possible positions for placing a house within a range of 10, leave a clearance of 1 and use extra restrictions (farm space around tc etc)
            var positions = GetModule<PlacementModule>().GetPlacementPositions(Mod.House, pos, 1, true, 10).ToList();
            if (positions.Count > 0)
            {
                // pick a random position from that list
                pos = positions[RNG.Next(positions.Count)];
                // and build a house there building a max of 2 concurrently
                GetModule<UnitsModule>().Build(Mod.House, pos, int.MaxValue, 2);
            }

            LogState();

            return Enumerable.Empty<Command>();
        }

        private void SetStrategicNumbers()
        {
            var sns = GetModule<InfoModule>().StrategicNumbers;

            sns[StrategicNumber.PERCENT_CIVILIAN_EXPLORERS] = 0;
            sns[StrategicNumber.CAP_CIVILIAN_EXPLORERS] = 0;
            sns[StrategicNumber.TOTAL_NUMBER_EXPLORERS] = 0;
            sns[StrategicNumber.NUMBER_EXPLORE_GROUPS] = 0;

            sns[StrategicNumber.PERCENT_ENEMY_SIGHTED_RESPONSE] = 100;
            sns[StrategicNumber.ENEMY_SIGHTED_RESPONSE_DISTANCE] = 100;
            sns[StrategicNumber.ZERO_PRIORITY_DISTANCE] = 100;
            sns[StrategicNumber.ENABLE_OFFENSIVE_PRIORITY] = 1;
            sns[StrategicNumber.ENABLE_PATROL_ATTACK] = 1;
            sns[StrategicNumber.MINIMUM_ATTACK_GROUP_SIZE] = 1;
            sns[StrategicNumber.MAXIMUM_ATTACK_GROUP_SIZE] = 1;
            sns[StrategicNumber.DISABLE_DEFEND_GROUPS] = 8;
            sns[StrategicNumber.CONSECUTIVE_IDLE_UNIT_LIMIT] = 0;
            sns[StrategicNumber.WALL_TARGETING_MODE] = 1;

            sns[StrategicNumber.ENABLE_NEW_BUILDING_SYSTEM] = 1;
            sns[StrategicNumber.PERCENT_BUILDING_CANCELLATION] = 0;
            sns[StrategicNumber.DISABLE_BUILDER_ASSISTANCE] = 1;
            sns[StrategicNumber.CAP_CIVILIAN_BUILDERS] = 4;

            sns[StrategicNumber.INTELLIGENT_GATHERING] = 1;
            sns[StrategicNumber.USE_BY_TYPE_MAX_GATHERING] = 1;
            sns[StrategicNumber.ENABLE_BOAR_HUNTING] = 0;
            sns[StrategicNumber.LIVESTOCK_TO_TOWN_CENTER] = 1;

            sns[StrategicNumber.FOOD_GATHERER_PERCENTAGE] = 70;
            sns[StrategicNumber.WOOD_GATHERER_PERCENTAGE] = 20;
            sns[StrategicNumber.GOLD_GATHERER_PERCENTAGE] = 5;
            sns[StrategicNumber.STONE_GATHERER_PERCENTAGE] = 5;
        }

        private void LogState()
        {
            var players = GetModule<PlayersModule>().Players.Count;
            Log.Info($"Bot {Name} {PlayerNumber}: Number of players: {players}");

            var tiles = GetModule<MapModule>().GetTiles().ToList();
            Log.Info($"Bot {Name} {PlayerNumber}: Number of tiles: {tiles.Count:N0} of which {tiles.Count(t => t.Explored):N0} explored");

            var seconds = GetModule<InfoModule>().GameSecondsPerTick;
            Log.Info($"Bot {Name} {PlayerNumber}: Game seconds per tick: {seconds:N2}");

            var units = GetModule<UnitsModule>().Units;
            var speed = 0d;
            foreach (var unit in units.Values)
            {
                if (unit.Velocity.Norm > speed)
                {
                    speed = unit.Velocity.Norm;
                }
            }
            Log.Info($"Bot {Name} {PlayerNumber}: Number of units: {units.Count} with highest speed {speed:N2}");
        }
    }
}
