using AoE2Lib;
using AoE2Lib.Bots;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        protected internal abstract void Update();

        protected void Run(IEnumerable<Action> actions)
        {
            var sw = ObjectPool.Get(() => new Stopwatch(), x => x.Reset());

            foreach (var action in actions)
            {
                sw.Restart();
                action();
                Unary.Log.Debug($"{GetType().Name}.{action.Method.Name} took {sw.Elapsed.TotalMilliseconds:N2} ms");
            }

            ObjectPool.Add(sw);
        }
    }
}
