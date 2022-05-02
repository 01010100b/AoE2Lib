using AoE2Lib;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unary.UnitControllers;
using Unary.UnitControllers.VillagerControllers;
using static Unary.Managers.ResourceManager;

namespace Unary.Strategies
{
    internal class Strategy
    {
        public class BuildOrderCommand
        {
            public enum BuildOrderCommandType { RESEARCH, UNIT }

            public BuildOrderCommandType Type { get; set; }
            public int Id { get; set; }
        }

        public string Name { get; set; } = "";
        public List<Resource> Gatherers { get; set; } = new();
        public int ExtraFoodPercentage { get; set; } = 0;
        public int ExtraWoodPercentage { get; set; } = 0;
        public int ExtraGoldPercentage { get; set; } = 0;
        public int ExtraStonePercentage { get; set; } = 0;
        public List<BuildOrderCommand> BuildOrder { get; set; } = new();
        public List<int> PrimaryUnits { get; set; } = new();
        public List<int> SecondaryUnits { get; set; } = new();
        public int SecondaryUnitPercentage { get; set; } = 0;
        public bool AutoEcoTechs { get; set; } = false;

        private Unary Unary { get; set; }

        public void Update()
        {
            SetStrategicNumbers();
            PerformBuildOrder();
            TrainUnits();
        }

        internal void SetUnary(Unary unary)
        {
            Unary = unary;
        }

        private void SetStrategicNumbers()
        {
            // building
            Unary.GameState.SetStrategicNumber(StrategicNumber.DISABLE_BUILDER_ASSISTANCE, 1);
            Unary.GameState.SetStrategicNumber(StrategicNumber.ENABLE_NEW_BUILDING_SYSTEM, 1);
            Unary.GameState.SetStrategicNumber(StrategicNumber.INITIAL_EXPLORATION_REQUIRED, 0);
            
            // units
            Unary.GameState.SetStrategicNumber(StrategicNumber.CAP_CIVILIAN_BUILDERS, -1);
            Unary.GameState.SetStrategicNumber(StrategicNumber.CAP_CIVILIAN_EXPLORERS, 0);
            Unary.GameState.SetStrategicNumber(StrategicNumber.CAP_CIVILIAN_GATHERERS, 0);
            Unary.GameState.SetStrategicNumber(StrategicNumber.MAXIMUM_WOOD_DROP_DISTANCE, -2);
            Unary.GameState.SetStrategicNumber(StrategicNumber.MAXIMUM_GOLD_DROP_DISTANCE, -2);
            Unary.GameState.SetStrategicNumber(StrategicNumber.MAXIMUM_STONE_DROP_DISTANCE, -2);
            Unary.GameState.SetStrategicNumber(StrategicNumber.MAXIMUM_FOOD_DROP_DISTANCE, -2);
            Unary.GameState.SetStrategicNumber(StrategicNumber.MAXIMUM_HUNT_DROP_DISTANCE, -2);
            Unary.GameState.SetStrategicNumber(StrategicNumber.ENABLE_BOAR_HUNTING, 0);

            // engine scouting
            Unary.GameState.SetStrategicNumber(StrategicNumber.MINIMUM_EXPLORE_GROUP_SIZE, 1);
            Unary.GameState.SetStrategicNumber(StrategicNumber.MAXIMUM_EXPLORE_GROUP_SIZE, 1);
            Unary.GameState.SetStrategicNumber(StrategicNumber.NUMBER_EXPLORE_GROUPS, 1);
            Unary.GameState.SetStrategicNumber(StrategicNumber.HOME_EXPLORATION_TIME, 600);

            // maybe unnecessary
            Unary.GameState.SetStrategicNumber(StrategicNumber.LIVESTOCK_TO_TOWN_CENTER, 1);
            Unary.GameState.SetStrategicNumber(StrategicNumber.FOOD_GATHERER_PERCENTAGE, 0);
            Unary.GameState.SetStrategicNumber(StrategicNumber.WOOD_GATHERER_PERCENTAGE, 0);
            Unary.GameState.SetStrategicNumber(StrategicNumber.GOLD_GATHERER_PERCENTAGE, 0);
            Unary.GameState.SetStrategicNumber(StrategicNumber.STONE_GATHERER_PERCENTAGE, 0);
        }

        private void PerformBuildOrder()
        {

        }

        private void TrainUnits()
        {
            // economy

            var max_civ = (int)Math.Round(0.6 * Unary.GameState.MyPlayer.PopulationCap);

            if (Unary.GameState.TryGetUnitType(Unary.Mod.Villager, out var villager))
            {
                Unary.ResourceManager.Train(villager, max_civ, 3, Priority.VILLAGER);
            }
        }
    }
}
