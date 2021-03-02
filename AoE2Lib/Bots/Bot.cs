using AoE2Lib.Utils;
using Google.Protobuf.WellKnownTypes;
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
        public const int GOAL_ID = 420;

        public abstract string Name { get; }
        public abstract int Id { get; }
        public int PlayerNumber { get; private set; } = -1;
        public int Tick { get; private set; } = 0;
        public Log Log { get; private set; }
        public TypeOp TypeOp { get; internal set; }
        public MathOp MathOp { get; internal set; }

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

        protected abstract IEnumerable<Command> Update();

        internal void Start(int player, ExpertAPIClient api)
        {
            Stop();

            PlayerNumber = player;
            Log = new Log(Path.Combine(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName), $"{Name} {PlayerNumber}.log"));

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
            Log.Info($"Started");

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

                // don't send commands if it's been more than 5 seconds since previous update

                if ((DateTime.UtcNow - previous) > TimeSpan.FromSeconds(5))
                {
                    commands.Clear();
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

                    // update the modules

                    modules.Reverse();
                    foreach (var module in modules)
                    {
                        module.UpdateInternal();
                    }

                    Tick++;
                    previous = DateTime.UtcNow;

                    Log.Info($"Update took {sw.ElapsedMilliseconds} ms");
                }
            }

            Log.Info($"Stopped");
        }
    }
}
