using AoE2Lib.Utils;
using Reloaded.Injector;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace AoE2Lib
{
    public class GameInstance
    {
        public GameVersion Version => Process.ProcessName.Contains("AoE2DE") ? GameVersion.DE : GameVersion.AOC;

        private readonly Process Process;
        private readonly HashSet<string> InjectedDlls = new HashSet<string>();

        public GameInstance(Process process)
        {
            Process = process;
        }

        public void StartAIModule()
        {
            var file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "aimodule-de.dll");

            if (Version == GameVersion.AOC)
            {
                file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "aimodule-aoc.dll");
            }

            InjectDll(file);
        }

        public void InjectDll(string file)
        {
            lock (InjectedDlls)
            {
                var name = Path.GetFileNameWithoutExtension(file);

                if (InjectedDlls.Contains(name))
                {
                    return;
                }

                using (var injector = new Injector(Process))
                {
                    injector.Inject(file);
                }

                InjectedDlls.Add(name);
            }
        }
    }
}
