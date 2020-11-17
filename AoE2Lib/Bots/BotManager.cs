﻿using AoE2Lib.Bots.Modules;
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
        private readonly Log Log = new Log();

        private Thread ManagerThread { get; set; } = null;
        private volatile bool Stopping = false;

        private bool DisposedValue;

        public BotManager(GameInstance instance)
        {
            GameInstance = instance;
            GameInstance.StartAIModule();

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
            PreviousGameTime = 0;

            // TODO parallelize properly
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
                    CommandResultList result;
                    try
                    {
                        result = results[i].GetAwaiter().GetResult();
                    }
                    catch (Exception e)
                    {
                        Log.Info(e.Message);
                        result = null;
                    }

                    if (result != null)
                    {
                        var gametime = result.Results[0].Unpack<GameTimeResult>().Result;
                        var id = result.Results[1].Unpack<GoalResult>().Result;

                        // new game?
                        if (gametime < PreviousGameTime - 2)
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

                                var api = new ExpertAPIClient(Channel);
                                var mod = new Mod();
                                mod.LoadDE();
                                bot.Start(mod, result.PlayerNumber, api);

                                Log.Info($"{bot.Name} taking control of player {result.PlayerNumber}");
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
