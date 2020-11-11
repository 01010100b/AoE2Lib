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
            //throw new NotImplementedException();
        }
    }
}
