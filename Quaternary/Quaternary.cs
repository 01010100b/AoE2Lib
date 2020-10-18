using AoE2Lib;
using AoE2Lib.Bots;
using AoE2Lib.Bots.GameElements;
using AoE2Lib.Mods;
using Quaternary.Strategies;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AoE2Lib.Bots.Command;

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
        private Strategy Strategy { get; set; } = null;

        protected override void StartGame()
        {
            StateLog = "";
            Strategy = new BasicStrategy();
        }

        protected override Command GetNextCommand()
        {
            var command = Strategy.GetNextCommand(this, GameState);

            foreach (var kvp in Strategy.StrategicNumbers)
            {
                SetStrategicNumber(kvp.Key, kvp.Value);
            }

            if (Tick % 20 == 0)
            {
                lock (this)
                {
                    StateLog = LogState();
                }
            }

            return command;
        }

        private string LogState()
        {
            var sb = new StringBuilder();

            var units = new int[9];
            var targets = new int[9];
            var wood = 0;
            var food = 0;
            var gold = 0;
            var stone = 0;
            foreach (var unit in GameState.Units.Values)
            {
                units[unit.PlayerNumber]++;

                if (unit.Targetable)
                {
                    targets[unit.PlayerNumber]++;
                }

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
                    sb.AppendLine($"Player {i} with {units[i]} known units of which {targets[i]} targetable");
                }
            }

            return sb.ToString();

        }
    }
}
