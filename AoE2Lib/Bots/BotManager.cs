using AoE2Lib.Bots.Modules;
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
        public readonly AoEInstance GameInstance;

        private readonly Channel Channel;
        private readonly Thread[] PlayerThreads = new Thread[8];
        private readonly Dictionary<int, Func<Bot>> RegisteredBots = new Dictionary<int, Func<Bot>>();
        private readonly Dictionary<int, Bot> CurrentPlayers = new Dictionary<int, Bot>();
        private readonly TypeOp TypeOp;
        private readonly MathOp MathOp;
        private int PreviousGameTime { get; set; } = 0;
        private readonly Log Log = Log.Static;
        private volatile bool Stopping = false;

        private bool DisposedValue;

        public BotManager(AoEInstance instance)
        {
            GameInstance = instance;
            GameInstance.LoadAIModule();
            Channel = new Channel("localhost:37412", ChannelCredentials.Insecure);
            TypeOp = new TypeOp();
            MathOp = new MathOp();

            if (GameInstance.Version == GameVersion.AOC)
            {
                TypeOp.SetAOC();
                MathOp.SetAOC();
            }
            else if (GameInstance.Version == GameVersion.DE)
            {
                TypeOp.SetDE();
                MathOp.SetDE();
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

                    Log.Info($"BotManager: Registered bot {bot.Name}");
                }
            }
        }

        public void Start()
        {
            Stop();

            for (int i = 0; i < PlayerThreads.Length; i++)
            {
                var player = i + 1;
                var thread = new Thread(() => RunPlayer(player));
                PlayerThreads[i] = thread;
                thread.Start();
            }

            Log.Info("BotManager: Started");
        }

        public void Stop()
        {
            Stopping = true;

            foreach (var thread in PlayerThreads)
            {
                thread?.Join();
            }

            Stopping = false;

            Log.Info("BotManager: Stopped");
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
                Log.Info($"BotManager: Player {player} ID: CHECKING");
                var commandlist = new CommandList() { PlayerNumber = player };
                commandlist.Commands.Add(Any.Pack(new GameTime()));
                commandlist.Commands.Add(Any.Pack(new Goal() { GoalId = Bot.GOAL_ID }));

                CommandResultList result;
                try
                {
                    result = api.ExecuteCommandList(commandlist);
                }
                catch (Exception e)
                {
                    Log.Info($"BotManager: Player {player}: {e.Message}");
                    Log.Info($"BotManager: Player {player} ID: FAILED");
                    result = null;
                }

                if (result != null)
                {
                    lock (PlayerThreads)
                    {
                        var gametime = result.Results[0].Unpack<GameTimeResult>().Result;
                        var id = result.Results[1].Unpack<GoalResult>().Result;

                        Log.Info($"BotManager: Player {player} ID: {id}");
                        // new game?
                        if (gametime < PreviousGameTime - 1)
                        {
                            foreach (var bot in CurrentPlayers.Values)
                            {
                                bot.Stop();
                            }

                            CurrentPlayers.Clear();

                            Log.Info("BotManager: Game restarted");
                        }

                        PreviousGameTime = gametime;

                        if (RegisteredBots.TryGetValue(id, out Func<Bot> create))
                        {
                            if (CurrentPlayers.TryGetValue(result.PlayerNumber, out Bot current))
                            {
                                if (current.Id != id)
                                {
                                    current.Stop();
                                    CurrentPlayers.Remove(result.PlayerNumber);
                                }
                            }

                            if (!CurrentPlayers.ContainsKey(result.PlayerNumber))
                            {
                                var bot = create();
                                bot.TypeOp = TypeOp;
                                bot.MathOp = MathOp;

                                bot.AddModule(new InfoModule());
                                bot.AddModule(new MapModule());
                                bot.AddModule(new PlayersModule());
                                bot.AddModule(new UnitsModule());
                                bot.AddModule(new ResearchModule());
                                bot.AddModule(new MicroModule());

                                CurrentPlayers.Add(result.PlayerNumber, bot);

                                var botapi = new ExpertAPIClient(Channel);
                                bot.Start(result.PlayerNumber, botapi);

                                Log.Info($"BotManager: {bot.Name} taking control of player {result.PlayerNumber}");
                            }
                        }
                    }
                }
            }

            if (CurrentPlayers.TryGetValue(player, out Bot _bot))
            {
                _bot.Stop();
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
