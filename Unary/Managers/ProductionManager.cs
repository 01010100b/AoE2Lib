using AoE2Lib;
using AoE2Lib.Bots;
using AoE2Lib.Bots.GameElements;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unary.Managers
{
    internal class ProductionManager : Manager
    {
        public static class Priority
        {
            public const int
            DEFAULT = 10,
            MILITARY = 100,
            TECH = 200,
            FARM = 300,
            PRODUCTION_BUILDING = 400,
            DROPSITE = 500,
            HOUSING = 600,
            VILLAGER = 700,
            AGE_UP = 800;
        }

        private class ProductionTask
        {
            public readonly int Priority;
            public readonly bool Blocking;
            public bool IsTech => Technology != null;
            public bool IsBuilding => IsTech == false && UnitType.IsBuilding;
            public int FoodCost => IsTech ? Technology.FoodCost : UnitType.FoodCost;
            public int WoodCost => IsTech ? Technology.WoodCost : UnitType.WoodCost;
            public int GoldCost => IsTech ? Technology.GoldCost : UnitType.GoldCost;
            public int StoneCost => IsTech ? Technology.StoneCost : UnitType.StoneCost;

            private readonly Technology Technology;
            private readonly UnitType UnitType;
            private readonly List<Tile> Tiles;
            private readonly int MaxCount;
            private readonly int MaxPending;

            public ProductionTask(Technology technology, int priority, bool blocking)
            {
                Priority = priority;
                Blocking = blocking;
                Technology = technology;
                UnitType = null;
                MaxCount = int.MaxValue;
                MaxPending = int.MaxValue;
            }

            public ProductionTask(UnitType type, List<Tile> tiles, int max_count, int max_pending, int priority, bool blocking)
            {
                // tiles = null for training
                Priority = priority;
                Blocking = blocking;
                Technology = null;
                UnitType = type;
                MaxCount = max_count;
                MaxPending = max_pending;
                Tiles = tiles;
            }

            public void Perform(Unary unary)
            {
                if (IsTech)
                {
                    unary.Log.Info($"Researching {Technology.Id}");
                    Technology.Research();
                }
                else if (Tiles != null && Tiles.Count > 0)
                {
                    unary.Log.Info($"Building {UnitType.Id} at {Tiles.Count} positions, {UnitType.Pending} pending");
                    UnitType.Build(Tiles, MaxCount, MaxPending);
                    ObjectPool.Add(Tiles);
                }
                else
                {
                    unary.Log.Info($"Training {UnitType.Id}");
                    UnitType.Train(MaxCount, MaxPending);
                }
            }
        }

        private readonly List<ProductionTask> ProductionTasks = new();

        public ProductionManager(Unary unary) : base(unary)
        {

        }

        public void Research(Technology technology, int priority, bool blocking = true)
        {
            if (!technology.Started)
            {
                var task = new ProductionTask(technology, priority, blocking);
                ProductionTasks.Add(task);
            }
        }

        public void Build(UnitType type, IEnumerable<Tile> tiles, int max_count, int max_pending, int priority, bool blocking = true)
        {
            if (type.CountTotal < max_count && type.Pending <= max_pending)
            {
                List<Tile> t = null;

                if (tiles != null)
                {
                    if (Unary.GameState.MyPlayer.CivilianPopulation == 0)
                    {
                        return;
                    }

                    t = ObjectPool.Get(() => new List<Tile>(), x => x.Clear());
                    t.AddRange(tiles.Take(100));

                    if (t.Count == 0)
                    {
                        throw new ArgumentOutOfRangeException(nameof(tiles), "Need at least 1 tile");
                    }
                }

                var task = new ProductionTask(type, t, max_count, max_pending, priority, blocking);
                ProductionTasks.Add(task);
            }
        }

        public void Train(UnitType type, int max_count, int max_pending, int priority, bool blocking = true)
        {
            if (type.CountTotal < max_count && type.Pending <= max_pending)
            {
                var task = new ProductionTask(type, null, max_count, max_pending, priority, blocking);
                ProductionTasks.Add(task);
            }
        }

        protected internal override void Update()
        {
            var researching = false;

            var remaining_wood = Unary.GameState.MyPlayer.GetFact(FactId.WOOD_AMOUNT);
            var remaining_food = Unary.GameState.MyPlayer.GetFact(FactId.FOOD_AMOUNT);
            var remaining_gold = Unary.GameState.MyPlayer.GetFact(FactId.GOLD_AMOUNT);
            var remaining_stone = Unary.GameState.MyPlayer.GetFact(FactId.STONE_AMOUNT);

            ProductionTasks.Sort((a, b) => b.Priority.CompareTo(a.Priority));

            foreach (var task in ProductionTasks)
            {
                var can_afford = true;
                if (task.WoodCost > 0 && task.WoodCost > remaining_wood)
                {
                    can_afford = false;
                }
                else if (task.FoodCost > 0 && task.FoodCost > remaining_food)
                {
                    can_afford = false;
                }
                else if (task.GoldCost > 0 && task.GoldCost > remaining_gold)
                {
                    can_afford = false;
                }
                else if (task.StoneCost > 0 && task.StoneCost > remaining_stone)
                {
                    can_afford = false;
                }

                var deduct = true;
                if (can_afford == false && task.Blocking == false)
                {
                    deduct = false;
                }

                if (can_afford)
                {
                    task.Perform(Unary);

                    if (task.IsTech)
                    {
                        researching = true;
                    }
                }

                if (deduct)
                {
                    remaining_wood -= task.WoodCost;
                    remaining_food -= task.FoodCost;
                    remaining_gold -= task.GoldCost;
                    remaining_stone -= task.StoneCost;
                }
            }

            ProductionTasks.Clear();

            if (researching)
            {
                Unary.GameState.SetStrategicNumber(StrategicNumber.ENABLE_TRAINING_QUEUE, 0);
            }
            else
            {
                Unary.GameState.SetStrategicNumber(StrategicNumber.ENABLE_TRAINING_QUEUE, 1);
            }
        }
    }
}
