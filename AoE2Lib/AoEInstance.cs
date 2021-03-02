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
        public string DatFolder => Path.Combine(Directory.GetParent(Path.GetDirectoryName(Process.MainModule.FileName)).FullName, "Data");

        private readonly Process Process;
        private readonly HashSet<string> InjectedDlls = new HashSet<string>();

        public AoEInstance(Process process)
        {
            Process = process;
        }

        public void StartGame()
        {
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 64720));
        }

        public void StartAocAutoGame()
        {
            if (Version == GameVersion.AOC)
            {
                var file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "aoc-auto-game.dll");
                InjectDll(file);
            }

            Thread.Sleep(1000);
        }

        public void StartAIModule()
        {
            if (Version == GameVersion.AOC)
            {
                var file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "aimodule-aoc.dll");
                InjectDll(file);
            }

            Thread.Sleep(1000);
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
