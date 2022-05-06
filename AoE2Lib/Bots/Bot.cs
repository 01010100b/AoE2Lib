using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Protos.Expert;
using Protos.Expert.Action;
using Protos.Expert.Fact;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using static Protos.AIModuleAPI;
using static Protos.Expert.ExpertAPI;

namespace AoE2Lib.Bots
{
    public abstract class Bot
    {
        // goals 400-420 are reserved, goal 420 is automatically set to Id
        public const int GOAL_START = 401;

        public abstract int Id { get; }
        public virtual string Name { get { return GetType().Name; } }
        public GameVersion GameVersion { get; private set; }
        public int PlayerNumber { get; private set; } = -1;
        public Log Log { get; private set; }
        public Random Rng { get; private set; }
        public GameState GameState { get; private set; }
        public string DatFilePath { get; private set; } = null;
        public bool AutoFindUnits { get; set; } = true; // automatically find units
        public int AutoUpdateUnits { get; set; } = 100; // units to update per tick per player

        private Thread BotThread { get; set; } = null;
        private volatile bool Stopping = false;

        public void Stop()
        {
            Stopping = true;

            BotThread?.Join();
            BotThread = null;

            Stopped();
            GameState = null;
            Log?.Dispose();
            Log = null;

            Stopping = false;
        }

        protected abstract void NewGame();
        protected abstract void Stopped();
        protected abstract IEnumerable<Command> Tick();

        internal void Start(int player, IPEndPoint endpoint, GameVersion version)
        {
            if (BotThread != null)
            {
                Stop();
            }

            GameVersion = version;
            PlayerNumber = player;
            Log = new Log(Path.Combine(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName), $"Player {PlayerNumber}.log"));
            Rng = new Random(Guid.NewGuid().GetHashCode());
            
            BotThread = new Thread(() => Run(endpoint)) { IsBackground = true };
            BotThread.Start();
        }

        private void Run(IPEndPoint endpoint)
        {
            var lib = GetType().BaseType.Assembly.GetName();
            Log.Info($"Using {lib.Name} {lib.Version}");

            lib = GetType().Assembly.GetName();
            Log.Info($"Started {Name} {lib.Version} for player {PlayerNumber}");

            var channel = new Channel(endpoint.ToString(), ChannelCredentials.Insecure);
            var module_api = new AIModuleAPIClient(channel);
            var api = new ExpertAPIClient(channel);

            StartNewGame(module_api);

            var sw = new Stopwatch();
            var commands = new List<Command>();
            var previous = DateTime.UtcNow;
            var game_time = 0;

            while (!Stopping)
            {
                // update

                sw.Restart();
                GameState.Update();
                commands.Clear();

                var first_command = new Command();
                first_command.Add(new GameTime());
                first_command.Add(new SetGoal() { InConstGoalId = 420, InConstValue = Id });
                commands.Add(first_command);

                commands.AddRange(Tick().Where(c => c.HasMessages));
                commands.AddRange(GameState.RequestUpdate());

                var commandlist = new CommandList() { PlayerNumber = PlayerNumber };

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

                foreach (var command in commands)
                {
                    foreach (var message in command.Messages)
                    {
                        commandlist.Commands.Add(Any.Pack(message));
                    }
                }

                Log.Info($"Update took {sw.ElapsedMilliseconds} ms");

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

                if (resultlist == null)
                {
                    foreach (var command in commands)
                    {
                        command.Reset();
                    }

                    commands.Clear();
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

                    if (first_command.HasResponses)
                    {
                        var ngt = first_command.Responses[0].Unpack<GameTimeResult>().Result;
                        if (ngt >= 0)
                        {
                            if (ngt < game_time)
                            {
                                StartNewGame(module_api);
                            }

                            game_time = ngt;
                        }
                    }

                    previous = DateTime.UtcNow;
                }

                Log.Info($"Call took {sw.ElapsedMilliseconds} ms");
                Log.Debug($"Bot Game time {game_time}");
            }

            channel.ShutdownAsync().Wait();
            Log.Info($"Stopped");
        }

        private void StartNewGame(AIModuleAPIClient module_api)
        {
            GameState = new GameState(this);

            if (GameVersion == GameVersion.AOC)
            {
                DatFilePath = module_api.GetGameDataFilePath(new Protos.GetGameDataFilePathRequest()).Result;
            }
            else
            {
                DatFilePath = null;
            }

            NewGame();

            Log.Info("New Game");
        }
    }
}
