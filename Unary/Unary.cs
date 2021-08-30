using AoE2Lib;
using AoE2Lib.Bots;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unary.Managers;

namespace Unary
{
    class Unary : Bot
    {
        public readonly StrategyManager StrategyManager;
        public readonly EconomyManager EconomyManager;
        public readonly BuildingManager BuildingManager;

        public Unary() : base()
        {
            StrategyManager = new StrategyManager(this);
            EconomyManager = new EconomyManager(this);
            BuildingManager = new BuildingManager(this);
        }

        protected override IEnumerable<Command> Update()
        {
            StrategyManager.Update();
            EconomyManager.Update();
            BuildingManager.Update();

            foreach (var op in Operation.GetOperations(this))
            {
                op.UpdateInternal();
            }

            yield break;
        }
    }
}
