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
using Unary.Utils;
using static Protos.AIModuleAPI;
using static Protos.Expert.ExpertAPI;

namespace Unary
{
    class Bot : IDisposable
    {
        public int Player { get; private set; } 
        public Strategy Strategy { get; private set; }
        public GameState GameState { get; private set; }
        public readonly Random RNG = new Random(Guid.NewGuid().GetHashCode() ^ DateTime.UtcNow.Ticks.GetHashCode());
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
        private readonly List<Module> Modules = new List<Module>();
        
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

        public void Start(int player)
        {
            Stop();

            Player = player;
            GameState = new GameState();
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

            Stopping = false;
        }

        private void Run()
        {
            Stopped = false;

            while (!Stopping)
            {
                var commands = new List<Command>();

                GameState.RequestUpdate(this);
                commands.Add(GameState.Command);

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

                foreach (var module in Modules)
                {
                    var command = module.Command;
                    if (command.Messages.Count > 0)
                    {
                        commands.Add(command);
                    }
                }

                var commandlist = new CommandList
                {
                    PlayerNumber = Player
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
                    Log.Debug(e.Message);
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

                    foreach (var module in Modules)
                    {
                        module.Update();
                    }

                    LogState();
                    Log.Debug(StateLog);
                }
            }

            Stopped = true;
        }

        private void LogState()
        {
            var sb = new StringBuilder();
            var me = GameState.Players[Player];

            sb.AppendLine($"---- CURRENT STATE ----");
            sb.AppendLine($"Game time: {GameState.GameTime}");
            sb.AppendLine($"Tiles: {GameState.Tiles.Count} of which {GameState.Tiles.Values.Count(t => t.Explored)} explored");

            sb.Append($"Player: {me.PlayerNumber} ");
            sb.AppendLine($"Civ {me.CivilianPopulation} Mil {me.MilitaryPopulation} Wood {me.WoodAmount} Food {me.FoodAmount} Gold {me.GoldAmount} Stone {me.StoneAmount}");

            foreach (var player in GameState.Players.Values)
            {
                me = player;
                sb.Append($"Player: {me.PlayerNumber} ");
                sb.AppendLine($"Civ {me.CivilianPopulation} Mil {me.MilitaryPopulation} Wood {me.WoodAmount} Food {me.FoodAmount} Gold {me.GoldAmount} Stone {me.StoneAmount}");
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
