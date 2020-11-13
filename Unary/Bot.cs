using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Protos;
using Protos.Expert;
using Protos.Expert.Action;
using Protos.Expert.Fact;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Unary.Mods;
using Unary.Modules;
using Unary.Strategies;
using Unary.Utils;
using static Protos.AIModuleAPI;
using static Protos.Expert.ExpertAPI;

namespace Unary
{
    public class Bot : IDisposable
    {
        public Mod Mod { get; private set; }
        public GameState GameState { get; private set; }
        public UnitFindModule UnitFindModule { get; private set; }
        public TrainModule TrainModule { get; private set; }
        public BuildModule BuildModule { get; private set; }
        public ResearchModule ResearchModule { get; private set; }
        public MicroModule MicroModule { get; private set; }
        public Strategy Strategy
        {
            get
            {
                lock (this)
                {
                    return _Strategy;
                }
            }
            set
            {
                lock (this)
                {
                    _Strategy = value;
                }
            }
        }
        private Strategy _Strategy { get; set; }
        public string StateLog
        {
            get
            {
                lock (this)
                {
                    return _StateLog;
                }
            }
            set
            {
                lock (this)
                {
                    _StateLog = value;
                }
            }
        }
        private string _StateLog { get; set; }

        private readonly Channel Channel;
        private readonly AIModuleAPIClient ModuleAPI;
        private readonly ExpertAPIClient ExpertAPI;
        
        private Thread BotThread { get; set; } = null;
        private bool Stopping { get; set; } = false;
        private volatile bool Stopped = true;
        private bool DisposedValue { get; set; }

        public Bot()
        {
            Channel = new Channel("localhost:37412", ChannelCredentials.Insecure);
            ModuleAPI = new AIModuleAPIClient(Channel);
            ExpertAPI = new ExpertAPIClient(Channel);
        }

        public void Start(Mod mod, int player)
        {
            Stop();

            Mod = mod;
            GameState = new GameState(player);
            UnitFindModule = new UnitFindModule();
            TrainModule = new TrainModule();
            BuildModule = new BuildModule();
            ResearchModule = new ResearchModule();
            MicroModule = new MicroModule();
            Strategy = new BasicStrategy();

            StateLog = "";

            BotThread = new Thread(() => Run())
            {
                IsBackground = true
            };

            BotThread.Start();
        }

        public void Stop()
        {
            Stopping = true;

            while (!Stopped)
            {
                Thread.Sleep(100);
            }

            BotThread?.Join();

            Stopping = false;
        }

        public void Exit()
        {
            if (DisposedValue == true)
            {
                return;
            }

            Stop();

            ModuleAPI.Unload(new UnloadRequest(), new CallOptions());
            Dispose();
        }

        private void Run()
        {
            Stopped = false;

            while (!Stopping)
            {
                if (GameState.Tick > 0)
                {
                    Strategy.UpdateInternal(this);
                }

                var commands = new List<Command>();

                commands.AddRange(UnitFindModule.RequestUpdate(this));
                commands.AddRange(TrainModule.RequestUpdate(this));
                commands.AddRange(BuildModule.RequestUpdate(this));
                commands.AddRange(ResearchModule.RequestUpdate(this));
                commands.AddRange(MicroModule.RequestUpdate(this));
                commands.Add(GameState.RequestUpdate());

                foreach (var player in GameState.Players.Values)
                {
                    var command = player.Command;
                    if (command.Messages.Count > 0)
                    {
                        commands.Add(command);
                    }
                }

                foreach (var tile in GameState.Tiles.Values)
                {
                    var command = tile.Command;
                    if (command.Messages.Count > 0)
                    {
                        commands.Add(command);
                    }
                }

                foreach (var unit in GameState.Units.Values)
                {
                    var command = unit.Command;
                    if (command.Messages.Count > 0)
                    {
                        commands.Add(command);
                    }
                }

                var commandlist = new CommandList
                {
                    PlayerNumber = GameState.PlayerNumber
                };

                foreach (var command in commands)
                {
                    foreach (var message in command.Messages)
                    {
                        commandlist.Commands.Add(Any.Pack(message));
                    }
                }

                CommandResultList resultlist = null;
                try
                {
                    resultlist = ExpertAPI.ExecuteCommandList(commandlist);
                }
                catch (Exception e)
                {
                    Log.Info(e.Message);
                }

                if (resultlist != null)
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

                    GameState.Update();
                    UnitFindModule.Update(this);
                    TrainModule.Update(this);
                    BuildModule.Update(this);
                    ResearchModule.Update(this);
                    MicroModule.Update(this);

                    LogState();
                    Log.Info(StateLog);
                }
            }

            Stopped = true;
        }

        private void LogState()
        {
            var sb = new StringBuilder();
            var me = GameState.Players[GameState.PlayerNumber];

            sb.AppendLine($"---- CURRENT STATE ----");
            sb.AppendLine($"Game time: {GameState.GameTime}");
            sb.AppendLine($"Tiles: {GameState.Tiles.Count} of which {GameState.Tiles.Values.Count(t => t.Explored)} explored");

            sb.AppendLine($"My Player: {me.PlayerNumber} at X {GameState.MyPosition.X} Y {GameState.MyPosition.Y}");
            sb.AppendLine($"Known units: {GameState.Units.Count} of which {GameState.Units.Values.Count(u => u.Targetable)} targetable");

            foreach (var player in GameState.Players.Values)
            {
                me = player;
                sb.AppendLine($"Player: {me.PlayerNumber} Civ {me.CivilianPopulation} Mil {me.MilitaryPopulation} " +
                    $"Wood {me.WoodAmount} Food {me.FoodAmount} Gold {me.GoldAmount} Stone {me.StoneAmount}");
            }

            StateLog = sb.ToString();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!DisposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
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

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        ~Bot()
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
