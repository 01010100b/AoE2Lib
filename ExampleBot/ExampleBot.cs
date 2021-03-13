using AoE2Lib.Bots;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExampleBot
{
    class ExampleBot : Bot
    {
        public override string Name => "ExampleBot";

        protected override IEnumerable<Command> Update()
        {
            return Enumerable.Empty<Command>();
        }
    }
}
