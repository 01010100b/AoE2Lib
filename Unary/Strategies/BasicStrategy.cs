using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unary.Strategies
{
    public class BasicStrategy : Strategy
    {
        public override void Update(Bot bot)
        {

            Economy(bot);
            
        }

        private void Economy(Bot bot)
        {
            var me = bot.GameState.Players[bot.GameState.PlayerNumber];

            if (me.Population >= 0)
            {
                bot.BuildModule.MaxBuildRange = 20 + (me.Population / 10);
                bot.BuildModule.MaxMillRange = bot.BuildModule.MaxBuildRange + 10;
                bot.BuildModule.MaxLumberRange = bot.BuildModule.MaxBuildRange + 15;
            }

            bot.TrainModule.Train(bot.Mod.Villager);
            bot.ResearchModule.Research(bot.Mod.TechDefs[22]);

            if (me.HousingHeadroom < 5 && me.PopulationHeadroom > 0)
            {
                bot.BuildModule.Build(bot.Mod.House, bot, 100, 2);
            }

            var lumbercamps = 1;
            if (me.CivilianPopulation > 10)
            {
                lumbercamps = me.CivilianPopulation / 10;
            }
            if (bot.GameState.GameTime < TimeSpan.FromMinutes(2) || bot.GameState.Units.Values.Count(u => u.Class == UnitClass.Tree) < 10)
            {
                lumbercamps = 0;
            }

            bot.BuildModule.Build(bot.Mod.LumberCamp, bot, lumbercamps, 1);

            var mills = 1;
            if (me.CivilianPopulation > 10)
            {
                mills = me.CivilianPopulation / 7;
            }
            if (bot.GameState.GameTime < TimeSpan.FromMinutes(3))
            {
                mills = 0;
            }

            bot.BuildModule.Build(bot.Mod.Mill, bot, mills, 1);

            var farms = 0;
            if (bot.GameState.GameTime > TimeSpan.FromMinutes(8))
            {
                farms = Math.Max(1, me.CivilianPopulation / 2);
            }
            if (bot.GameState.GetObjectTypeCountTotal(bot.Mod.Mill.BaseId) == 0)
            {
                farms = 0;
            }

            bot.BuildModule.Build(bot.Mod.Farm, bot, farms, 3);
        }
    }
}
