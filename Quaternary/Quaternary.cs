using AoE2Lib.Bots;
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
            Log("updating");
            //throw new NotImplementedException();
        }
    }
}
