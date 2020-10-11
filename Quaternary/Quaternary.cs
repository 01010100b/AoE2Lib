using AoE2Lib;
using AoE2Lib.Bots;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AoE2Lib.Bots.Command;
using static AoE2Lib.Bots.Command.UnitSearchCommand;
using static AoE2Lib.Bots.UnitTypeInfo;

namespace Quaternary
{
    class Quaternary : Bot
    {
        public override string Name => "Quaternary";
        public override int Id => 27613;

        protected override void StartGame()
        {

        }

        protected override Command GetNextCommand()
        {
            SetStrategicNumbers();

            var command = new Command();

            Economy(command);

            CheckTiles(command);
            CheckUnits(command);
            CheckUnitTypeInfo(command);

            if (Tick % 20 == 0)
            {
                LogState();
            }

            return command;
        }

        private void SetStrategicNumbers()
        {
            SetStrategicNumber(StrategicNumber.PERCENT_CIVILIAN_EXPLORERS, 0);
            SetStrategicNumber(StrategicNumber.CAP_CIVILIAN_EXPLORERS, 0);
            SetStrategicNumber(StrategicNumber.TOTAL_NUMBER_EXPLORERS, 1);
            SetStrategicNumber(StrategicNumber.NUMBER_EXPLORE_GROUPS, 1);

            SetStrategicNumber(StrategicNumber.PERCENT_ENEMY_SIGHTED_RESPONSE, 100);
            SetStrategicNumber(StrategicNumber.ENEMY_SIGHTED_RESPONSE_DISTANCE, 100);
            SetStrategicNumber(StrategicNumber.ZERO_PRIORITY_DISTANCE, 100);
            SetStrategicNumber(StrategicNumber.ENABLE_OFFENSIVE_PRIORITY, 1);
            SetStrategicNumber(StrategicNumber.ENABLE_PATROL_ATTACK, 1);
            SetStrategicNumber(StrategicNumber.MINIMUM_ATTACK_GROUP_SIZE, 1);
            SetStrategicNumber(StrategicNumber.MAXIMUM_ATTACK_GROUP_SIZE, 1);
            SetStrategicNumber(StrategicNumber.DISABLE_DEFEND_GROUPS, 8);
            SetStrategicNumber(StrategicNumber.CONSECUTIVE_IDLE_UNIT_LIMIT, 0);
            SetStrategicNumber(StrategicNumber.WALL_TARGETING_MODE, 1);

            SetStrategicNumber(StrategicNumber.TOWN_CENTER_PLACEMENT, 584);
            SetStrategicNumber(StrategicNumber.ENABLE_NEW_BUILDING_SYSTEM, 1);
            SetStrategicNumber(StrategicNumber.PERCENT_BUILDING_CANCELLATION, 0);
            SetStrategicNumber(StrategicNumber.DISABLE_BUILDER_ASSISTANCE, 1);
            SetStrategicNumber(StrategicNumber.DEFER_DROPSITE_UPDATE, 1);
            SetStrategicNumber(StrategicNumber.DROPSITE_SEPARATION_DISTANCE, 4);
            SetStrategicNumber(StrategicNumber.MILL_MAX_DISTANCE, 20);
            SetStrategicNumber(StrategicNumber.CAMP_MAX_DISTANCE, 20);
            SetStrategicNumber(StrategicNumber.CAP_CIVILIAN_BUILDERS, 4);

            SetStrategicNumber(StrategicNumber.INTELLIGENT_GATHERING, 1);
            SetStrategicNumber(StrategicNumber.USE_BY_TYPE_MAX_GATHERING, 1);
            SetStrategicNumber(StrategicNumber.MAXIMUM_WOOD_DROP_DISTANCE, 7);
            SetStrategicNumber(StrategicNumber.MAXIMUM_GOLD_DROP_DISTANCE, 7);
            SetStrategicNumber(StrategicNumber.MAXIMUM_STONE_DROP_DISTANCE, 7);
            SetStrategicNumber(StrategicNumber.MAXIMUM_FOOD_DROP_DISTANCE, 8);
            SetStrategicNumber(StrategicNumber.MAXIMUM_HUNT_DROP_DISTANCE, 8);
            SetStrategicNumber(StrategicNumber.ENABLE_BOAR_HUNTING, 0);
            SetStrategicNumber(StrategicNumber.LIVESTOCK_TO_TOWN_CENTER, 1);

            SetStrategicNumber(StrategicNumber.HOME_EXPLORATION_TIME, 600);

            SetStrategicNumber(StrategicNumber.FOOD_GATHERER_PERCENTAGE, 80);
            SetStrategicNumber(StrategicNumber.WOOD_GATHERER_PERCENTAGE, 20);
            SetStrategicNumber(StrategicNumber.GOLD_GATHERER_PERCENTAGE, 0);
            SetStrategicNumber(StrategicNumber.STONE_GATHERER_PERCENTAGE, 0);
        }

