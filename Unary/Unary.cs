using AoE2Lib;
using AoE2Lib.Bots;
using Protos.Expert.Action;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unary.Behaviours;
using Unary.Managers;
using YTY.AocDatLib;

namespace Unary
{
    internal class Unary : Bot
    {
        public const int UNARY_ID = 21453;

        public override int Id => UNARY_ID;
        public readonly Settings Settings;
        public Mod Mod { get; private set; }
        public StrategyManager StrategyManager { get; private set; }
        public DiplomacyManager DiplomacyManager { get; private set; }
        public TownManager TownManager { get; private set; }
        public SitRepManager SitRepManager { get; private set; }
        public UnitsManager UnitsManager { get; private set; }
        public ResourcesManager ResourcesManager { get; private set; }

        public OldMapManager OldMapManager { get; private set; }
        public OldBuildingManager OldBuildingManager { get; private set; }
        public OldEconomyManager OldEconomyManager { get; private set; }
        public OldMilitaryManager OldMilitaryManager { get; private set; }
        public OldUnitsManager OldUnitsManager { get; private set; }
        public OldProductionManager OldProductionManager { get; private set; }

        private readonly List<Command> Commands = new();
        private readonly Dictionary<Func<object>, object> Cache = new();
        private bool ChattedOK { get; set; } = false;
        

        public Unary(Settings settings) : base()
        {
            Settings = settings;
        }

        public void ExecuteCommand(Command command) => Commands.Add(command);

        public bool ShouldRareTick(object obj, int rate) => obj.GetHashCode() % rate == GameState.Tick % rate;

        public T GetCached<T>(Func<T> func) where T : class
        {
            if (!Cache.ContainsKey(func))
            {
                var result = func();
                
                Cache.Add(func, result);
            }

            return (T)Cache[func];
        }

        protected override void Stopped()
        {

        }

        protected override void NewGame()
        {
            //Log.Level = AoE2Lib.Log.LEVEL_INFO;

            DatFile datfile = null;
            if (DatFilePath != null)
            {
                datfile = new DatFile();
                datfile.Load(DatFilePath);
            }

            Mod = new(datfile);

            StrategyManager = new(this);
            DiplomacyManager = new(this);
            TownManager = new (this);
            SitRepManager = new(this);
            UnitsManager = new(this);
            ResourcesManager = new(this);

            OldMapManager = new (this);
            OldBuildingManager = new (this);
            OldEconomyManager = new (this);
            OldMilitaryManager = new (this);
            OldUnitsManager = new (this);
            OldProductionManager = new (this);

            ChattedOK = false;
        }

        protected override IEnumerable<Command> Tick()
        {
            Commands.Clear();
            Cache.Clear();

            var sw = new Stopwatch();

            sw.Restart();
            StrategyManager.Update();
            Log.Info($"Strategy Manager took {sw.ElapsedMilliseconds} ms");

            sw.Restart();
            DiplomacyManager.Update();
            Log.Info($"Diplomacy Manager took {sw.ElapsedMilliseconds} ms");

            sw.Restart();
            TownManager.Update();
            Log.Info($"Town Manager took {sw.ElapsedMilliseconds} ms");

            sw.Restart();
            SitRepManager.Update();
            Log.Info($"SitRep Manager took {sw.ElapsedMilliseconds} ms");

            sw.Restart();
            UnitsManager.Update();
            Log.Info($"Units Manager took {sw.ElapsedMilliseconds} ms");

            sw.Restart();
            ResourcesManager.Update();
            Log.Info($"Resources Manager took {sw.ElapsedMilliseconds} ms");

            // old
            /*
            sw.Restart();
            OldMapManager.Update();
            Log.Info($"Old Map Manager took {sw.ElapsedMilliseconds} ms");

            sw.Restart();
            OldBuildingManager.Update();
            Log.Info($"Old Building Manager took {sw.ElapsedMilliseconds} ms");

            sw.Restart();
            OldEconomyManager.Update();
            Log.Info($"Old Economy Manager took {sw.ElapsedMilliseconds} ms");

            sw.Restart();
            OldMilitaryManager.Update();
            Log.Info($"Old Military Manager took {sw.ElapsedMilliseconds} ms");
            
            sw.Restart();
            OldUnitsManager.Update();
            Log.Info($"Old Units Manager took {sw.ElapsedMilliseconds} ms");
            
            sw.Restart();
            OldProductionManager.Update();
            Log.Info($"Old Production Manager took {sw.ElapsedMilliseconds} ms");
            */
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
