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

        private readonly List<Command> Commands = new List<Command>();

        public Unary() : base()
        {
            StrategyManager = new StrategyManager(this);
            EconomyManager = new EconomyManager(this);
            BuildingManager = new BuildingManager(this);
        }

        internal void ExecuteCommand(Command command)
        {
            Commands.Add(command);
        }

        protected override IEnumerable<Command> Update()
        {
            Commands.Clear();

            StrategyManager.Update();
            EconomyManager.Update();
            BuildingManager.Update();

            return Commands;
        }
    }
}
