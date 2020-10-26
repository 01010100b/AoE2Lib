using AoE2Lib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Quaternary
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void ButtonTest_Click(object sender, EventArgs e)
        {
            ButtonTest.Enabled = false;
            Refresh();

            var thread = new Thread(() => RunManager("age2_x1.5"));
            thread.IsBackground = true;
            thread.Start();
        }

        private void RunManager(string pname)
        {
            var process = Process.GetProcessesByName(pname)[0];

            using (var instance = new AoCInstance(process))
            {
                LogMessage($"connected to process {process.Id}");

                while (!instance.HasExited)
                {
                    while (!(instance.HasExited || instance.IsGameRunning))
                    {
                        Thread.Sleep(200);
                    }

                    if (instance.HasExited)
                    {
                        break;
                    }

                    RunGame(instance);
                }
            }
        }

        private void RunGame(GameInstance instance)
        {
            const int IDENTITY_GOAL = 511;

            var mod = new Mod();
            mod.LoadWK();
            var quaternary = new Quaternary().Id;
            var sw = new Stopwatch();
            var bots = new Dictionary<int, Bot>();

            LogMessage("game started");

            sw.Start();

            while (sw.ElapsedMilliseconds < 3000)
            {
                for (int i = 1; i <= 8; i++)
                {
                    var goals = instance.GetGoals(i);
                    if (goals != null && !bots.ContainsKey(i))
                    {
                        Thread.Sleep(200);
                        var id = goals[IDENTITY_GOAL - 1];

                        if (id == quaternary)
                        {
                            var q = new Quaternary();

                            q.Start(instance, i, mod);
                            bots.Add(i, q);

                            LogMessage($"Quaternary taking control of player {i}");
                        }
                    }
                }
                Thread.Sleep(300);
            }

            while (instance.IsGameRunning)
            {
                Thread.Sleep(1000);

                var bot = bots.Values.FirstOrDefault();
                if (bot != null && bot is Quaternary q)
                {
                    var state = q.CurrentState;
                    Invoke(new Action(() => TextBoxState.Text = state));
                }
            }

            foreach (var bot in bots.Values)
            {
                bot.Stop();
            }

            bots.Clear();
            sw.Stop();

            LogMessage("game finished");
        }

        private void ButtonCopy_Click(object sender, EventArgs e)
        {
            var from = @"C:\Users\Tim\source\repos\AoE2Lib\AoE2Lib\Bots\Script";
            var to = @"C:\Users\Tim\AppData\Roaming\Microsoft Games\Age of Empires ii\Ai";

            var queue = new Queue<string>();
            queue.Enqueue(from);

            while (queue.Count > 0)
            {
                var f = queue.Dequeue();
                var rel = f.Replace(from, "");
                var t = to + rel;

                if (!Directory.Exists(t))
                {
                    Directory.CreateDirectory(t);
                }
                
                foreach (var file in Directory.EnumerateFiles(f))
                {
                    var ofile = Path.Combine(t, Path.GetFileName(file));
                    File.Copy(file, ofile, true);
                }

                foreach (var dir in Directory.EnumerateDirectories(f))
                {
                    queue.Enqueue(dir);
                }
            }
        }

        private void LogMessage(string message)
        {
            Log.Debug(message);

            Invoke(new Action(() => TextBoxLog.Text += message + "\r\n"));
        }
    }
}
