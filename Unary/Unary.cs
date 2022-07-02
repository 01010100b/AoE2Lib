using AoE2Lib;
using AoE2Lib.Bots;
using Protos.Expert.Action;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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
        public ProductionManager ProductionManager { get; private set; }

        private readonly Dictionary<Func<object>, object> Cache = new();
        private readonly List<Command> Commands = new();
        private bool ChattedOK { get; set; } = false;
        

        public Unary(Settings settings) : base()
        {
            Settings = settings;
        }

        public Unary() : this(Program.DefaultSettings) { }

        public T GetCached<T>(Func<T> func) where T : class
        {
            if (!Cache.ContainsKey(func))
            {
                var res = func();

                Cache.Add(func, res);
            }

            return (T)Cache[func];
        }

        public void ExecuteCommand(Command command) => Commands.Add(command);

        public bool ShouldRareTick(object obj, int rate) => obj.GetHashCode() % rate == GameState.Tick % rate;

        protected override void Stopped()
        {
            Mod = null;
            StrategyManager = null;
            DiplomacyManager = null;
            TownManager = null;
            SitRepManager = null;
            UnitsManager = null;
            ProductionManager = null;

            Cache.Clear();
            Commands.Clear();
            ChattedOK = false;
        }

        protected override void NewGame()
        {
            //Log.Level = AoE2Lib.Log.LEVEL_INFO;

            DatFile datfile = null;
            if (DatFilePath != null)
            {
                lock (DatFilePath)
                {
                    var sw = new Stopwatch();
                    sw.Start();

                    while (sw.Elapsed < TimeSpan.FromMinutes(1))
                    {
                        try
                        {
                            datfile = new DatFile();
                            datfile.Load(DatFilePath);

                            Log.Info($"Unary loaded dat file {DatFilePath}");

                            break;
                        }
                        catch (Exception)
                        {
                            Thread.Sleep(500);
                        }
                    }
                }
            }

            Mod = new(datfile);

            StrategyManager = new(this);
            DiplomacyManager = new(this);
            TownManager = new (this);
            SitRepManager = new(this);
            UnitsManager = new(this);
            ProductionManager = new(this);

            Cache.Clear();
            Commands.Clear();
            ChattedOK = false;
        }

        protected override IEnumerable<Command> Tick()
        {
            foreach (var obj in Cache.Values)
            {
                ObjectPool.Add(obj);
            }

            Cache.Clear();
            Commands.Clear();

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
            ProductionManager.Update();
            Log.Info($"Resources Manager took {sw.ElapsedMilliseconds} ms");

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
