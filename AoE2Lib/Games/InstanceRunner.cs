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
        private readonly string Exe;
        private readonly string Args;
        private readonly double Speed;
        private ConcurrentQueue<KeyValuePair<Game, Dictionary<int, Bot>>> Queue { get; set; }
        private Thread Thread { get; set; }
        private volatile bool Stopping = false;

        public InstanceRunner(string exe, string args = null, double speed = AoEInstance.SPEED_FAST)
        {
            Exe = exe;
            Args = args;
            Speed = speed;
        }

        public void Start(ConcurrentQueue<KeyValuePair<Game, Dictionary<int, Bot>>> queue)
        {
            Stop();
            Queue = queue;
            Thread = new Thread(() => Run()) { IsBackground = true };
            Thread.Start();
        }

        public void Stop()
        {
            Stopping = true;
            Thread?.Join();
            Queue = null;
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
                    try
                    {
                        var rng = new Random(Guid.NewGuid().GetHashCode());
                        var autogame = rng.Next(10000, 65000);
                        var aimodule = rng.Next(10000, 65000);

                        Thread.Sleep(5000);
                        aoe = AoEInstance.StartInstance(Exe, Args, Speed, aimodule, autogame);

                        Thread.Sleep(5000);

                        if (aoe.HasExited)
                        {
                            throw new Exception("aoe exited");
                        }
                    }
                    catch (Exception)
                    {
                        Debug.WriteLine($"Instance runner failed to start aoe {Exe}");
                        aoe = null;
                        Thread.Sleep(60 * 1000);
                    }
                }
                else if (Queue.TryDequeue(out var run))
                {
                    if (run.Key != null)
                    {
                        try
                        {
                            Thread.Sleep(1000);

                            foreach (var bot in run.Value)
                            {
                                aoe.StartBot(bot.Value, bot.Key, false);
                            }

                            aoe.StartGame(run.Key, true);

                            var sw = new Stopwatch();
                            sw.Start();

                            while (!run.Key.Finished)
                            {
                                Thread.Sleep(1000);

                                if (DateTime.UtcNow - run.Key.LastProgressTimeUtc < TimeSpan.FromSeconds(20))
                                {
                                    sw.Restart();
                                }

                                if (sw.Elapsed > TimeSpan.FromMinutes(1))
                                {
                                    throw new Exception("Game hasn't progressed for 1 minute.");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            run.Key.Stop();
                            Queue.Enqueue(run);
                            Debug.WriteLine($"Instance runner exception: {ex}");
                            aoe.Kill();
                            aoe = null;
                        }
                        finally
                        {
                            foreach (var bot in run.Value.Values)
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