        private void Economy(Command command)
        {
            const int VILLAGER = 83;
            const int HOUSE = 70;

            var civ = 0;
            if (GameState.Players.TryGetValue(PlayerNumber, out Player me))
            {
                civ = me.CivilianPopulation;
            }

            if (civ < 120)
            {
                command.Train(VILLAGER, -1);
            }

            if (GameState.HousingHeadroom < 5 && GameState.PopulationHeadroom > 0)
            {
                command.Build(HOUSE, GameState.MyPosition, 1);
            }
        }

        private void CheckTiles(Command command)
        {
            const int TILES_PER_COMMAND = 20;

            var tile_time = DateTime.UtcNow - TimeSpan.FromMinutes(3);
            if (GameState.GameTime < TimeSpan.FromMinutes(5))
            {
                tile_time = DateTime.UtcNow - TimeSpan.FromMinutes(0.5);
            }
            else if (GameState.GameTime < TimeSpan.FromMinutes(15))
            {
                tile_time = DateTime.UtcNow - TimeSpan.FromMinutes(1);
            }

            var tiles = GameState.Tiles.Values.ToList();
            tiles.Sort((a, b) =>
            {
                var ca = a.LastUpdate < tile_time;
                var cb = b.LastUpdate < tile_time;

                if (ca && !cb)
                {
                    return -1;
                }
                else if (cb && !ca)
                {
                    return 1;
                }
                else
                {
                    if (b.Explored && !a.Explored)
                    {
                        return -1;
                    }
                    else if (a.Explored && !b.Explored)
                    {
                        return 1;
                    }
                    else
                    {
                        var da = a.Position.DistanceTo(GameState.MyPosition);
                        var db = b.Position.DistanceTo(GameState.MyPosition);

                        return da.CompareTo(db);
                    }
                }
            });
            
            for (int i = 0; i < Math.Min(tiles.Count, TILES_PER_COMMAND); i++)
            {
                command.CheckTile(tiles[i].Position);
            }
        }

        private void CheckUnits(Command command)
        {
            const int UNIT_SEARCHES_PER_TICK = 5;

            var explored = GameState.Tiles.Values.Where(t => t.Explored).Select(t => t.Position).ToList();
            if (explored.Count == 0)
            {
                explored.Add(GameState.MyPosition);
            }

            for (int i = 0; i < UNIT_SEARCHES_PER_TICK; i++)
            {
                var player = PlayerNumber;
                if (RNG.NextDouble() < 0.5)
                {
                    player = 0;

                    if (RNG.NextDouble() < 0.5)
                    {
                        player = RNG.Next(9);

                        if (GameState.Players.Count > 0)
                        {
                            while (!GameState.Players.ContainsKey(player))
                            {
                                player = RNG.Next(9);
                            }
                        }
                    }
                }

                var type = UnitSearchType.MILITARY;

                if (RNG.NextDouble() < 0.5)
                {
                    type = UnitSearchType.CIVILIAN;

                    if (RNG.NextDouble() < 0.5)
                    {
                        type = UnitSearchType.BUILDING;

                        if (RNG.NextDouble() < 0.5)
                        {
                            type = UnitSearchType.FOOD;
                        }
                    }
                }

                if (player == 0)
                {
                    type = UnitSearchType.WOOD;

                    if (RNG.NextDouble() < 0.5)
                    {
                        type = UnitSearchType.FOOD;

                        if (RNG.NextDouble() < 0.5)
                        {
                            type = UnitSearchType.GOLD;

                            if (RNG.NextDouble() < 0.5)
                            {
                                type = UnitSearchType.STONE;

                                if (RNG.NextDouble() < 0.5)
                                {
                                    type = UnitSearchType.ALL;
                                }
                            }
                        }
                    }
                }

                var position = explored[RNG.Next(explored.Count)];
                var radius = 20;

                command.SearchForUnits(new UnitSearchCommand(player, position, radius, type));
            }
        }

        private void CheckUnitTypeInfo(Command command)
        {
            var player = -1;
            var type = -1;
            var lastupdate = DateTime.MaxValue;

            foreach (var kvp in GameState.UnitTypeInfos)
            {
                if (kvp.Value.LastUpdate < lastupdate)
                {
                    player = kvp.Key.Player;
                    type = kvp.Key.TypeId;
                    lastupdate = kvp.Value.LastUpdate;
                }
            }

            command.CheckUnitTypeInfo(player, type);
        }

        private void LogState()
        {
            Log.Debug(GameState);
        }
    }
}
