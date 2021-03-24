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

        public readonly MapManager MapManager;
        public readonly StrategyManager StrategyManager;
        public readonly EconomyManager EconomyManager;
        public readonly ProductionManager ProductionManager;
        public readonly OperationsManager OperationsManager;

        private readonly List<Command> Commands = new List<Command>();

        public Unary() : base()
        {
            Mod = new Mod();
            Mod.Load();

            MapManager = new MapManager(this);
            StrategyManager = new StrategyManager(this);
            EconomyManager = new EconomyManager(this);
            ProductionManager = new ProductionManager(this);
            OperationsManager = new OperationsManager(this);
        }

        public void ExecuteCommand(Command command)
        {
            Commands.Add(command);
        }

        protected override IEnumerable<Command> Update()
        {
            Commands.Clear();

            var pos = InfoModule.MyPosition;
            pos += new Position(Rng.Next(-10, 10), Rng.Next(-10, 10));
            UnitsModule.Build(70, pos);

            UpdateManagers();

            Log.Info($"Free units: {OperationsManager.FreeUnits.Count()}");

            var info = InfoModule;
            var map = MapModule;
            var units = UnitsModule;

            Log.Info($"Tick {Tick} Game time {info.GameTime}");
            Log.Info($"Wood {info.WoodAmount} Food {info.FoodAmount} Gold {info.GoldAmount} Stone {info.StoneAmount}");
            Log.Info($"Explored {map.Tiles.Count(t => t.Explored):N0} tiles of {map.Width * map.Height:N0}");
            Log.Info($"I have {units.Units.Values.Count(u => u.PlayerNumber == PlayerNumber && u.Targetable):N0} units");
            Log.Info($"Gaia has {units.Units.Values.Count(u => u.PlayerNumber == 0 && u.Targetable):N0} units");

            return Commands;
        }

        private void UpdateManagers()
        {
            MapManager.Update();
            StrategyManager.Update();
            EconomyManager.Update();
            ProductionManager.Update();
            OperationsManager.Update();
        }
    }
}
