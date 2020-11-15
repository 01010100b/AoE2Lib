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
        private readonly Process Process;
        private readonly HashSet<string> InjectedDlls = new HashSet<string>();

        public GameInstance(string name)
        {
            Process = Process.GetProcessesByName(name)[0];
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

                Log.Info($"Injected dll {name} into process {Process.ProcessName}");
            }
        }
    }
}
