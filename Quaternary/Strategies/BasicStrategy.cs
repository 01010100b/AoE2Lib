using AoE2Lib.Bots;
using AoE2Lib.Bots.GameElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AoE2Lib.Bots.Command;

namespace Quaternary.Strategies
{
    class BasicStrategy : Strategy
    {
        protected override Command GetCommand(Quaternary quaternary, GameState state)
        {
            SetStrategicNumbers();

            var command = new Command();
            
            Economy(quaternary, state, command);
            CheckTiles(quaternary, state, command);
            CheckUnits(quaternary, state, command);
            CheckUnitTypeInfo(quaternary, state, command);
            CheckUnitTargetable(quaternary, state, command);

            return command;
            
            throw new NotImplementedException();
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

        private void SetStrategicNumber(StrategicNumber sn, int value)
        {
            StrategicNumbers[sn] = value;
        }

        private void Economy(Quaternary quaternary, GameState state, Command command)
        {
            var civ = 0;
            if (state.Players.TryGetValue(quaternary.PlayerNumber, out Player me))
            {
                civ = me.CivilianPopulation;
            }

            if (civ < 120)
            {
                command.Train(quaternary.Mod.Villager.TypeId, -1);
            }

            if (state.HousingHeadroom < 5 && state.PopulationHeadroom > 0)
            {
                command.Build(quaternary.Mod.House.TypeId, state.MyPosition, 1);
            }
        }

        private void CheckTiles(Quaternary quaternary, GameState state, Command command)
        {
            const int TILES_PER_COMMAND = 20;

            var tile_time = DateTime.UtcNow - TimeSpan.FromMinutes(3);
            if (state.GameTime < TimeSpan.FromMinutes(5))
            {
                tile_time = DateTime.UtcNow - TimeSpan.FromMinutes(0.5);
            }
            else if (state.GameTime < TimeSpan.FromMinutes(15))
            {
                tile_time = DateTime.UtcNow - TimeSpan.FromMinutes(1);
            }

            var tiles = state.Tiles.Values.ToList();
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
                        var da = a.Position.DistanceTo(state.MyPosition);
                        var db = b.Position.DistanceTo(state.MyPosition);

                        return da.CompareTo(db);
                    }
                }
            });

            for (int i = 0; i < Math.Min(tiles.Count, TILES_PER_COMMAND); i++)
            {
                command.CheckTile(tiles[i].Position);
            }
        }

        private void CheckUnits(Quaternary quaternary, GameState state, Command command)
        {
            const int UNIT_SEARCHES_PER_TICK = 5;

            var explored = state.Tiles.Values.Where(t => t.Explored).Select(t => t.Position).ToList();
            if (explored.Count == 0)
            {
                explored.Add(state.MyPosition);
            }

            for (int i = 0; i < UNIT_SEARCHES_PER_TICK; i++)
            {
                var player = quaternary.PlayerNumber;
                if (quaternary.RNG.NextDouble() < 0.5)
                {
                    player = 0;

                    if (quaternary.RNG.NextDouble() < 0.5)
                    {
                        player = quaternary.RNG.Next(9);

                        if (state.Players.Count > 0)
                        {
                            while (!state.Players.ContainsKey(player))
                            {
                                player = quaternary.RNG.Next(9);
                            }
                        }
                    }
                }

                var type = UnitSearchType.MILITARY;

                if (quaternary.RNG.NextDouble() < 0.5)
                {
                    type = UnitSearchType.CIVILIAN;

                    if (quaternary.RNG.NextDouble() < 0.5)
                    {
                        type = UnitSearchType.BUILDING;

                        if (quaternary.RNG.NextDouble() < 0.5)
                        {
                            type = UnitSearchType.FOOD;
                        }
                    }
                }

                if (player == 0)
                {
                    type = UnitSearchType.WOOD;

                    if (quaternary.RNG.NextDouble() < 0.5)
                    {
                        type = UnitSearchType.FOOD;

                        if (quaternary.RNG.NextDouble() < 0.5)
                        {
                            type = UnitSearchType.GOLD;

                            if (quaternary.RNG.NextDouble() < 0.5)
                            {
                                type = UnitSearchType.STONE;

                                if (quaternary.RNG.NextDouble() < 0.5)
                                {
                                    type = UnitSearchType.ALL;
                                }
                            }
                        }
                    }
                }

                var position = explored[quaternary.RNG.Next(explored.Count)];
                var radius = 20;

                command.SearchForUnits(player, position, radius, type);
            }
        }

        private void CheckUnitTypeInfo(Quaternary quaternary, GameState state, Command command)
        {
            var player = -1;
            var type = -1;
            var lastupdate = DateTime.MaxValue;

            foreach (var kvp in state.UnitTypeInfos)
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

        private void CheckUnitTargetable(Quaternary quaternary, GameState state, Command command)
        {
            var units = state.Units.Values.ToList();
            
            if (units.Count > 0)
            {
                if (quaternary.RNG.NextDouble() < 0.5)
                {
                    for (int i = 0; i < 10; i++)
                    {
                        var unit = units[quaternary.RNG.Next(units.Count)];

                        command.CheckUnitTargetable(unit.Id);
                    }
                }
                else
                {
                    units.Sort((a, b) => a.TargetableUpdate.CompareTo(b.TargetableUpdate));
                    for (int i = 0; i < Math.Min(10, units.Count); i++)
                    {
                        var unit = units[i];

                        command.CheckUnitTargetable(unit.Id);
                    }
                }
            }
            
        }
    }
}
