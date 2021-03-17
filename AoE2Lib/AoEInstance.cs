using AoE2Lib.Bots;
using AoE2Lib.Utils;
using Reloaded.Injector;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace AoE2Lib
{
    public class AoEInstance
    {
        public GameVersion Version => Process.ProcessName.Contains("AoE2DE") ? GameVersion.DE : GameVersion.AOC;

        private readonly Process Process;
        private readonly HashSet<string> InjectedDlls = new HashSet<string>();

        public AoEInstance(Process process)
        {
            Process = process;
        }

        public void StartBot(Bot bot, int player, string endpoint = "localhost:37412", int seed = -1)
        {
            LoadAIModule();
            bot.Start(player, endpoint, seed, Version);
        }

        public void RunGame(Game game)
        {
            LoadAocAutoGame();
            game.Start(64720);
        }

        public void LoadAocAutoGame()
        {
            if (Version == GameVersion.AOC)
            {
                var file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "aoc-auto-game.dll");
                InjectDll(file);
            }
            else
            {
                throw new Exception("Not supported for DE.");
            }
        }

        public void LoadAIModule()
        {
            if (Version == GameVersion.AOC)
            {
                var file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "aimodule-aoc.dll");
                InjectDll(file);
            }
            else
            {
                var file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "aimodule-de.dll");
                InjectDll(file);
            }
        }

        public void InjectDll(string file)
        {
            lock (InjectedDlls)
            {
                if (InjectedDlls.Contains(file))
                {
                    return;
                }

                using (var injector = new Injector(Process))
                {
                    injector.Inject(file);
                }

                InjectedDlls.Add(file);

                Debug.WriteLine($"Injected dll {Path.GetFileName(file)}");
            }

            Thread.Sleep(1000);
        }
    }
}
