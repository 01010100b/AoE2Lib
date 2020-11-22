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
            //Bot.Log.Debug("SPENDING MODULE");
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
                //Bot.Log.Debug($"command cost {command.Cost} priority {command.Priority}");
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
                            //Bot.Log.Debug("wood shortage");
                        }
                        if (food < 0)
                        {
                            food_shortage = true;
                            //Bot.Log.Debug("food shortage");
                        }
                        if (gold < 0)
                        {
                            gold_shortage = true;
                            //Bot.Log.Debug("gold shortage");
                        }
                        if (stone < 0)
                        {
                            stone_shortage = true;
                            //Bot.Log.Debug("stone shortage");
                        }

                        priority = Math.Max(priority, command.Priority + 1);
                        //Bot.Log.Debug($"new min priority {priority}");
                    }

                    if (spend && command.Priority < priority)
                    {
                        //Bot.Log.Debug($"command priority {command.Priority} below min priority {priority}");
                        if (command.WoodCost > 0 && wood_shortage)
                        {
                            spend = false;
                            //Bot.Log.Debug($"wood shortage fail cost {command.WoodCost}");
                        }
                        else if (command.FoodCost > 0 && food_shortage)
                        {
                            spend = false;
                            //Bot.Log.Debug($"food shortage fail cost {command.FoodCost}");
                        }
                        else if (command.GoldCost > 0 && gold_shortage)
                        {
                            spend = false;
                            //Bot.Log.Debug($"gold shortage fail cost {command.GoldCost}");
                        }
                        else if (command.StoneCost > 0 && stone_shortage)
                        {
                            spend = false;
                            //Bot.Log.Debug($"stone shortage fail cost {command.StoneCost}");
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
