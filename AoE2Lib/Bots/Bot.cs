using AoE2Lib.Bots.GameElements;
using AoE2Lib.Bots.Modules;
using AoE2Lib.Utils;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Protos.Expert;
using Protos.Expert.Fact;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using static Protos.AIModuleAPI;
using static Protos.Expert.ExpertAPI;

namespace AoE2Lib.Bots
{
    public abstract class Bot
    {
        public const int SN_PENDING_PLACEMENT = 450;

        public virtual string Name { get { return GetType().Name; } }
        public GameVersion GameVersion { get; private set; }
        public string DatFile { get; private set; } // Only available on AoC
        public int PlayerNumber { get; private set; } = -1;
        public int Tick { get; private set; } = 0;
        public Log Log { get; private set; }
        public Random Rng { get; private set; }

        public GameState GameState { get; private set; }
        public Player MyPlayer => GetPlayer(PlayerNumber);

        public InfoModule InfoModule { get; private set; }
        public MapModule MapModule { get; private set; }
        public MicroModule MicroModule { get; private set; }

        private readonly List<ProductionTask> ProductionTasks = new List<ProductionTask>();
        private readonly List<Player> Players = new List<Player>();
        private readonly Dictionary<int, Technology> Technologies = new Dictionary<int, Technology>();
        private readonly Dictionary<int, UnitType> UnitTypes = new Dictionary<int, UnitType>();

        private Thread BotThread { get; set; } = null;
        private volatile bool Stopping = false;
        private readonly Dictionary<GameElement, Command> GameElementUpdates = new Dictionary<GameElement, Command>();

        public void Stop()
        {
            Stopping = true;

            BotThread?.Join();
            BotThread = null;

            Log?.Dispose();

            Stopping = false;
        }

        public Player GetPlayer(int player_number)
        {
            return Players[player_number];
        }

        public IEnumerable<Player> GetPlayers()
        {
            return Players.Where(p => p.PlayerNumber == 0 || p.IsValid);
        }

        public Technology GetTechnology(int id)
        {
            if (!Technologies.ContainsKey(id))
            {
                Technologies.Add(id, new Technology(this, id));
            }

            return Technologies[id];
        }

        public UnitType GetUnitType(int id)
        {
            if (!UnitTypes.ContainsKey(id))
            {
                UnitTypes.Add(id, new UnitType(this, id));
            }

            return UnitTypes[id];
        }

        public Unit GetUnit(int id)
        {
            return GameState.GetUnitById(id);
        }

        public bool GetResourceFound(Resource resource)
        {
            return InfoModule.GetResourceFound(resource);
        }

        public int GetDropsiteMinDistance(Resource resource)
        {
            return InfoModule.GetDropsiteMinDistance(resource);
        }

        public int GetStrategicNumber(StrategicNumber sn)
        {
            return InfoModule.GetStrategicNumber(sn);
        }

        public void SetStrategicNumber(StrategicNumber sn, int val)
        {
            InfoModule.SetStrategicNumber(sn, val);
        }

        internal void UpdateGameElement(GameElement element, Command command)
        {
            GameElementUpdates[element] = command;
        }

        internal void AddProductionTask(ProductionTask task)
        {
            ProductionTasks.Add(task);
        }

        protected abstract IEnumerable<Command> Update();

        private IEnumerable<Command> DoProduction()
        {
            var info = InfoModule;
            var remaining_wood = info.WoodAmount;
            var remaining_food = info.FoodAmount;
            var remaining_gold = info.GoldAmount;
            var remaining_stone = info.StoneAmount;

            ProductionTasks.Sort((a, b) => b.Priority.CompareTo(a.Priority));

            foreach (var prod in ProductionTasks)
            {
                var can_afford = true;
                if (prod.WoodCost > 0 && prod.WoodCost > remaining_wood)
                {
                    can_afford = false;
                }
                else if (prod.FoodCost > 0 && prod.FoodCost > remaining_food)
                {
                    can_afford = false;
                }
                else if (prod.GoldCost > 0 && prod.GoldCost > remaining_gold)
                {
                    can_afford = false;
                }
                else if (prod.StoneCost > 0 && prod.StoneCost > remaining_stone)
                {
                    can_afford = false;
                }

                var deduct = true;
                if (can_afford == false && prod.Blocking == false)
                {
                    deduct = false;
                }

                if (can_afford)
                {
                    yield return prod.GetCommand(this);
                }

                if (deduct)
                {
                    remaining_wood -= prod.WoodCost;
                    remaining_food -= prod.FoodCost;
                    remaining_gold -= prod.GoldCost;
                    remaining_stone -= prod.StoneCost;
                }
            }

            ProductionTasks.Clear();
        }

        internal void Start(int player, string endpoint, int seed, GameVersion version)
        {
            Stop();

            GameVersion = version;

            if (seed < 0)
            {
                seed = Guid.NewGuid().GetHashCode() ^ DateTime.UtcNow.GetHashCode();
            }

            Rng = new Random(seed);

            PlayerNumber = player;
            Log = new Log(Path.Combine(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName), $"{Name} {PlayerNumber}.log"));

