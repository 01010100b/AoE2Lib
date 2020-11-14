using AoE2Lib.Bots;
using AoE2Lib.Bots.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quaternary
{
    class Quaternary : Bot
    {
        public override string Name => "Quaternary";

        public override int Id => 27432;

        protected override IEnumerable<Command> RequestUpdate()
        {
            return Enumerable.Empty<Command>();
            //throw new NotImplementedException();
        }

        protected override void Update()
        {
            var players = GetModule<PlayersModule>().Players.Count;
            Log($"number of players {players}");
            //throw new NotImplementedException();
        }
    }
}
