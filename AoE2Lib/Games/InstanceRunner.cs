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

        private readonly string Exe;
        private readonly string Args;
        private readonly double Speed;
        private ConcurrentQueue<Game> Games { get; set; }
        private Thread Thread { get; set; }
        private volatile bool Stopping = false;

        public InstanceRunner(string exe, string args = null, double speed = AoEInstance.SPEED_FAST)
        {
            Exe = exe;
            Args = args;
            Speed = speed;
        }

        public void Start(ConcurrentQueue<Game> games)
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
                }

                if (Games.TryDequeue(out var game))
                {
                    if (game != null)
                    {
                        try
                        {
                            aoe.StartGame(game);

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

            aoe?.Kill();
        }
    }
}
