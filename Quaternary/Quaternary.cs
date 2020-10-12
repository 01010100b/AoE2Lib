using AoE2Lib;
using AoE2Lib.Bots;
using AoE2Lib.Mods;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AoE2Lib.Bots.Command;
using static AoE2Lib.Bots.UnitTypeInfo;

namespace Quaternary
{
    class Quaternary : Bot
    {
        public override string Name => "Quaternary";
        public override int Id => 27613;
        public string CurrentState
        {
            get
            {
                lock (this)
                {
                    return StateLog;
                }
            }
        }
        private string StateLog { get; set; } = "";

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
                lock (this)
                {
                    StateLog = LogState();
                }
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
            var civ = 0;
            if (GameState.Players.TryGetValue(PlayerNumber, out Player me))
            {
                civ = me.CivilianPopulation;
            }

            if (civ < 120)
            {
                command.Train(Mod.Villager.TypeId, -1);
            }

            if (GameState.HousingHeadroom < 5 && GameState.PopulationHeadroom > 0)
            {
                command.Build(Mod.House.TypeId, GameState.MyPosition, 1);
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

                command.SearchForUnits(player, position, radius, type);
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

        private string LogState()
        {
            var sb = new StringBuilder();

            var units = new int[9];
            var wood = 0;
            var food = 0;
            var gold = 0;
            var stone = 0;
            foreach (var unit in GameState.Units.Values)
            {
                units[unit.PlayerNumber]++;

                if ((unit.PlayerNumber == 0 || unit.PlayerNumber == PlayerNumber) && Mod.UnitDefs.TryGetValue(unit.TypeId, out UnitDef def))
                {
                    if (def.UnitClass == UnitClass.Tree)
                    {
                        wood++;
                    }
                    else if (def.UnitClass == UnitClass.BerryBush || def.UnitClass == UnitClass.Livestock)
                    {
                        food++;
                    }
                    else if (def.UnitClass == UnitClass.GoldMine)
                    {
                        gold++;
                    }
                    else if (def.UnitClass == UnitClass.StoneMine)
                    {
                        stone++;
                    }
                }
            }

            sb.AppendLine("--- CURRENT STATE ---");
            sb.AppendLine();

            sb.AppendLine($"Game Time: {GameState.GameTime} Position: X {GameState.MyPosition.X} Y {GameState.MyPosition.Y}");
            if (GameState.Players.TryGetValue(PlayerNumber, out Player me))
            {
                sb.AppendLine($"Military {me.MilitaryPopulation} Civilian {me.CivilianPopulation}");
            }
            sb.AppendLine($"Wood {GameState.WoodAmount} Food {GameState.FoodAmount} Gold {GameState.GoldAmount} Stone {GameState.StoneAmount}");
            sb.AppendLine($"Population Headroom {GameState.PopulationHeadroom} Housing Headroom {GameState.HousingHeadroom}");

            var explored = GameState.Tiles.Count(t => t.Value.Explored);
            sb.AppendLine($"Map tiles {GameState.Tiles.Count} explored {explored} ({explored / (double)GameState.Tiles.Count:P})");
            sb.AppendLine($"Found resources Wood {wood} Food {food} Gold {gold} Stone {stone}");
            sb.AppendLine();

            for (int i = 0; i < units.Length; i++)
            {
                if (GameState.Players.ContainsKey(i))
                {
                    sb.AppendLine($"Player {i} with {units[i]} known units");
                }
            }

            return sb.ToString();

        }
    }
}
