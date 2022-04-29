using AoE2Lib.Bots;
using System;
using System.Collections.Generic;
using System.Text;

namespace Unary.Managers
{
    abstract class Manager
    {
        public readonly Unary Unary;

        public Manager(Unary unary)
        {
            Unary = unary;
        }

        internal abstract void Update();
    }
}
