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
using System.IO;
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
        private readonly ExpertAPIClient[] Clients;
        private readonly Dictionary<int, Func<Bot>> RegisteredBots = new Dictionary<int, Func<Bot>>();
        private readonly Dictionary<int, Bot> Players = new Dictionary<int, Bot>();
        private int PreviousGameTime { get; set; } = 0;

        private Thread ManagerThread { get; set; } = null;
        private volatile bool Stopping = false;

        private bool DisposedValue;

        public BotManager(GameInstance instance)
        {
            GameInstance = instance;
            GameInstance.InjectDll(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "aimodule-de.dll"));

            Channel = new Channel("localhost:37412", ChannelCredentials.Insecure);
            
            Clients = new ExpertAPIClient[8];
            for (int i = 0; i < Clients.Length; i++)
            {
                Clients[i] = new ExpertAPIClient(Channel);
            }
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

            ManagerThread = new Thread(() => Run()) { IsBackground = true };
            ManagerThread.Start();

            Log.Info("Bot manager started");
        }

        public void Stop()
        {
            Stopping = true;
            ManagerThread?.Join();
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

        private void Run()
        {
            var mod = new Mod();
            PreviousGameTime = 0;

            while (!Stopping)
            {
                var results = new List<AsyncUnaryCall<CommandResultList>>();

                for (int i = 0; i < Clients.Length; i++)
                {
                    var commandlist = new CommandList() { PlayerNumber = i + 1 };
                    commandlist.Commands.Add(Any.Pack(new GameTime()));
                    commandlist.Commands.Add(Any.Pack(new Goal() { GoalId = 420 }));

                    results.Add(Clients[i].ExecuteCommandListAsync(commandlist));
                }

                for (int i = 0; i < results.Count; i++)
                {
                    CommandResultList result = null;
                    try
                    {
                        result = results[i].GetAwaiter().GetResult();
                    }
                    catch (Exception e)
                    {
                        Log.Debug(e.Message);
                    }

                    if (result != null)
                    {
                        var player = i + 1;
                        var gametime = result.Results[0].Unpack<GameTimeResult>().Result;
                        var id = result.Results[1].Unpack<GoalResult>().Result;

                        // new game?
                        if (gametime < PreviousGameTime - 10)
                        {
                            foreach (var bot in Players.Values)
                            {
                                bot.Stop();
                            }

                            Players.Clear();

                            Log.Info("Game restarted");
                        }

                        PreviousGameTime = gametime;

                        if (RegisteredBots.TryGetValue(id, out Func<Bot> create))
                        {
                            if (Players.TryGetValue(player, out Bot current))
                            {
                                if (current.Id != id)
                                {
                                    current.Stop();
                                    Players.Remove(player);
                                }
                            }

                            if (!Players.ContainsKey(player))
                            {
                                var bot = create();
                                var api = new ExpertAPIClient(Channel);

                                Players.Add(player, bot);
                                bot.Start(mod, player, api);

                                Log.Info($"{bot.Name} taking control of player {player}");
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
