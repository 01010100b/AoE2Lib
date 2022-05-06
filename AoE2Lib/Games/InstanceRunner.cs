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
        private ConcurrentQueue<KeyValuePair<Game, Dictionary<int, Bot>>> Games { get; set; }
        private Thread Thread { get; set; }
        private volatile bool Stopping = false;

        public InstanceRunner(string exe, string args = null, double speed = AoEInstance.SPEED_FAST)
        {
            Exe = exe;
            Args = args;
            Speed = speed;
        }

        public void Start(ConcurrentQueue<KeyValuePair<Game, Dictionary<int, Bot>>> games)
        {
            if (IsRunning)
            {
                Stop();
            }

            Games = games;

            Thread = new Thread(() => Run()) { IsBackground = true };
            Thread.Start();
        }

        public void Stop()
        {
            Stopping = true;
            Thread?.Join();
            Games = null;
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
                    Thread.Sleep(5000);
                    aoe = AoEInstance.StartInstance(Exe, Args, Speed);

                    Thread.Sleep(5000);
                }
                else if (Games.TryDequeue(out var game))
                {
                    if (game.Key != null)
                    {
                        try
                        {
                            Thread.Sleep(2000);

                            foreach (var bot in game.Value)
                            {
                                aoe.StartBot(bot.Value, bot.Key);
                            }

                            aoe.StartGame(game.Key, RunMinimized);

                            while (!game.Key.Finished)
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
                        finally
                        {
                            foreach (var bot in game.Value.Values)
                            {
                                bot.Stop();
                            }
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
