using AoE2Lib.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace AoE2Lib.Bots.Modules
{
    public class SpendingModule : Module
    {
        public class SpendingCommand : Command
        {
            public int Priority { get; set; } = 0;
            public int WoodAmount { get; set; } = 0;
            public int FoodAmount { get; set; } = 0;
            public int GoldAmount { get; set; } = 0;
            public int StoneAmount { get; set; } = 0;
            public int Cost => WoodAmount + FoodAmount + GoldAmount + StoneAmount;
        }

        public int WoodMin { get; set; } = 0;
        public int FoodMin { get; set; } = 0;
        public int GoldMin { get; set; } = 0;
        public int StoneMin { get; set; } = 0;

        private readonly List<SpendingCommand> Commands = new List<SpendingCommand>();

        public void Add(SpendingCommand command)
        {
            Commands.Add(command);
        }

        protected override IEnumerable<Command> RequestUpdate()
        {
            var info = Bot.GetModule<InfoModule>();
            var wood = info.WoodAmount - WoodMin;
            var food = info.FoodAmount - FoodMin;
            var gold = info.GoldAmount - GoldMin;
            var stone = info.StoneAmount - StoneMin;
            var priority = 0;

            foreach (var command in Commands)
            {
                if (command.Cost <= 0)
                {
                    yield return command;
                }
                else if (command.Priority >= priority)
                {
                    wood -= command.WoodAmount;
                    food -= command.FoodAmount;
                    gold -= command.GoldAmount;
                    stone -= command.StoneAmount;

                    if (wood >= 0 && food >= 0 && gold >= 0 && stone >= 0)
                    {
                        yield return command;
                    }
                    else
                    {
                        wood += command.WoodAmount;
                        food += command.FoodAmount;
                        gold += command.GoldAmount;
                        stone += command.StoneAmount;

                        priority = command.Priority + 1;
                    }
                }
            }

            Commands.Clear();
        }

        protected override void Update()
        {
            
        }
    }
}
