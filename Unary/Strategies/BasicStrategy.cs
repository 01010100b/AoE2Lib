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
            bot.TrainModule.Train(bot.Mod.Villager);

            var me = bot.GameState.Players[bot.GameState.Player];

            if (me.HousingHeadroom < 5 && me.PopulationHeadroom > 0)
            {
                bot.BuildModule.Build(bot.Mod.House, bot, 100, 2);
            }

            var lumbercamps = bot.GameState.Units.Values.Where(u => u.PlayerNumber == me.PlayerNumber && u.TypeId == bot.Mod.LumberCamp.Id).ToList();
            var req = 1;
            if (me.CivilianPopulation > 10)
            {
                req = me.CivilianPopulation / 10;
            }

            bot.BuildModule.Build(bot.Mod.LumberCamp, bot, req, 1);
        }
    }
}
