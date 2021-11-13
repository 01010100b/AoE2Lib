using AoE2Lib;
using AoE2Lib.Bots;
using Protos.Expert.Action;
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
        public const int UNARY_ID = 21453;

        public Mod Mod { get; private set; }
        public MapManager MapManager { get; private set; }
        public DiplomacyManager DiplomacyManager { get; private set; }
        public BuildingManager BuildingManager { get; private set; }
        public StrategyManager StrategyManager { get; private set; }
        public EconomyManager EconomyManager { get; private set; }
        public MilitaryManager MilitaryManager { get; private set; }
        public UnitsManager UnitsManager { get; private set; }
        public ProductionManager ProductionManager { get; private set; }

        private readonly List<Command> Commands = new();
        private bool ChattedOK { get; set; } = false;

        public Unary() : base()
        {
            
        }

        public void ExecuteCommand(Command command)
        {
            Commands.Add(command);
        }

        protected override void Stopped()
        {

        }

        protected override void NewGame()
        {
            Mod = new Mod();
            MapManager = new MapManager(this);
            DiplomacyManager = new DiplomacyManager(this);
            BuildingManager = new BuildingManager(this);
            StrategyManager = new StrategyManager(this);
            EconomyManager = new EconomyManager(this);
            MilitaryManager = new MilitaryManager(this);
            UnitsManager = new UnitsManager(this);
            ProductionManager = new ProductionManager(this);
            ChattedOK = false;
        }

        protected override IEnumerable<Command> Tick()
        {
            Commands.Clear();

            var sw = new Stopwatch();

            sw.Start();
            MapManager.Update();
            Log.Info($"Map Manager took {sw.ElapsedMilliseconds} ms");

            sw.Restart();
            DiplomacyManager.Update();
            Log.Info($"Diplomacy Manager took {sw.ElapsedMilliseconds} ms");

            sw.Restart();
            BuildingManager.Update();
            Log.Info($"Building Manager took {sw.ElapsedMilliseconds} ms");

            sw.Restart();
            StrategyManager.Update();
            Log.Info($"Strategy Manager took {sw.ElapsedMilliseconds} ms");

            sw.Restart();
            EconomyManager.Update();
            Log.Info($"Economy Manager took {sw.ElapsedMilliseconds} ms");

            sw.Restart();
            MilitaryManager.Update();
            Log.Info($"Military Manager took {sw.ElapsedMilliseconds} ms");

            sw.Restart();
            UnitsManager.Update();
            Log.Info($"Units Manager took {sw.ElapsedMilliseconds} ms");

            sw.Restart();
            ProductionManager.Update();
            Log.Info($"Production Manager took {sw.ElapsedMilliseconds} ms");

            sw.Stop();

            if (ChattedOK == false && GameState.GameTime.TotalSeconds >= 10 + PlayerNumber)
            {
                var command = new Command();
                command.Add(new ChatToAll() { InTextString = $"Unary {PlayerNumber} working OK!" });
                ChattedOK = true;

                yield return command;
            }

            foreach (var command in Commands)
            {
                yield return command;
            }
        }
    }
}
