using AoE2Lib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
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

            var thread = new Thread(() => RunManager());
            thread.IsBackground = true;
            thread.Start();
        }

        private void RunManager()
        {
            const int IDENTITY_GOAL = 511;

            var process = Process.GetProcessesByName("WK")[0];
            var instance = new AoCInstance(process);

            Log.Debug($"connected to process {process.Id}");

            var quaternary = new Quaternary().Id;
            var sw = new Stopwatch();
            var bots = new Dictionary<int, Bot>();

            while (!instance.HasExited)
            {
                while (!instance.IsGameRunning)
                {
                    Thread.Sleep(200);
                }

                Log.Debug("game started");

                sw.Restart();

                while (sw.ElapsedMilliseconds < 3000)
                {
                    for (int i = 1; i <= 8; i++)
                    {
                        if (instance.IsPlayerInGame(i) && !bots.ContainsKey(i))
                        {
                            var id = instance.GetGoals(i)[IDENTITY_GOAL - 1];

                            if (id == quaternary)
                            {
                                var q = new Quaternary();
                                
                                q.Start(instance, i);
                                bots.Add(i, q);

                                Log.Debug($"Quaternary taking control of player {i}");
                            }
                        }
                    }

                    Thread.Sleep(300);
                }

                while (instance.IsGameRunning)
                {
                    Thread.Sleep(1000);
                }

                bots.Clear();
                Log.Debug("game finished");
            }
        }
    }
}
