using AoE2Lib.Bots;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace AoE2Lib.Games
{
    public class InstanceRunner
    {
        public bool IsRunning => Thread != null;
        public bool RunMinimized { get; set; } = false;

        private readonly string Exe;
        private readonly string Args;
        private readonly double Speed;
        private readonly Dictionary<int, Bot> Bots = new();
        private ConcurrentQueue<Game> Games { get; set; }
        private Thread Thread { get; set; }
        private volatile bool Stopping = false;

        public InstanceRunner(string exe, string args = null, double speed = AoEInstance.SPEED_FAST)
        {
            Exe = exe;
            Args = args;
            Speed = speed;
        }

        public void Start(ConcurrentQueue<Game> games, Dictionary<int, Bot> bots)
        {
            if (IsRunning)
            {
                Stop();
            }

            Games = games;
            Bots.Clear();
            
            foreach (var kvp in bots)
            {
                Bots.Add(kvp.Key, kvp.Value);
            }

            Thread = new Thread(() => Run()) { IsBackground = true };
            Thread.Start();
        }

        public void Stop()
        {
            Stopping = true;
            Thread?.Join();
            Games = null;

            foreach (var bot in Bots.Values)
            {
                bot.Stop();
            }

            Bots.Clear();
            Thread = null;
            Stopping = false;
        }

        private void Run()
        {
            AoEInstance aoe = null;

            while (!Stopping)
            {
                if (aoe == null || aoe.HasExited)
                {
                    foreach (var bot in Bots.Values)
                    {
                        bot.Stop();
                    }

                    Thread.Sleep(5000);
                    aoe = AoEInstance.StartInstance(Exe, Args, Speed);

                    foreach (var kvp in Bots)
                    {
                        aoe.StartBot(kvp.Value, kvp.Key);
                    }

                    Thread.Sleep(5000);
                }
                else if (Games.TryDequeue(out var game))
                {
                    if (game != null)
                    {
                        try
                        {
                            Thread.Sleep(2000);
                            aoe.StartGame(game, RunMinimized);

                            while (!game.Finished)
                            {
                                Thread.Sleep(1000);
                            }
                        }
                        catch (Exception ex)
                        {
                            Games.Enqueue(game);
                            Debug.WriteLine(ex);
                            aoe.Kill();
                            aoe = null;
                        }
                    }
                }
                else
                {
                    Thread.Sleep(1000);
                }
            }

            Thread.Sleep(1000);
            aoe?.Kill();
        }
    }
}
