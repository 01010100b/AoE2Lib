using AoE2Lib;
using AoE2Lib.Bots.GameElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unary.Managers
{
    // resource management
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
                Priority = priority;
                Blocking = blocking;
                Technology = null;
                UnitType = type;
                Tiles = tiles;
                MaxCount = max_count;
                MaxPending = max_pending;
            }

            public void Perform(Unary unary, HashSet<Unit> excluded_trainsites)
            {
                if (IsTech)
                {
                    Technology.Research();
                }
                else if (UnitType.IsBuilding)
                {
                    var placements = unary.TownManager.GetSortedBuildingPlacements(UnitType, Tiles);
                    
                    if (placements.Count > 0)
                    {
                        UnitType.Build(placements.Take(100), MaxCount, MaxPending);
                    }
                }
                else
                {
                    var site_id = UnitType.TrainSite[ObjectData.BASE_TYPE];
                    var sites = unary.GameState.MyPlayer.Units
                        .Where(u => u[ObjectData.BASE_TYPE] == site_id && !excluded_trainsites.Contains(u))
                        .Where(u => u[ObjectData.PROGRESS_TYPE] == 0 || u[ObjectData.PROGRESS_TYPE] == 102)
                        .ToList();

                    if (sites.Count > 0)
                    {
                        var site = sites[unary.Rng.Next(sites.Count)];
                        site.Train(UnitType, MaxCount, MaxPending);
                    }
                }
            }
        }

        private readonly List<ProductionTask> ProductionTasks = new List<ProductionTask>();

        public ProductionManager(Unary unary) : base(unary)
        {

        }

        public void Research(Technology technology, int priority, bool blocking = true)
        {
            throw new NotImplementedException();
        }

        public void Build(UnitType type, List<Tile> tiles, int max_count, int max_pending, int priority, bool blocking = true)
        {
            throw new NotImplementedException();
        }

        internal override void Update()
        {
            var excluded_trainsites = new HashSet<Unit>();
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
                else if (task.StoneCost > 0 && task.StoneCost > remaining_stone - 1) // keep 1 stone for TC repair
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
                    task.Perform(Unary, excluded_trainsites);
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
        }
    }
}
