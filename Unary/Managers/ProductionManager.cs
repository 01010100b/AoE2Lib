using AoE2Lib;
using AoE2Lib.Bots;
using AoE2Lib.Utils;
using Protos.Expert.Action;
using System;
using System.Collections.Generic;
using System.Text;

namespace Unary.Managers
{
    class ProductionManager : Manager
    {
        private class ProductionTask
        {
            public int Priority { get; set; }
            public bool Blocking { get; set; }
            public int WoodCost { get; set; }
            public int FoodCost { get; set; }
            public int GoldCost { get; set; }
            public int StoneCost { get; set; }
            public int Id { get; set; }
            public int MaxCount { get; set; }
            public int MaxPending { get; set; }
            public bool IsTech { get; set; }
            public bool IsBuilding { get; set; }
            public List<Position> BuildPositions = new List<Position>();

        }

        private readonly List<ProductionTask> ProductionTasks = new List<ProductionTask>();

        public ProductionManager(Unary unary) : base(unary)
        {

        }

        public void Research(int id, int priority = 10, bool blocking = false)
        {
            var research = Unary.ResearchModule;
            research.Add(id);

            var tech = research.Researches[id];

            if (tech.Updated == false || tech.State != ResearchState.AVAILABLE)
            {
                return;
            }

            var prod = new ProductionTask()
            {
                Priority = priority,
                Blocking = blocking,
                WoodCost = tech.WoodCost,
                FoodCost = tech.FoodCost,
                GoldCost = tech.GoldCost,
                StoneCost = tech.StoneCost,
                Id = id,
                IsTech = true,
                IsBuilding = false
            };

            ProductionTasks.Add(prod);
        }

        public void Train(int id, int max_count = 10000, int max_pending = 10000, int priority = 10, bool blocking = false)
        {
            var units = Unary.UnitsModule;
            units.AddUnitType(id);

            var type = units.UnitTypes[id];

            if (type.Updated == false || type.IsAvailable == false)
            {
                return;
            }

            var prod = new ProductionTask()
            {
                Priority = priority,
                Blocking = blocking,
                WoodCost = type.WoodCost,
                FoodCost = type.FoodCost,
                GoldCost = type.GoldCost,
                StoneCost = type.StoneCost,
                Id = id,
                MaxCount = max_count,
                MaxPending = max_pending,
                IsTech = false,
                IsBuilding = false
            };

            ProductionTasks.Add(prod);
        }

        public void Build(int id, Position position, int max_count = 10000, int max_pending = 10000, int priority = 10, bool blocking = false)
        {
            Build(id, new[] { position }, max_count, max_pending, priority, blocking);
        }

        public void Build(int id, IEnumerable<Position> positions, int max_count = 10000, int max_pending = 10000, int priority = 10, bool blocking = false)
        {
            var units = Unary.UnitsModule;
            units.AddUnitType(id);

            var type = units.UnitTypes[id];

            if (type.Updated == false || type.IsAvailable == false)
            {
                return;
            }

            var prod = new ProductionTask()
            {
                Priority = priority,
                Blocking = blocking,
                WoodCost = type.WoodCost,
                FoodCost = type.FoodCost,
                GoldCost = type.GoldCost,
                StoneCost = type.StoneCost,
                Id = id,
                MaxCount = max_count,
                MaxPending = max_pending,
                IsTech = false,
                IsBuilding = true
            };

            prod.BuildPositions.AddRange(positions);

            ProductionTasks.Add(prod);
        }

        public override void Update()
        {
            var info = Unary.InfoModule;
            var remaining_wood = info.WoodAmount;
            var remaining_food = info.FoodAmount;
            var remaining_gold = info.GoldAmount;
            var remaining_stone = info.StoneAmount;
            var research = Unary.ResearchModule;
            var units = Unary.UnitsModule;

            ProductionTasks.Sort((a, b) => b.Priority.CompareTo(a.Priority));

            foreach (var prod in ProductionTasks)
            {
                var can_afford = true;
                if (prod.WoodCost > 0 && prod.WoodCost > remaining_wood)
                {
                    can_afford = false;
                }
                else if (prod.FoodCost > 0 && prod.FoodCost > remaining_food)
                {
                    can_afford = false;
                }
                else if (prod.GoldCost > 0 && prod.GoldCost > remaining_gold)
                {
                    can_afford = false;
                }
                else if (prod.StoneCost > 0 && prod.StoneCost > remaining_stone)
                {
                    can_afford = false;
                }

                var deduct = true;
                if (can_afford == false && prod.Blocking == false)
                {
                    deduct = false;
                }

                if (can_afford)
                {
                    if (prod.IsTech)
                    {
                        research.Research(prod.Id);
                    }
                    else if (prod.IsBuilding)
                    {
                        Unary.Log.Debug($"building {prod.Id} at {prod.BuildPositions.Count} positions count {prod.MaxCount} pending {prod.MaxPending}");
                        units.Build(prod.Id, prod.BuildPositions, prod.MaxCount, prod.MaxPending);
                    }
                    else
                    {
                        units.Train(prod.Id, prod.MaxCount, prod.MaxPending);
                    }
                }

                if (deduct)
                {
                    remaining_wood -= prod.WoodCost;
                    remaining_food -= prod.FoodCost;
                    remaining_gold -= prod.GoldCost;
                    remaining_stone -= prod.StoneCost;
                }
            }

            ProductionTasks.Clear();
        }
    }
}
