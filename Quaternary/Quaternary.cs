using AoE2Lib;
using AoE2Lib.Bots;
using AoE2Lib.Bots.GameElements;
using AoE2Lib.Bots.Modules;
using AoE2Lib.Mods;
using AoE2Lib.Utils;
using Quaternary.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quaternary
{
    class Quaternary : Bot
    {
        public override string Name => "Quaternary";
        public override int Id => 27432;

        private readonly Random RNG = new Random(Guid.NewGuid().GetHashCode());

        protected override IEnumerable<Command> Update()
        {
            AddModules();
            SetStrategicNumbers();

            var my_pos = GetModule<InfoModule>().MyPosition;

            // research loom
            GetModule<ResearchModule>().Research(22);

            // train vill
            GetModule<UnitsModule>().Train(Mod.Villager);

            // build mill
            if (GetModule<InfoModule>().GameTime > TimeSpan.FromMinutes(2))
            {
                GetModule<BuildModule>().BuildNormal(Mod.Mill, 2, 1);
            }

            // build farm
            GetModule<BuildModule>().BuildFarm(Mod.Farm, 10, 1);

            // build house
            GetModule<BuildModule>().BuildNormal(Mod.House, 100, 2);

            LogState();

            return Enumerable.Empty<Command>();
        }

        private void AddModules()
        {
            if (!HasModule<UnitManagerModule>())
            {
                AddModule(new UnitManagerModule());
            }

            if (!HasModule<ScoutingModule>())
            {
                AddModule(new ScoutingModule());
            }

            if (!HasModule<BuildModule>())
            {
                AddModule(new BuildModule());
            }
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
            sns[StrategicNumber.CONSECUTIVE_IDLE_UNIT_LIMIT] = 0;
            sns[StrategicNumber.WALL_TARGETING_MODE] = 1;

            sns[StrategicNumber.ENABLE_NEW_BUILDING_SYSTEM] = 1;
            sns[StrategicNumber.PERCENT_BUILDING_CANCELLATION] = 0;
            sns[StrategicNumber.DISABLE_BUILDER_ASSISTANCE] = 1;
            sns[StrategicNumber.CAP_CIVILIAN_BUILDERS] = 4;

            sns[StrategicNumber.INTELLIGENT_GATHERING] = 1;
            sns[StrategicNumber.USE_BY_TYPE_MAX_GATHERING] = 1;
            //sns[StrategicNumber.MAXIMUM_WOOD_DROP_DISTANCE] = 4;
            //sns[StrategicNumber.MAXIMUM_GOLD_DROP_DISTANCE] = 4;
            //sns[StrategicNumber.MAXIMUM_STONE_DROP_DISTANCE] = 4;
            //sns[StrategicNumber.MAXIMUM_FOOD_DROP_DISTANCE] = 4;
            //sns[StrategicNumber.MAXIMUM_HUNT_DROP_DISTANCE] = 10;
            sns[StrategicNumber.ENABLE_BOAR_HUNTING] = 0;
            sns[StrategicNumber.LIVESTOCK_TO_TOWN_CENTER] = 1;

            sns[StrategicNumber.HOME_EXPLORATION_TIME] = 600;

            sns[StrategicNumber.FOOD_GATHERER_PERCENTAGE] = 70;
            sns[StrategicNumber.WOOD_GATHERER_PERCENTAGE] = 20;
            sns[StrategicNumber.GOLD_GATHERER_PERCENTAGE] = 5;
            sns[StrategicNumber.STONE_GATHERER_PERCENTAGE] = 5;
        }

        private void LogState()
        {
            var players = GetModule<PlayersModule>().Players.Count;
            Log.Info($"Number of players: {players}");

            var tiles = GetModule<MapModule>().GetTiles().ToList();
            Log.Info($"Number of tiles: {tiles.Count:N0} of which {tiles.Count(t => t.Explored):N0} explored");

            var seconds = GetModule<InfoModule>().GameSecondsPerTick;
            Log.Info($"Game seconds per tick: {seconds:N2}");

            var units = GetModule<UnitsModule>().Units;
            var speed = 0d;
            foreach (var unit in units.Values)
            {
                if (unit.Velocity.Norm > speed)
                {
                    speed = unit.Velocity.Norm;
                }
            }
            Log.Info($"Number of units: {units.Count} with highest speed {speed:N2}");
        }
    }
}
