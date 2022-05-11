using AoE2Lib;
using AoE2Lib.Bots;
using AoE2Lib.Games;
using System;
using System.Collections.Concurrent;
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
using Unary.Learning;
using Unary.Learning.Scenarios;

namespace Unary.UI
{
    public partial class FormUnary : Form
    {
        private AoEInstance Instance { get; set; }
        private readonly Dictionary<int, Unary> Players = new Dictionary<int, Unary>();
        private readonly UISettings UISettings;

        public FormUnary()
        {
            InitializeComponent();

            UISettings = UISettings.Load();
            LabelExePath.Text = UISettings.ExePath;

            if (File.Exists(UISettings.ExePath))
            {
                ButtonStart.Enabled = true;
            }
#if DEBUG
            ButtonDev.Enabled = true;
#endif
        }

        private void ButtonBrowseExe_Click(object sender, EventArgs e)
        {
            var diag = new OpenFileDialog()
            {
                CheckFileExists = true,
                CheckPathExists = true,
                Filter = "Exe files|*.exe"
            };

            var res = diag.ShowDialog();

            if (res == DialogResult.OK)
            {
                var exe = diag.FileName;

                if (File.Exists(exe))
                {
                    UISettings.ExePath = exe;
                    UISettings.Save();
                    LabelExePath.Text = exe;
                    ButtonStart.Enabled = true;
                }
            }
        }

        private void ButtonStart_Click(object sender, EventArgs e)
        {
            ButtonDev.Enabled = false;
            ButtonStart.Enabled = false;
            ButtonBrowseExe.Enabled = false;
            Cursor = Cursors.WaitCursor;
            Refresh();

            EnsureInstance();

            var player = (int)NumericPlayer.Value;
            Message($"Starting for player {player}...");

            if (Players.ContainsKey(player))
            {
                Message($"Player {player} is already running.");
            }
            else
            {
                var bot = new Unary();
                Instance.StartBot(bot, player);
                Players.Add(player, bot);
                Message($"Started player {player}");
                ButtonStop.Enabled = true;
            }

            Cursor = Cursors.Default;
            ButtonStart.Enabled = true;
        }

        private void ButtonStop_Click(object sender, EventArgs e)
        {
            ButtonStart.Enabled = false;
            ButtonStop.Enabled = false;
            Cursor = Cursors.WaitCursor;
            Refresh();

            Message("Stopping all players...");

            var tasks = new List<Task>();
            foreach (var bot in Players.Values)
            {
                var task = new Task(() => bot.Stop());
                tasks.Add(task);
                task.Start();
            }

            foreach (var task in tasks)
            {
                task.Wait();
            }

            Message("Stopped all players");
            Players.Clear();

            ButtonStart.Enabled = true;
            Cursor = Cursors.Default;
        }

        private void Message(string message)
        {
            Invoke(() =>
            {
                Program.Log.Info($"Unary UI: {message}");
                message = $"{DateTime.Now}: {message}";

                var lines = TextMessages.Lines.ToList();
                lines.Add(message);
                TextMessages.Lines = lines.ToArray();
            });
        }

        private void EnsureInstance()
        {
            if (Instance != null && !Instance.HasExited)
            {
                return;
            }

            var file = LabelExePath.Text;
            var name = Path.GetFileNameWithoutExtension(file);
            
            Message($"Connecting to process {name}...");

            var running = Process.GetProcessesByName(name);

            if (running.Length > 0)
            {
                var process = Process.GetProcessesByName(name)[0];
                Instance = new AoEInstance(process);
                Message($"Connected to process {process.Id}");
            }
            else
            {
                Instance = AoEInstance.StartInstance(file);
                Message($"Started AoE {file}");
            }
        }

        private void ButtonDev_Click(object sender, EventArgs e)
        {
            ButtonStart.Enabled = false;
            ButtonDev.Enabled = false;
            Refresh();

            if (Instance != null)
            {
                Instance.Kill();
                Thread.Sleep(5000);
            }

            var dev = new FormDev();
            dev.Show();
        }
    }
}
