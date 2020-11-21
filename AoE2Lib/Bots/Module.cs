using System;
using System.Collections.Generic;
using System.Text;

namespace AoE2Lib.Bots
{
    public abstract class Module
    {
        internal Bot BotInternal { set { Bot = value; } }
        protected Bot Bot { get; private set; }
        protected TypeOp TypeOp => Bot.TypeOp;
        protected MathOp MathOp => Bot.MathOp;

        internal IEnumerable<Command> RequestUpdateInternal()
        {
            return RequestUpdate();
        }

        internal void UpdateInternal()
        {
            Update();
        }

        protected abstract IEnumerable<Command> RequestUpdate();
        protected abstract void Update();
    }
}
