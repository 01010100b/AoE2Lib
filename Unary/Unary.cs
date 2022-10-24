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
        public UnitsManager UnitsManager => Managers.OfType<UnitsManager>().Single();
        public MapManager MapManager => Managers.OfType<MapManager>().Single();
        public TownManager TownManager => Managers.OfType<TownManager>().Single();
        public EconomyManager EconomyManager => Managers.OfType<EconomyManager>().Single();
        public MilitaryManager MilitaryManager => Managers.OfType<MilitaryManager>().Single();
        public StrategyManager StrategyManager { get; private set; }
        public DiplomacyManager DiplomacyManager { get; private set; }
        
        public ProductionManager ProductionManager { get; private set; }

        private readonly List<Manager> Managers = new();
        private readonly Dictionary<Func<object>, object> Cache = new();
        private readonly List<Command> Commands = new();
        private bool ChattedOK { get; set; } = false;
        

        public Unary(Settings settings) : base()
        {
            Settings = settings;
        }

        public Unary() : this(Program.DefaultSettings) { }

        public T GetManager<T>() where T : Manager => Managers.OfType<T>().Cast<T>().Single();

        public void ExecuteCommand(Command command) => Commands.Add(command);

        public bool ShouldRareTick(object obj, int rate) => obj.GetHashCode() % rate == GameState.Tick % rate;

        public T GetCached<T>(Func<T> func) where T : class
        {
            if (!Cache.ContainsKey(func))
            {
                Cache.Add(func, func());
            }

            return (T)Cache[func];
        }

        protected override void Stopped()
        {
            Mod = null;

            Managers.Clear();

            StrategyManager = null;
            DiplomacyManager = null;
            ProductionManager = null;

            Cache.Clear();
            Commands.Clear();
            ChattedOK = false;
        }

        protected override void NewGame()
        {
            //Log.Level = AoE2Lib.Log.LEVEL_INFO;

            Managers.Clear();
            Cache.Clear();
            Commands.Clear();
            ChattedOK = false;
            Mod = null;

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
                            var datfile = new DatFile();
                            datfile.Load(DatFilePath);
                            Mod = new(datfile);

                            Log.Info($"Unary loaded dat file {DatFilePath}");

                            break;
                        }
                        catch (IOException)
                        {
                            Thread.Sleep(500);
                        }
                    }
                }
            }
            
            Managers.Add(new UnitsManager(this));
            Managers.Add(new MapManager(this));
            Managers.Add(new TownManager(this));
            Managers.Add(new EconomyManager(this));
            Managers.Add(new MilitaryManager(this));
            StrategyManager = new(this);
            DiplomacyManager = new(this);
            
            ProductionManager = new(this);

            
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

            foreach (var manager in Managers)
            {
                sw.Restart();
                manager.Update();
                Log.Info($"{manager.GetType().Name} took {sw.Elapsed.TotalMilliseconds:N2} ms");
            }

            sw.Restart();
            StrategyManager.Update();
            Log.Info($"Strategy Manager took {sw.ElapsedMilliseconds} ms");

            sw.Restart();
            DiplomacyManager.Update();
            Log.Info($"Diplomacy Manager took {sw.ElapsedMilliseconds} ms");

            sw.Restart();
            ProductionManager.Update();
            Log.Info($"Production Manager took {sw.ElapsedMilliseconds} ms");

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

            foreach (var command in Test())
            {
                yield return command;
            }
        }

        protected override void Handle(Exception ex)
        {
            throw ex;
        }

        private IEnumerable<Command> Test()
        {
            var civ = GameState.MyPlayer.Civilization;

            foreach (var unit in Mod.GetAvailableUnits(civ))
            {
                if (GameState.TryGetUnitType(unit, out var type))
                {
                    if (type[ObjectData.CMDID] == (int)CmdId.VILLAGER)
                    {
                        Log.Debug($"Civ {civ} has villager {unit} {Mod.GetUnitName(civ, unit)}");
                    }
                }
            }

            yield break;
        }
    }
}
