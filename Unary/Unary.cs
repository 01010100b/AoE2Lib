using AoE2Lib;
using AoE2Lib.Bots;
using AoE2Lib.Bots.Modules;
using AoE2Lib.Utils;
using Protos.Expert.Action;
using Protos.Expert.Fact;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unary.Managers;
using Unary.Operations;
using Unary.Utils;

namespace Unary
{
    class Unary : Bot
    {
        public readonly Mod Mod;

        public readonly StrategyManager StrategyManager;
        public readonly EconomyManager EconomyManager;
        public readonly BuildingManager BuildingManager;
        public readonly OperationsManager OperationsManager;

        private readonly List<Command> Commands = new List<Command>();

        public Unary() : base()
        {
            Mod = new Mod();
            Mod.Load();

            StrategyManager = new StrategyManager(this);
            EconomyManager = new EconomyManager(this);
            BuildingManager = new BuildingManager(this);
            OperationsManager = new OperationsManager(this);
        }

        internal void ExecuteCommand(Command command)
        {
            Commands.Add(command);
        }

        protected override IEnumerable<Command> Update()
        {
            Commands.Clear();

            UpdateManagers();

            return Commands;
        }

        private void UpdateManagers()
        {
            StrategyManager.Update();
            EconomyManager.Update();
            BuildingManager.Update();
            OperationsManager.Update();
        }
    }
}
