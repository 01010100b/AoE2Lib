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
        public const double SPEED_SLOW = 1;
        public const double SPEED_NORMAL = 1.5;
        public const double SPEED_FAST = 2;

        private static readonly object Lock = new();

        public static AoEInstance StartInstance(string exe, string args = null, double speed = SPEED_FAST, 
            int aimodule_port = DEFAULT_AIMODULE_PORT, int autogame_port = DEFAULT_AUTO_GAME_PORT)
        {
            if (args == null)
            {
                args = "";
            }

            args += $" -multipleinstances -autogameport {autogame_port} -aimoduleport {aimodule_port}";
            Process process = null;

            lock (Lock)
            {
                var sp = (int)Math.Round(speed * 10);
                var old = GetSpeed();
                SetSpeed(sp);

                try
                {
                    process = Process.Start(exe, args);
                    process.WaitForInputIdle();
                }
                finally
                {
                    SetSpeed(old);
                }

                var instance = new AoEInstance(process, aimodule_port, autogame_port);

                if (instance.Version == GameVersion.AOC)
                {
                    instance.LoadAocAutoGame();
                }

                Thread.Sleep(10000);
                instance.LoadAIModule();
                Thread.Sleep(3000);

                return instance;
            }
        }

        private static int GetSpeed()
        {
            try
            {
                var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Microsoft Games\Age of Empires II: The Conquerors Expansion\1.0");
                
                return (int)key.GetValue("Game Speed");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);

                return -1;
            }
        }

        private static void SetSpeed(int speed)
        {
            try
            {
                var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Microsoft Games\Age of Empires II: The Conquerors Expansion\1.0", true);
                key.SetValue("Game Speed", speed);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, uint dwSize, uint lpNumberOfBytesRead);
        [DllImport("kernel32.dll")]
        private static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, uint nSize, uint lpNumberOfBytesWritten);
        [DllImport("kernel32.dll")]
        private static extern bool VirtualProtectEx(IntPtr hProcess, IntPtr lpAddress, UIntPtr dwSize, uint flNewProtect, out uint lpflOldProtect);
        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);
        [DllImport("user32.dll")]
        private static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);

        public bool HasExited => Process.HasExited;
        public GameVersion Version => Process.ProcessName.Contains("AoE2DE") ? GameVersion.DE : GameVersion.AOC;

        private readonly Process Process;
        private IntPtr WindowHandle { get; set; }
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
            if (!HasExited)
            {
                Process.Kill();
                Process.WaitForExit();
                Thread.Sleep(1000);
            }
        }

        public void StartBot(Bot bot, int player, bool log = true)
        {
            LoadAIModule();
            bot.Start(player, new IPEndPoint(IPAddress.Loopback, AimodulePort), Version, log);
        }

        public void StartGame(Game game, bool minimized = false)
        {
            LoadAocAutoGame();
            game.Start(new IPEndPoint(IPAddress.Loopback, AutoGamePort), minimized);

            if (game.GameType == GameType.SCENARIO)
            {
                SendKeys("{ENTER}");
                Thread.Sleep(1000);
            }

            if (minimized)
            {
                Minimize();
            }
            else
            {
                Restore();
            }

            Thread.Sleep(1000);
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

        public void SendKeys(string keys) => SendWindowCommand(() =>
        {
            ShowWindowAsync(WindowHandle, 9);
            Thread.Sleep(500);
            Process.WaitForInputIdle();
            System.Windows.Forms.SendKeys.SendWait(keys);
        });

        public void Minimize() => SendWindowCommand(() => ShowWindowAsync(WindowHandle, 2));

        public void Restore() => SendWindowCommand(() => ShowWindowAsync(WindowHandle, 9));

        private void SendWindowCommand(Action action)
        {
            lock (Lock)
            {
                Process.WaitForInputIdle();

                if (WindowHandle == IntPtr.Zero)
                {
                    WindowHandle = Process.MainWindowHandle;
                }

                if (!WindowHandle.Equals(IntPtr.Zero))
                {
                    if (SetForegroundWindow(WindowHandle))
                    {
                        Process.WaitForInputIdle();
                        action();
                        Thread.Sleep(500);
                        Process.WaitForInputIdle();
                    }
                    else
                    {
                        Debug.WriteLine("Failed to set foreground window");
                    }
                }
                else
                {
                    Debug.WriteLine($"Window handle is null ptr");
                }
            }
        }
    }
}
