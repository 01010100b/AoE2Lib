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
        public int Tick { get; private set; } = 0;

        private Thread BotThread { get; set; } = null;
        private volatile bool Stopping = false;
        private readonly List<Module> Modules = new List<Module>();

        public bool HasModule<T>() where T : Module
        {
            return GetModule<T>() != default(T);
        }

        public T GetModule<T>() where T: Module
        {
            lock (Modules)
            {
                return Modules.OfType<T>().FirstOrDefault();
            }
        }

        public void AddModule<T>(T module) where T : Module
        {
            lock (Modules)
            {
                if (Modules.OfType<T>().Count() == 0)
                {
                    Modules.Add(module);
                    module.BotInternal = this;
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
            Utils.Log.Info($"Bot {Name} playing {PlayerNumber} has started");

            Tick = 0;

            var sw = new Stopwatch();
            var commands = new List<Command>();

            while (!Stopping)
            {
                Utils.Log.Info($"Tick {Tick}");

                sw.Restart();
                commands.Clear();

                // request self update

                commands.AddRange(RequestUpdate().Where(c => c.Messages.Count > 0));

                // request modules update in reverse

                List<Module> modules = null;
                lock (Modules)
                {
                    modules = Modules.ToList();
                }
                
                modules.Reverse();
                foreach (var module in modules)
                {
                    commands.AddRange(module.RequestUpdateInternal().Where(c => c.Messages.Count > 0));
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

                Utils.Log.Info($"RequestUpdate took {sw.ElapsedMilliseconds} ms");

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
                    Utils.Log.Info(e.Message);
                    resultlist = null;
                }

                Utils.Log.Info($"Call took {sw.ElapsedMilliseconds} ms");

                if (resultlist != null)
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

                    // update the modules

                    modules.Reverse();
                    foreach (var module in modules)
                    {
                        module.UpdateInternal();
                    }

                    // update self

                    Update();

                    Tick++;

                    Utils.Log.Info($"Update took {sw.ElapsedMilliseconds} ms");
                }
            }

            Utils.Log.Info($"Bot {Name} playing {PlayerNumber} has stopped");
        }
    }
}
