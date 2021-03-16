﻿using AoE2Lib.Bots.Modules;
using AoE2Lib.Utils;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Protos.Expert;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using static Protos.Expert.ExpertAPI;

namespace AoE2Lib.Bots
{
    public abstract class Bot
    {
        public virtual string Name { get { return GetType().Name; } }
        public int PlayerNumber { get; private set; } = -1;
        public int Tick { get; private set; } = 0;
        public Log Log { get; private set; }
        public InfoModule InfoModule { get; private set; }
        public MapModule MapModule { get; private set; }
        public PlayersModule PlayersModule { get; private set; }
        public UnitsModule UnitsModule { get; private set; }
        public ResearchModule ResearchModule { get; private set; }
        public MicroModule MicroModule { get; private set; }
        public Random Rng { get; private set; } = new Random();

        private Thread BotThread { get; set; } = null;
        private volatile bool Stopping = false;
        private readonly Dictionary<GameElement, Command> GameElementUpdates = new Dictionary<GameElement, Command>();

        public void Stop()
        {
            Stopping = true;

            BotThread?.Join();
            BotThread = null;

            Stopping = false;
        }

        protected abstract IEnumerable<Command> Update();

        internal void Start(int player, int seed = -1)
        {
            Stop();

            if (seed < 0)
            {
                seed = Guid.NewGuid().GetHashCode() ^ DateTime.UtcNow.GetHashCode();
            }

            Rng = new Random(seed);

            PlayerNumber = player;
            Log = new Log(Path.Combine(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName), $"{Name} {PlayerNumber}.log"));

            InfoModule = new InfoModule();
            MapModule = new MapModule();
            PlayersModule = new PlayersModule();
            UnitsModule = new UnitsModule();
            ResearchModule = new ResearchModule();
            MicroModule = new MicroModule();

            BotThread = new Thread(() => Run()) { IsBackground = true };
            BotThread.Start();
        }

        internal void UpdateGameElement(GameElement element, Command command)
        {
            GameElementUpdates[element] = command;
        }

        private void Run()
        {
            Log.Info($"Started");

            var channel = new Channel("localhost:37412", ChannelCredentials.Insecure);
            var api = new ExpertAPIClient(channel);

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

                if (Tick > 0)
                {
                    commands.AddRange(Update().Where(c => c.HasMessages));
                }

                commands.AddRange(MicroModule.RequestUpdateInternal().Where(c => c.Messages.Count > 0));
                commands.AddRange(ResearchModule.RequestUpdateInternal().Where(c => c.Messages.Count > 0));
                commands.AddRange(UnitsModule.RequestUpdateInternal().Where(c => c.Messages.Count > 0));
                commands.AddRange(PlayersModule.RequestUpdateInternal().Where(c => c.Messages.Count > 0));
                commands.AddRange(MapModule.RequestUpdateInternal().Where(c => c.Messages.Count > 0));
                commands.AddRange(InfoModule.RequestUpdateInternal().Where(c => c.Messages.Count > 0));

                commands.AddRange(GameElementUpdates.Values);

                // don't send commands if it's been more than 5 seconds since previous update

                if ((DateTime.UtcNow - previous) > TimeSpan.FromSeconds(5))
                {
                    commands.Clear();
                    GameElementUpdates.Clear();
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

                Log.Info($"RequestUpdate took {sw.ElapsedMilliseconds} ms");

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
                    Log.Info($"{e.Message}");
                    resultlist = null;
                }

                Log.Info($"Call took {sw.ElapsedMilliseconds} ms");

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
                    PlayersModule.UpdateInternal();
                    UnitsModule.UpdateInternal();
                    ResearchModule.UpdateInternal();
                    MicroModule.UpdateInternal();

                    previous = DateTime.UtcNow;

                    Log.Info($"Update took {sw.ElapsedMilliseconds} ms");
                }
            }

            channel.ShutdownAsync().Wait();

            Log.Info($"Stopped");
        }
    }
}
