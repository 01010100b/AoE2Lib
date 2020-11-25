using AoE2Lib;
using AoE2Lib.Bots;
using AoE2Lib.Bots.GameElements;
using AoE2Lib.Bots.Modules;
using AoE2Lib.Mods;
using AoE2Lib.Utils;
using Protos.Expert.Action;
using Quaternary.Algorithms;
using Quaternary.Modules;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Quaternary.Modules.MapAnalysisModule;
using static Quaternary.Modules.WallingModule;

namespace Quaternary
{
    class Quaternary : Bot
    {
        public override string Name => "Quaternary";
        public override int Id => 27432;

        private readonly Random RNG = new Random(Guid.NewGuid().GetHashCode());
        private Wall CurrentWall { get; set; }

        protected override IEnumerable<Command> Update()
        {
            var command = new Command();

            AddModules();
            SetStrategicNumbers();

            var info = GetModule<InfoModule>();
            var build = GetModule<BuildModule>();

            // research loom
            GetModule<ResearchModule>().Research(22);

            // train vill
            GetModule<UnitsModule>().Train(Mod.Villager);

            // build mill
            if (info.GameTime > TimeSpan.FromMinutes(2))
            {
                build.BuildNormal(Mod.Mill, 1, 1);
            }

            // build farm
            build.BuildFarm(Mod.Farm, 10, 1);

            // build house
            build.BuildNormal(Mod.House, 5, 2);

            LogState();

            yield return command;
        }

        private void AddModules()
        {
            if (!HasModule<MapAnalysisModule>())
            {
                AddModule(new MapAnalysisModule());
            }

            if (!HasModule<WallingModule>())
            {
                AddModule(new WallingModule());
            }

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
            sns[StrategicNumber.CAP_CIVILIAN_BUILDERS] = 8;
            sns[StrategicNumber.DISABLE_BUILDER_ASSISTANCE] = 1;
            sns[StrategicNumber.INITIAL_EXPLORATION_REQUIRED] = 0;

            sns[StrategicNumber.INTELLIGENT_GATHERING] = 1;
            sns[StrategicNumber.USE_BY_TYPE_MAX_GATHERING] = 1;
            //sns[StrategicNumber.MAXIMUM_WOOD_DROP_DISTANCE] = 4;
            //sns[StrategicNumber.MAXIMUM_GOLD_DROP_DISTANCE] = 4;
            //sns[StrategicNumber.MAXIMUM_STONE_DROP_DISTANCE] = 4;
            //sns[StrategicNumber.MAXIMUM_FOOD_DROP_DISTANCE] = 4;
            //sns[StrategicNumber.MAXIMUM_HUNT_DROP_DISTANCE] = 10;
            sns[StrategicNumber.ENABLE_BOAR_HUNTING] = 0;
            sns[StrategicNumber.LIVESTOCK_TO_TOWN_CENTER] = 1;
            sns[StrategicNumber.FOOD_GATHERER_PERCENTAGE] = 70;
            sns[StrategicNumber.WOOD_GATHERER_PERCENTAGE] = 20;
            sns[StrategicNumber.GOLD_GATHERER_PERCENTAGE] = 5;
            sns[StrategicNumber.STONE_GATHERER_PERCENTAGE] = 5;
        }

        private Wall GetWall()
        {
            var center = GetModule<InfoModule>().MyPosition;
            var map = GetModule<MapAnalysisModule>();

            var chosen = new List<List<AnalysisTile>>();
            
            foreach (var clumps in map.Clumps.Values)
            {
                clumps.Sort((a, b) => a.Min(t => center.DistanceTo(t.Point)).CompareTo(b.Min(t => center.DistanceTo(t.Point))));
            }

            if (map.Clumps.TryGetValue(Resource.WOOD, out List<List<AnalysisTile>> woodclumps))
            {
                foreach (var wood in woodclumps.Where(c => c.Count >= 10))
                {
                    chosen.Add(wood);

                    if (chosen.Count >= 2)
                    {
                        break;
                    }
                }
            }

            if (map.Clumps.TryGetValue(Resource.FOOD, out List<List<AnalysisTile>> foodclumps))
            {
                if (foodclumps.Count > 0)
                {
                    chosen.Add(foodclumps[0]);
                }
            }

            if (map.Clumps.TryGetValue(Resource.GOLD, out List<List<AnalysisTile>> goldclumps))
            {
                if (goldclumps.Count > 0)
                {
                    chosen.Add(goldclumps[0]);
                }
            }

            if (map.Clumps.TryGetValue(Resource.STONE, out List<List<AnalysisTile>> stoneclumps))
            {
                if (stoneclumps.Count > 0)
                {
                    chosen.Add(stoneclumps[0]);
                }
            }
            
            var walling = GetModule<WallingModule>();
            var goals = walling.GetGoals(center, 10, chosen);
            var wall = walling.GetWall(goals, 2);

            return wall;
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
