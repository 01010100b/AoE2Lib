using AoE2Lib;
using AoE2Lib.Bots.GameElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unary.Managers
{
    internal class ProductionManager : Manager
    {
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
            //throw new NotImplementedException();
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
