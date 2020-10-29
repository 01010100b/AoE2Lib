using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Protos;
using Protos.Expert;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Protos.AIModuleAPI;
using static Protos.Expert.ExpertAPI;

namespace Unary
{
    class Bot : IDisposable
    {
        public int Player { get; private set; } 
        public Strategy Strategy { get; private set; }
        public GameState GameState { get; private set; }
        
        private readonly Channel Channel;
        private readonly AIModuleAPIClient ModuleAPI;
        private readonly ExpertAPIClient ExpertAPI;
        private readonly Dictionary<IMessage, Any> Messages = new Dictionary<IMessage, Any>();
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

        public T GetResponse<T>(IMessage message) where T : IMessage, new()
        {
            return Messages[message].Unpack<T>();
        }

        private void Run()
        {
            Stopped = false;

            while (!Stopping)
            {
                Strategy.Update(this);

                var commands = new List<IMessage>();

                var commandlist = new CommandList
                {
                    PlayerNumber = Player
                };

                foreach (var module in Modules)
                {
                    foreach (var message in module.GetMessages(this))
                    {
                        commands.Add(message);
                    }
                }

                foreach (var command in commands)
                {
                    commandlist.Commands.Add(Any.Pack(command));
                }

                Messages.Clear();

                try
                {
                    var resultlist = ExpertAPI.ExecuteCommandList(commandlist);
                    Debug.Assert(commands.Count == resultlist.Results.Count);

                    for (int i = 0; i < commands.Count; i++)
                    {
                        var message = commands[i];
                        var response = resultlist.Results[i];

                        Messages.Add(message, response);
                    }
                }
                catch (Exception e)
                {
                    Log.Debug(e.Message);
                }
                
            }

            Stopped = true;
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

                Channel.ShutdownAsync().Wait();
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
