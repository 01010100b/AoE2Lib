using AoE2Lib.Bots;
using AoE2Lib.Games;
using Microsoft.Win32;
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

        public static AoEInstance StartInstance(string exe, string args = null, double speed = 1.7, 
            int aimodule_port = DEFAULT_AIMODULE_PORT, int autogame_port = DEFAULT_AUTO_GAME_PORT)
        {
            if (aimodule_port != DEFAULT_AIMODULE_PORT)
            {
                throw new NotSupportedException("Changing aimodule port from default not supported yet.");
            }
            else if (autogame_port != DEFAULT_AUTO_GAME_PORT)
            {
                throw new NotSupportedException("Changing auto game port from default not supported yet.");
            }

            Process process = null;

            var sp = (int)Math.Round(speed * 10);
            var old = GetSpeed();
            SetSpeed(sp);

            try
            {
                process = Process.Start(exe, args);
                while (!process.Responding)
                {
                    Thread.Sleep(1000);
                }
            }
            finally
            {
                SetSpeed(old);
            }
            
            Thread.Sleep(10 * 1000);

            var instance = new AoEInstance(process, aimodule_port, autogame_port);
            instance.LoadAIModule();

            if (instance.Version == GameVersion.AOC)
            {
                instance.LoadAocAutoGame();
            }

            return instance;
        }

        private static int GetSpeed()
        {
            var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Microsoft Games\Age of Empires II: The Conquerors Expansion\1.0");
            return (int)key.GetValue("Game Speed");
        }

        private static void SetSpeed(int speed)
        {
            var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Microsoft Games\Age of Empires II: The Conquerors Expansion\1.0", true);
            key.SetValue("Game Speed", speed);
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, uint dwSize, uint lpNumberOfBytesRead);
        [DllImport("kernel32.dll")]
        private static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, uint nSize, uint lpNumberOfBytesWritten);
        [DllImport("kernel32.dll")]
        private static extern bool VirtualProtectEx(IntPtr hProcess, IntPtr lpAddress, UIntPtr dwSize, uint flNewProtect, out uint lpflOldProtect);
        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        public bool HasExited => Process.HasExited;
        public GameVersion Version => Process.ProcessName.Contains("AoE2DE") ? GameVersion.DE : GameVersion.AOC;

        private readonly Process Process;
        private readonly IntPtr WindowHandle;
        private readonly HashSet<string> InjectedDlls = new HashSet<string>();
        private readonly int AimodulePort;
        private readonly int AutoGamePort;

        public AoEInstance(Process process, int aimodule_port = DEFAULT_AIMODULE_PORT, int auto_game_port = DEFAULT_AUTO_GAME_PORT)
        {
            Process = process;
            WindowHandle = Process.MainWindowHandle;
            AimodulePort = aimodule_port;
            AutoGamePort = auto_game_port;
        }

        public void Kill()
        {
            Process.Kill();
        }

        public void StartBot(Bot bot, int player)
        {
            LoadAIModule();
            bot.Start(player, new IPEndPoint(IPAddress.Loopback, AimodulePort), Version);
        }

        public void StartGame(Game game)
        {
            LoadAocAutoGame();
            game.Start(new IPEndPoint(IPAddress.Loopback, AutoGamePort));

            if (game.GameType == GameType.SCENARIO)
            {
                SendKeys("{ENTER}");
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

        public void SendKeys(string keys)
        {
            Process.WaitForInputIdle();

            if (SetForegroundWindow(WindowHandle))
            {
                Debug.WriteLine($"sending keys {keys}");
                System.Windows.Forms.SendKeys.SendWait(keys);
            }
        }
    }
}
