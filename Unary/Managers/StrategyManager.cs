﻿using AoE2Lib;
using AoE2Lib.Bots.GameElements;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Unary.UnitControllers.VillagerControllers;
using static Unary.Strategy.BuildOrderCommand;

namespace Unary.Managers
{
    class StrategyManager : Manager
    {
        private Strategy Strategy { get; set; }
        private readonly List<Strategy> Strategies = new();

        public StrategyManager(Unary unary) : base(unary)
        {
            var folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Strategies");

            foreach (var file in Directory.EnumerateFiles(folder, "*.json"))
            {
                var str = File.ReadAllText(file);
                var strat = Strategy.Deserialize(str);
                Strategies.Add(strat);
            }
        }

        public int GetDesiredGatherers(Resource resource)
        {
            var gatherers = 0;
            var pop = Unary.GameState.MyPlayer.CivilianPopulation;

            for (int i = 0; i < Math.Min(pop, Strategy.Gatherers.Count); i++)
            {
                if (Strategy.Gatherers[i] == resource)
                {
                    gatherers++;
                }
            }

            if (pop > Strategy.Gatherers.Count)
            {
                pop -= Strategy.Gatherers.Count;
                var fraction = 0d;

                switch (resource)
                {
                    case Resource.FOOD: fraction = Strategy.ExtraFoodPercentage / 100d; break;
                    case Resource.WOOD: fraction = Strategy.ExtraWoodPercentage / 100d; break;
                    case Resource.GOLD: fraction = Strategy.ExtraGoldPercentage / 100d; break;
                    case Resource.STONE: fraction = Strategy.ExtraStonePercentage / 100d; break;
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

        public void Attack(Player player)
        {
            return;

            Unary.GameState.SetStrategicNumber(StrategicNumber.ENABLE_PATROL_ATTACK, 1);
            Unary.GameState.SetStrategicNumber(StrategicNumber.TARGET_PLAYER_NUMBER, player.PlayerNumber);
            Unary.GameState.SetStrategicNumber(StrategicNumber.MINIMUM_ATTACK_GROUP_SIZE, 1);
            Unary.GameState.SetStrategicNumber(StrategicNumber.MAXIMUM_ATTACK_GROUP_SIZE, 1);
            Unary.GameState.SetStrategicNumber(StrategicNumber.NUMBER_ATTACK_GROUPS, 1000);
            Unary.GameState.SetStrategicNumber(StrategicNumber.ZERO_PRIORITY_DISTANCE, 250);

            Unary.Log.Info($"Attacking player {player.PlayerNumber}");
        }

        public void Retreat()
        {
            Unary.GameState.SetStrategicNumber(StrategicNumber.NUMBER_ATTACK_GROUPS, 0);
        }

        internal override void Update()
        {
            if (Strategy == null)
            {
                ChooseStrategy();
            }
            else
            {
                PerformBuildOrder();
                TrainUnits();

                if (Strategy.AutoEcoTechs)
                {
                    DoAutoEcoTechs();
                }
            }
        }

        private void ChooseStrategy()
        {
            Strategy = Strategies[0];

            Unary.Log.Info($"Choose strategy: {Strategy.Name}");
        }

        private void PerformBuildOrder()
        {
            foreach (var bo in Strategy.BuildOrder)
            {
                if (bo.Type == BuildOrderCommandType.RESEARCH)
                {
                    var tech = Unary.GameState.GetTechnology(bo.Id);

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

                    Unary.ProductionManager.Research(tech, priority, blocking);

                    break;
                }
                else if (bo.Type == BuildOrderCommandType.UNIT)
                {
                    var unit = Unary.GameState.GetUnitType(bo.Id);

                    if (!unit.Updated)
                    {
                        break;
                    }

                    if (unit.CountTotal > 0)
                    {
                        continue;
                    }

                    if (!unit.Available)
                    {
                        Unary.Log.Debug($"Unit {unit.Id} not available");

                        break;
                    }

                    if (unit.IsBuilding)
                    {
                        Unary.ProductionManager.Build(unit, unit.CountTotal + 1, 1, Priority.PRODUCTION_BUILDING);
                    }
                    else
                    {
                        Unary.ProductionManager.Train(unit, unit.CountTotal + 1, 1, Priority.MILITARY);
                    }

                    break;
                }
            }
        }

        private void TrainUnits()
        {
            // military

            UnitType primary = null;
            
            foreach (var p in Strategy.PrimaryUnits)
            {
                var unit = Unary.GameState.GetUnitType(p);

                if (unit.Updated && unit.Available)
                {
                    primary = unit;
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
                    Unary.ProductionManager.Train(primary, 50, 10, Priority.MILITARY);
                }
            }

            // economy

            var max_civ = (int)Math.Round(0.6 * Unary.GameState.MyPlayer.PopulationCap);
            var villager = Unary.GameState.GetUnitType(Unary.Mod.Villager);
            Unary.ProductionManager.Train(villager, max_civ, 3, Priority.VILLAGER);
        }

        private void DoAutoEcoTechs()
        {
            var horse_collar = Unary.GameState.GetTechnology(14);
            var heavy_plow = Unary.GameState.GetTechnology(13);
            var crop_rotation = Unary.GameState.GetTechnology(12);

            horse_collar.Research(Priority.TECH);
            heavy_plow.Research(Priority.TECH);
            crop_rotation.Research(Priority.TECH);

            var double_bit_axe = Unary.GameState.GetTechnology(202);
            var bow_saw = Unary.GameState.GetTechnology(203);
            var two_man_saw = Unary.GameState.GetTechnology(221);

            double_bit_axe.Research(Priority.TECH);
            bow_saw.Research(Priority.TECH);
            two_man_saw.Research(Priority.TECH);

            var gold_mining = Unary.GameState.GetTechnology(55);
            var stone_mining = Unary.GameState.GetTechnology(278);
            var gold_shaft_mining = Unary.GameState.GetTechnology(182);
            var stone_shaft_mining = Unary.GameState.GetTechnology(279);

            gold_mining.Research(Priority.TECH);
            stone_mining.Research(Priority.TECH);
            gold_shaft_mining.Research(Priority.TECH);
            stone_shaft_mining.Research(Priority.TECH);

            var loom = Unary.GameState.GetTechnology(22);
            var wheelbarrow = Unary.GameState.GetTechnology(213);
            var hand_cart = Unary.GameState.GetTechnology(249);

            loom.Research(Priority.AGE_UP, false);
            wheelbarrow.Research(Priority.AGE_UP, false);
            hand_cart.Research(Priority.AGE_UP, false);
        }
    }
}
