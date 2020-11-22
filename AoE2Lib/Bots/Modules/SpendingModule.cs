using AoE2Lib.Utils;
using PeNet.PatternMatching;
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
            public int WoodCost { get; set; } = 0;
            public int FoodCost { get; set; } = 0;
            public int GoldCost { get; set; } = 0;
            public int StoneCost { get; set; } = 0;
            public int Cost => WoodCost + FoodCost + GoldCost + StoneCost;
        }

        private readonly List<SpendingCommand> Commands = new List<SpendingCommand>();

        public void Add(SpendingCommand command)
        {
            Commands.Add(command);
        }

        protected override IEnumerable<Command> RequestUpdate()
        {
            var info = Bot.GetModule<InfoModule>();
            var wood = info.WoodAmount;
            var food = info.FoodAmount;
            var gold = info.GoldAmount;
            var stone = info.StoneAmount;
            var priority = 0;

            var wood_shortage = false;
            var food_shortage = false;
            var gold_shortage = false;
            var stone_shortage = false;

            foreach (var command in Commands)
            {
                if (command.Cost <= 0)
                {
                    yield return command;
                }
                else
                {
                    wood -= command.WoodCost;
                    food -= command.FoodCost;
                    gold -= command.GoldCost;
                    stone -= command.StoneCost;

                    var spend = true;
                    if (wood < 0 || food < 0 || gold < 0 || stone < 0)
                    {
                        spend = false;

                        if (wood < 0)
                        {
                            wood_shortage = true;
                        }
                        if (food < 0)
                        {
                            food_shortage = true;
                        }
                        if (gold < 0)
                        {
                            gold_shortage = true;
                        }
                        if (stone < 0)
                        {
                            stone_shortage = true;
                        }

                        priority = Math.Max(priority, command.Priority + 1);
                    }

                    if (command.Priority < priority)
                    {
                        if (command.WoodCost > 0 && wood_shortage)
                        {
                            spend = false;
                        }
                        else if (command.FoodCost > 0 && food_shortage)
                        {
                            spend = false;
                        }
                        else if (command.GoldCost > 0 && gold_shortage)
                        {
                            spend = false;
                        }
                        else if (command.StoneCost > 0 && stone_shortage)
                        {
                            spend = false;
                        }
                    }

                    if (spend)
                    {
                        yield return command;
                    }
                    else
                    {
                        wood += command.WoodCost;
                        food += command.FoodCost;
                        gold += command.GoldCost;
                        stone += command.StoneCost;
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
