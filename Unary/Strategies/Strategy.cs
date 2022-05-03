using AoE2Lib;
using AoE2Lib.Bots.GameElements;
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
using static Unary.Managers.ResourcesManager;
using static Unary.Strategies.Strategy.BuildOrderCommand;

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

        public int GetDesiredGatherers(Resource resource)
        {
            var gatherers = 0;
            var pop = Unary.GameState.MyPlayer.CivilianPopulation;

            for (int i = 0; i < Math.Min(pop, Gatherers.Count); i++)
            {
                if (Gatherers[i] == resource)
                {
                    gatherers++;
                }
            }

            if (pop > Gatherers.Count)
            {
                pop -= Gatherers.Count;
                var fraction = 0d;

                switch (resource)
                {
                    case Resource.FOOD: fraction = ExtraFoodPercentage / 100d; break;
                    case Resource.WOOD: fraction = ExtraWoodPercentage / 100d; break;
                    case Resource.GOLD: fraction = ExtraGoldPercentage / 100d; break;
                    case Resource.STONE: fraction = ExtraStonePercentage / 100d; break;
                }

                gatherers += (int)Math.Round(pop * fraction);
            }

            return gatherers;
        }

        public int GetMinimumGatherers(Resource resource)
        {
            return (int)Math.Ceiling(GetDesiredGatherers(resource) * 0.9);
        }

        public int GetMaximumGatherers(Resource resource)
        {
            return (int)Math.Floor(GetDesiredGatherers(resource) * 1.1);
        }

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
            var req = new Dictionary<UnitType, int>();

            foreach (var bo in BuildOrder)
            {
                if (bo.Type == BuildOrderCommandType.RESEARCH)
                {
                    if (Unary.GameState.TryGetTechnology(bo.Id, out var tech))
                    {
                        if (!tech.Updated)
                        {
                            break;
                        }

                        if (tech.Started)
                        {
                            continue;
                        }

                        if (!tech.Available)
                        {
                            Unary.Log.Debug($"Tech {bo.Id} not available");

                            break;
                        }

                        var priority = Priority.TECH;
                        var blocking = true;

                        if (Unary.Mod.IsTownCenterTech(tech.Id))
                        {
                            priority = Priority.VILLAGER + 10;
                            blocking = false;
                        }

                        Unary.OldProductionManager.Research(tech, priority, blocking);

                        break;
                    }
                }
                else if (bo.Type == BuildOrderCommandType.UNIT)
                {
                    if (Unary.GameState.TryGetUnitType(bo.Id, out var unit))
                    {
                        if (!unit.Updated)
                        {
                            break;
                        }

                        if (!req.ContainsKey(unit))
                        {
                            req.Add(unit, 0);
                        }

                        req[unit]++;

                        if (unit.CountTotal >= req[unit])
                        {
                            continue;
                        }

                        if (!unit.Available)
                        {
                            Unary.Log.Debug($"Unit type {unit.Id} not available");

                            break;
                        }

                        if (unit.IsBuilding)
                        {
                            var possible_placements = Unary.TownManager.GetDefaultSortedPossiblePlacements(unit);
                            Unary.ResourcesManager.Build(unit, possible_placements, req[unit], 1, Priority.PRODUCTION_BUILDING);
                        }
                        else
                        {
                            Unary.ResourcesManager.Train(unit, req[unit], 1, Priority.MILITARY);
                        }

                        break;
                    }
                }
            }
        }

        private void TrainUnits()
        {
            // economy

            var max_civ = (int)Math.Round(0.6 * Unary.GameState.MyPlayer.PopulationCap);

            if (Unary.GameState.TryGetUnitType(Unary.Mod.Villager, out var villager))
            {
                Unary.ResourcesManager.Train(villager, max_civ, 3, Priority.VILLAGER);
            }

            // military

            UnitType primary = null;

            foreach (var p in PrimaryUnits)
            {
                if (Unary.GameState.TryGetUnitType(p, out var unit))
                {
                    if (unit.Updated && unit.Available)
                    {
                        primary = unit;
                    }
                }
            }

            if (primary == null)
            {
                Unary.Log.Debug($"No primary unit available");
            }
            else
            {
                Unary.Log.Info($"Primary unit {primary.Id}");

                if (primary.CountTotal < 50)
                {
                    Unary.ResourcesManager.Train(primary, 50, 10, Priority.MILITARY);
                }
            }
        }
    }
}
