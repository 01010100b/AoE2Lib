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
        public Log Log { get; private set; }
        public Random Rng { get; private set; }
        public GameState GameState { get; private set; }

        private Thread BotThread { get; set; } = null;
        private volatile bool Stopping = false;

        public void Stop()
        {
            Stopping = true;

            BotThread?.Join();
            BotThread = null;

            Stopped();

            Log?.Dispose();

            Stopping = false;
        }

        protected virtual void Started() { }
        protected virtual void Stopped() { }
        protected virtual void NewGame() { }
        protected abstract IEnumerable<Command> Tick();

        internal void Start(int player, string endpoint, int seed, GameVersion version)
        {
            Stop();

            GameVersion = version;

            if (seed < 0)
            {
                seed = Guid.NewGuid().GetHashCode() ^ DateTime.UtcNow.GetHashCode();
            }

            PlayerNumber = player;
            Log = new Log(Path.Combine(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName), $"{PlayerNumber}.log"));
            Rng = new Random(seed);
            GameState = new GameState(this);

            Started();

            BotThread = new Thread(() => Run(endpoint)) { IsBackground = true };
            BotThread.Start();
        }

        private void Run(string endpoint)
        {
            var lib = GetType().BaseType.Assembly.GetName();
            Log.Info($"Using {lib.Name} {lib.Version}");

            lib = GetType().Assembly.GetName();
            Log.Info($"Started {Name} {lib.Version} for player {PlayerNumber}");

            var channel = new Channel(endpoint, ChannelCredentials.Insecure);
            var module_api = new AIModuleAPIClient(channel);
            var api = new ExpertAPIClient(channel);

            if (GameVersion == GameVersion.AOC)
            {
                DatFile = module_api.GetGameDataFilePath(new Protos.GetGameDataFilePathRequest()).Result;
            }

            var sw = new Stopwatch();
            var commands = new List<Command>();
            var previous = DateTime.UtcNow;
            var game_time = 0;

            while (!Stopping)
            {
                // update

                sw.Restart();
                commands.Clear();
                

                var first_command = new Command();
                first_command.Add(new GameTime());
                first_command.Add(new UpPendingPlacement() { InSnBuildingId = SN_PENDING_PLACEMENT }, "==", 0,
                    new Protos.Expert.Action.SetStrategicNumber() { InConstSnId = SN_PENDING_PLACEMENT, InConstValue = 0 });
                commands.Add(first_command);

                commands.AddRange(Tick().Where(c => c.HasMessages));
                commands.AddRange(GameState.RequestUpdate());

                if ((DateTime.UtcNow - previous) > TimeSpan.FromSeconds(5))
                {
                    // don't send commands if it's been more than 5 seconds since previous update

                    foreach (var command in commands)
                    {
                        command.Reset();
                    }

                    commands.Clear();
                    Log.Debug("Clearing commands (more than 5 seconds since previous)");
                }

                Log.Debug($"Update took {sw.ElapsedMilliseconds} ms");

                // make the call

                sw.Restart();

                var commandlist = new CommandList() { PlayerNumber = PlayerNumber };

                foreach (var command in commands)
                {
                    foreach (var message in command.Messages)
                    {
                        commandlist.Commands.Add(Any.Pack(message));
                    }
                }

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

                if (resultlist == null)
                {
                    foreach (var command in commands)
                    {
                        command.Reset();
                    }
                }
                else
                {
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

                    previous = DateTime.UtcNow;
                    
                    if (first_command.HasResponses)
                    {
                        var ngt = first_command.Responses[0].Unpack<GameTimeResult>().Result;
                        if (ngt >= 0)
                        {
                            if (ngt < game_time)
                            {
                                GameState = new GameState(this);
                                NewGame();

                                Log.Info("New Game");
                            }

                            game_time = ngt;
                        }

                        Log.Debug($"Bot Game time {game_time}");
                    }

                    GameState.Update();
                }

                Log.Debug($"Call took {sw.ElapsedMilliseconds} ms");
            }

            channel.ShutdownAsync().Wait();
            Log.Info($"Stopped");
        }
    }
}