            GameState = new GameState(this);
            InfoModule = new InfoModule() { BotInternal = this };
            MapModule = new MapModule() { BotInternal = this };
            MicroModule = new MicroModule() { BotInternal = this };

            for (int i = 0; i <= 8; i++)
            {
                Players.Add(new Player(this, i));
            }

            BotThread = new Thread(() => Run(endpoint)) { IsBackground = true };
            BotThread.Start();
        }

        private void Run(string endpoint)
        {
            Log.Info($"Started");

            var channel = new Channel(endpoint, ChannelCredentials.Insecure);
            var module_api = new AIModuleAPIClient(channel);
            var api = new ExpertAPIClient(channel);

            if (GameVersion == GameVersion.AOC)
            {
                DatFile = module_api.GetGameDataFilePath(new Protos.GetGameDataFilePathRequest()).Result;
            }

            Tick = 0;
            var sw = new Stopwatch();
            var commands = new List<Command>();
            var previous = DateTime.UtcNow;

            while (!Stopping)
            {
                Log.Info($"Tick {Tick}");

                sw.Restart();
                commands.Clear();

                // update

                var bot_command = new Command();
                bot_command.Add(new UpPendingPlacement() { InSnBuildingId = SN_PENDING_PLACEMENT }, "==", 0,
                    new Protos.Expert.Action.SetStrategicNumber() { InConstSnId = SN_PENDING_PLACEMENT, InConstValue = 0 });

                commands.Add(bot_command);

                foreach (var player in Players)
                {
                    player.Units.Clear();
                }

                foreach (var unit in GameState.GetAllUnits())
                {
                    if (unit.Updated && unit[ObjectData.PLAYER] >= 0)
                    {
                        Players[unit[ObjectData.PLAYER]].Units.Add(unit);
                    }
                }

                commands.AddRange(Update().Where(c => c.HasMessages));
                commands.AddRange(DoProduction());

                foreach (var player in Players)
                {
                    player.RequestUpdate();
                }

                foreach (var tech in Technologies.Values)
                {
                    tech.RequestUpdate();
                }

                foreach (var type in UnitTypes.Values)
                {
                    type.RequestUpdate();
                }

                commands.AddRange(GameState.RequestUpdate());

                commands.AddRange(MicroModule.RequestUpdateInternal().Where(c => c.Messages.Count > 0));
                commands.AddRange(MapModule.RequestUpdateInternal().Where(c => c.Messages.Count > 0));
                commands.AddRange(InfoModule.RequestUpdateInternal().Where(c => c.Messages.Count > 0));

                commands.AddRange(GameElementUpdates.Values);

                // don't send commands if it's been more than 5 seconds since previous update

                if ((DateTime.UtcNow - previous) > TimeSpan.FromSeconds(5))
                {
                    foreach (var command in commands)
                    {
                        command.Reset();
                    }

                    commands.Clear();
                    GameElementUpdates.Clear();

                    Log.Debug("Clearing commands (more than 5 seconds since previous)");
                }

                // set up api call

                var commandlist = new CommandList() { PlayerNumber = PlayerNumber };

                foreach (var command in commands)
                {
                    foreach (var message in command.Messages)
                    {
                        commandlist.Commands.Add(Any.Pack(message));
                    }
                }

                Log.Debug($"RequestUpdate took {sw.ElapsedMilliseconds} ms");

                // make the call

                sw.Restart();

                CommandResultList resultlist;
                try
                {
                    var aw = api.ExecuteCommandListAsync(commandlist);
                    //GC.Collect();
                    resultlist = aw.GetAwaiter().GetResult();
                }
                catch (Exception e)
                {
                    Log.Debug($"{e.Message}");
                    resultlist = null;
                }

                Log.Debug($"Call took {sw.ElapsedMilliseconds} ms");

                if (resultlist == null)
                {
                    foreach (var command in commands)
                    {
                        command.Reset();
                    }
                }
                else
                {
                    sw.Restart();

                    // update the results to the commands

                    Debug.Assert(commands.Sum(c => c.Messages.Count) == resultlist.Results.Count);

                    var offset = 0;
                    foreach (var command in commands)
                    {
                        command.Responses.Clear();

                        for (int i = 0; i < command.Messages.Count; i++)
                        {
                            command.Responses.Add(resultlist.Results[offset + i]);
                        }

                        offset += command.Responses.Count;
                    }

                    if (commands.Count > 0)
                    {
                        Tick++;
                    }

                    // perform update

                    foreach (var element in GameElementUpdates.Keys)
                    {
                        element.Update();
                    }
                    GameElementUpdates.Clear();

                    InfoModule.UpdateInternal();
                    MapModule.UpdateInternal();
                    MicroModule.UpdateInternal();

                    GameState.Update();

                    previous = DateTime.UtcNow;

                    Log.Debug($"Update took {sw.ElapsedMilliseconds} ms");
                }
            }

            channel.ShutdownAsync().Wait();

            Log.Info($"Stopped");
        }
    }
}
