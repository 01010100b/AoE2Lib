using System;
using System.Collections.Generic;
using System.Text;

namespace Unary
{
    abstract class Manager
    {
        public readonly Unary Unary;

        public Manager(Unary unary)
        {
            Unary = unary;
        }

        public abstract void Update();
    }
}
