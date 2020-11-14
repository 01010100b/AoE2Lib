using AoE2Lib.Mods;
using AoE2Lib.Utils;
using Google.Protobuf.WellKnownTypes;
using Protos.Expert;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using static Protos.Expert.ExpertAPI;

namespace AoE2Lib.Bots
{
    public abstract class Bot
    {
        public abstract string Name { get; }
        public abstract int Id { get; }
        public Mod Mod { get; private set; } = null;
        public int PlayerNumber { get; private set; } = -1;

        private Thread BotThread { get; set; } = null;
        private volatile bool Stopping = false;
        private readonly List<Module> Modules = new List<Module>();

        public T GetModule<T>() where T: Module
        {
            lock (Modules)
            {
                return Modules.OfType<T>().FirstOrDefault();
            }
        }

        public void AddModule<T>(T module) where T : Module
        {
            if (GetModule<T>() == null)
            {
                lock (Modules)
                {
                    Modules.Add(module);
                    module.Bot = this;
                }
            }
        }

        protected abstract IEnumerable<Command> RequestUpdate();
        protected abstract void Update();

        protected void Log(object message)
        {
            Utils.Log.Write(message);
        }

        internal void Start(Mod mod, int player, ExpertAPIClient api)
        {
            Stop();

            Mod = mod;
            PlayerNumber = player;

            BotThread = new Thread(() => Run(api)) { IsBackground = true };
            BotThread.Start();
        }

        internal void Stop()
        {
            Stopping = true;

            BotThread?.Join();
            BotThread = null;

            Stopping = false;
        }

        private void Run(ExpertAPIClient api)
        {
            var commands = new List<Command>();

            while (!Stopping)
            {
                commands.Clear();

                commands.AddRange(RequestUpdate());

                // add modules in reverse

                List<Module> modules = null;
                lock (Modules)
                {
                    modules = Modules.ToList();
                }
                
                modules.Reverse();
                foreach (var module in modules)
                {
                    commands.AddRange(module.RequestUpdate());
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

                // make the call

                CommandResultList resultlist = null;
                try
                {
                    resultlist = api.ExecuteCommandList(commandlist);
                }
                catch (Exception e)
                {
                    Utils.Log.Debug(e.Message);
                    resultlist = null;
                }

                if (resultlist != null)
                {
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

                    // update the modules

                    modules.Reverse();
                    foreach (var module in modules)
                    {
                        module.Update();
                    }

                    Update();
                }
            }
        }
    }
}
