using System;
using System.Collections.Generic;
using System.Text;

namespace AoE2Lib.Bots
{
    public abstract class Module
    {
        protected internal Bot Bot { get; internal set; }

        protected internal abstract void StartNewGame();
        protected internal abstract IEnumerable<Command> RequestUpdate();
        protected internal abstract void Update();
    }
}
