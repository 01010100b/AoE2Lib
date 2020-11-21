using AoE2Lib.Bots.Modules;
using AoE2Lib.Mods;
using AoE2Lib.Utils;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using PeNet.Structures;
using Protos;
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
    public class BotManager : IDisposable
    {
        public readonly GameInstance GameInstance;

        private readonly Channel Channel;
        private readonly Thread[] Threads = new Thread[8];
        private readonly Dictionary<int, Func<Bot>> RegisteredBots = new Dictionary<int, Func<Bot>>();
        private readonly Dictionary<int, Bot> Players = new Dictionary<int, Bot>();
        private int PreviousGameTime { get; set; } = 0;
        private readonly Log Log = Log.Static;
        private volatile bool Stopping = false;

        private bool DisposedValue;

        public BotManager(GameInstance instance)
        {
            GameInstance = instance;
            GameInstance.StartAIModule();

            Channel = new Channel("localhost:37412", ChannelCredentials.Insecure);
        }

        public void RegisterBot<T>() where T : Bot, new()
        {
            var bot = new T();

            lock (RegisteredBots)
            {
                if (!RegisteredBots.ContainsKey(bot.Id))
                {
                    var create = new Func<T>(() => new T());
                    RegisteredBots.Add(bot.Id, create);

                    Log.Info($"Registered bot {bot.Name}");
                }
            }
        }

        public void Start()
        {
            Stop();

            for (int i = 0; i < Threads.Length; i++)
            {
                var player = i + 1;
                var thread = new Thread(() => RunPlayer(player));
                Threads[i] = thread;
                thread.Start();
            }

            Log.Info("Bot manager started");
        }

        public void Stop()
        {
            Stopping = true;

            foreach (var thread in Threads)
            {
                thread?.Join();
            }

            Stopping = false;

            Log.Info("Bot manager stopped");
        }

        public void Exit()
        {
            Stop();

            try
            {
                var module = new AIModuleAPIClient(Channel);
                module.Unload(new UnloadRequest(), new CallOptions());
                Dispose();
            }
            catch
            {

            }
        }

        private void RunPlayer(int player)
        {
            var api = new ExpertAPIClient(Channel);

            while (!Stopping)
            {
                Log.Info($"Player {player} ID: CHECKiNG");
                var commandlist = new CommandList() { PlayerNumber = player };
                commandlist.Commands.Add(Any.Pack(new GameTime()));
                commandlist.Commands.Add(Any.Pack(new Goal() { GoalId = 420 }));

                CommandResultList result;
                try
                {
                    result = api.ExecuteCommandList(commandlist);
                }
                catch (Exception e)
                {
                    Log.Info(e.Message);
                    result = null;
                    Log.Info($"Player {player} ID: FAILED");
                }

                if (result != null)
                {
                    lock (Threads)
                    {
                        var gametime = result.Results[0].Unpack<GameTimeResult>().Result;
                        var id = result.Results[1].Unpack<GoalResult>().Result;

                        Log.Info($"Player {player} ID: {id}");
                        // new game?
                        if (gametime < PreviousGameTime - 1)
                        {
                            foreach (var bot in Players.Values)
                            {
                                bot.Stop();
                            }

                            Players.Clear();

                            Log.Debug("Game restarted");
                        }

                        PreviousGameTime = gametime;

                        if (RegisteredBots.TryGetValue(id, out Func<Bot> create))
                        {
                            if (Players.TryGetValue(result.PlayerNumber, out Bot current))
                            {
                                if (current.Id != id)
                                {
                                    current.Stop();
                                    Players.Remove(result.PlayerNumber);
                                }
                            }

                            if (!Players.ContainsKey(result.PlayerNumber))
                            {
                                var bot = create();

                                bot.AddModule(new InfoModule());
                                bot.AddModule(new SpendingModule());
                                bot.AddModule(new MapModule());
                                bot.AddModule(new PlayersModule());
                                bot.AddModule(new UnitsModule());
                                bot.AddModule(new ResearchModule());
                                bot.AddModule(new PlacementModule());

                                Players.Add(result.PlayerNumber, bot);

                                var botapi = new ExpertAPIClient(Channel);
                                var mod = new Mod();
                                mod.LoadDE();
                                bot.Start(mod, result.PlayerNumber, botapi);

                                Log.Debug($"{bot.Name} taking control of player {result.PlayerNumber}");
                            }
                        }
                    }
                }
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!DisposedValue)
            {
                if (disposing)
                {
                    // dispose managed state (managed objects)
                }

                // free unmanaged resources (unmanaged objects) and override finalizer
                // set large fields to null
                DisposedValue = true;

                try
                {
                    Channel.ShutdownAsync().Wait();
                }
                catch
                {

                }
            }
        }

        // // override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        ~BotManager()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
