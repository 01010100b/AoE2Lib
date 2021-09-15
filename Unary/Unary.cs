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
        public Mod Mod { get; private set; }
        public StrategyManager StrategyManager { get; private set; }
        public EconomyManager EconomyManager { get; private set; }
        public BuildingManager BuildingManager { get; private set; }

        private bool ChattedOK { get; set; } = false;

        public Unary() : base()
        {
            NewGame();
        }

        protected override void Stopped()
        {
            Operation.ClearOperations(this);
        }

        protected override void NewGame()
        {
            Mod = new Mod();

            StrategyManager = new StrategyManager(this);
            EconomyManager = new EconomyManager(this);
            BuildingManager = new BuildingManager(this);

            ChattedOK = false;

            Operation.ClearOperations(this);
        }

        protected override IEnumerable<Command> Tick()
        {
            var sw = new Stopwatch();
            
            sw.Start();
            StrategyManager.Update();
            Log.Info($"Strategy Manager took {sw.ElapsedMilliseconds} ms");
            
            sw.Restart();
            EconomyManager.Update();
            Log.Info($"Economy Manager took {sw.ElapsedMilliseconds} ms");
            
            sw.Restart();
            BuildingManager.Update();
            Log.Info($"Building Manager took {sw.ElapsedMilliseconds} ms");
            
            sw.Restart();
            foreach (var op in Operation.GetOperations(this))
            {
                op.UpdateInternal();
            }
            Log.Info($"Operations took {sw.ElapsedMilliseconds} ms");

            sw.Stop();

            if (ChattedOK == false && GameState.GameTime.TotalSeconds >= 10 + PlayerNumber)
            {
                var command = new Command();
                command.Add(new ChatToAll() { InTextString = $"Unary {PlayerNumber} working OK!" });
                ChattedOK = true;

                yield return command;
            }

            yield break;
        }
    }
}
