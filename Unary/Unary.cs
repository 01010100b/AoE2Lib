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
        public StrategyManager StrategyManager { get; private set; }
        public EconomyManager EconomyManager { get; private set; }
        public BuildingManager BuildingManager { get; private set; }

        public Unary() : base()
        {
            NewGame();
        }

        protected override void NewGame()
        {
            StrategyManager = new StrategyManager(this);
            EconomyManager = new EconomyManager(this);
            BuildingManager = new BuildingManager(this);

            Operation.ClearOperations(this);
        }

        protected override IEnumerable<Command> Tick()
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
