﻿using AoE2Lib;
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

        public readonly Settings Settings;
        public Mod Mod { get; private set; }
        public TownManager TownManager { get; private set; }
        public ProductionManager ProductionManager { get; private set; }
        public OldMapManager OldMapManager { get; private set; }
        public DiplomacyManager DiplomacyManager { get; private set; }
        public OldBuildingManager OldBuildingManager { get; private set; }
        public StrategyManager StrategyManager { get; private set; }
        public OldEconomyManager OldEconomyManager { get; private set; }
        public OldMilitaryManager OldMilitaryManager { get; private set; }
        public UnitsManager UnitsManager { get; private set; }
        public OldProductionManager OldProductionManager { get; private set; }

        private readonly List<Command> Commands = new();
        private bool ChattedOK { get; set; } = false;

        public Unary(Settings settings) : base()
        {
            Settings = settings;
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
            TownManager = new TownManager(this);
            ProductionManager = new ProductionManager(this);
            OldMapManager = new OldMapManager(this);
            DiplomacyManager = new DiplomacyManager(this);
            OldBuildingManager = new OldBuildingManager(this);
            StrategyManager = new StrategyManager(this);
            OldEconomyManager = new OldEconomyManager(this);
            OldMilitaryManager = new OldMilitaryManager(this);
            UnitsManager = new UnitsManager(this);
            OldProductionManager = new OldProductionManager(this);
            ChattedOK = false;
        }

        protected override IEnumerable<Command> Tick()
        {
            Commands.Clear();

            var sw = new Stopwatch();

            sw.Start();
            TownManager.Update();
            Log.Info($"Town Manager took {sw.ElapsedMilliseconds} ms");

            sw.Restart();
            ProductionManager.Update();
            Log.Info($"Production Manager took {sw.ElapsedMilliseconds} ms");

            sw.Restart();
            OldMapManager.Update();
            Log.Info($"Old Map Manager took {sw.ElapsedMilliseconds} ms");

            sw.Restart();
            DiplomacyManager.Update();
            Log.Info($"Diplomacy Manager took {sw.ElapsedMilliseconds} ms");

            sw.Restart();
            OldBuildingManager.Update();
            Log.Info($"Old Building Manager took {sw.ElapsedMilliseconds} ms");

            sw.Restart();
            StrategyManager.Update();
            Log.Info($"Strategy Manager took {sw.ElapsedMilliseconds} ms");

            sw.Restart();
            OldEconomyManager.Update();
            Log.Info($"Old Economy Manager took {sw.ElapsedMilliseconds} ms");

            sw.Restart();
            OldMilitaryManager.Update();
            Log.Info($"Old Military Manager took {sw.ElapsedMilliseconds} ms");

            sw.Restart();
            UnitsManager.Update();
            Log.Info($"Units Manager took {sw.ElapsedMilliseconds} ms");

            sw.Restart();
            OldProductionManager.Update();
            Log.Info($"Old Production Manager took {sw.ElapsedMilliseconds} ms");

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
