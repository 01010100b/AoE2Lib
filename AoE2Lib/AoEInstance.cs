using AoE2Lib.Bots;
using AoE2Lib.Games;
using Reloaded.Injector;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace AoE2Lib
{
    public class AoEInstance
    {
        public const int DEFAULT_AIMODULE_PORT = 37412;
        public const int DEFAULT_AUTO_GAME_PORT = 64720;

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, uint dwSize, uint lpNumberOfBytesRead);
        [DllImport("kernel32.dll")]
        private static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, uint nSize, uint lpNumberOfBytesWritten);
        [DllImport("kernel32.dll")]
        private static extern bool VirtualProtectEx(IntPtr hProcess, IntPtr lpAddress, UIntPtr dwSize, uint flNewProtect, out uint lpflOldProtect);

        public bool HasExited => Process.HasExited;
        public GameVersion Version => Process.ProcessName.Contains("AoE2DE") ? GameVersion.DE : GameVersion.AOC;

        private readonly Process Process;
        private readonly HashSet<string> InjectedDlls = new HashSet<string>();
        private readonly int AimodulePort;
        private readonly int AutoGamePort;

        public AoEInstance(Process process, int aimodule_port = DEFAULT_AIMODULE_PORT, int auto_game_port = DEFAULT_AUTO_GAME_PORT)
        {
            Process = process;
            AimodulePort = aimodule_port;
            AutoGamePort = auto_game_port;
        }

        public void StartBot(Bot bot, int player)
        {
            LoadAIModule();
            bot.Start(player, new IPEndPoint(IPAddress.Loopback, AimodulePort), Version);
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

        public void RunGame(Game game)
        {
            LoadAocAutoGame();
            game.Start(AutoGamePort);
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
                throw new NotSupportedException("AutoGame is not supported for DE.");
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
                Thread.Sleep(1000);

                Debug.WriteLine($"Injected dll {Path.GetFileName(file)}");
            }
        }

        public byte[] ReadMemory(IntPtr addr, int length)
        {
            VirtualProtectEx(Process.Handle, addr, (UIntPtr)length, 0x40 /* rw */, out uint protect);

            byte[] array = new byte[length];
            ReadProcessMemory(Process.Handle, addr, array, (uint)length, 0u);

            VirtualProtectEx(Process.Handle, addr, (UIntPtr)length, protect, out _);

            return array;
        }

        public bool WriteMemory(IntPtr addr, byte[] bytes)
        {
            VirtualProtectEx(Process.Handle, addr, (UIntPtr)bytes.Length, 0x40 /* rw */, out uint protect);

            bool flag = WriteProcessMemory(Process.Handle, addr, bytes, (uint)bytes.Length, 0u);

            VirtualProtectEx(Process.Handle, addr, (UIntPtr)bytes.Length, protect, out _);

            return flag;
        }
    }
}
