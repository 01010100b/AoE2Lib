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

namespace Unary
{
    public partial class FormUnary : Form
    {
        private class FormSettings
        {
            public string ExePath { get; set; } = "path-to-exe";
        }

        private AoEInstance Instance { get; set; }
        private readonly Dictionary<int, Unary> Players = new Dictionary<int, Unary>();

        public FormUnary()
        {
            InitializeComponent();

            var file = Path.Combine(Program.Folder, "uisettings.json");
            
            if (File.Exists(file))
            {
                var exe = Program.Deserialize<FormSettings>(file).ExePath;
                LabelExePath.Text = exe;

                if (File.Exists(exe))
                {
                    ButtonStart.Enabled = true;
                }
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
                    var file = Path.Combine(Program.Folder, "uisettings.json");
                    var settings = new FormSettings() { ExePath = exe };

                    Program.Serialize(settings, file);
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
                var bot = new Unary(Program.Settings);
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
            }

            var thread = new Thread(() => Dev()) { IsBackground = true };
            thread.Start();
        }

        private void Dev()
        {
            const int GAMES_PER_SCENARIO = 10;
            var included_scenarios = new HashSet<string>() {  };
            var included_opponents = new HashSet<string>() {  };

            var runner = new InstanceRunner(LabelExePath.Text);
            var unary = new Unary(Program.Settings);
            var bots = new Dictionary<int, Bot>() { { 1, unary } };
            var opponents = new[] { "Null", "ArcherMicroTest_E" };
            var games = new ConcurrentQueue<Game>();
            var results = new Dictionary<Game, Scenario>();

            runner.RunMinimized = included_scenarios.Count == 0 && included_opponents.Count == 0;
            runner.Start(games, bots);
            Message("runner started");

            for (int i = 0; i < GAMES_PER_SCENARIO; i++)
            {
                foreach (var opponent in opponents)
                {
                    if (included_opponents.Count > 0 && !included_opponents.Contains(opponent))
                    {
                        continue;
                    }

                    foreach (var scenario in Scenario.GetDefaultScenarios())
                    {
                        if (included_scenarios.Count > 0 && !included_scenarios.Contains(scenario.ScenarioName))
                        {
                            continue;
                        }

                        scenario.OpponentAiFile = opponent;

                        var game = scenario.CreateGame("Null");
                        games.Enqueue(game);
                        results.Add(game, scenario);
                    }
                }
            }

            Message($"Total game count {results.Count}");

            foreach (var result in results)
            {
                while (!result.Key.Finished)
                {
                    Thread.Sleep(1000);
                }

                Message($"Ran game {result.Value.ScenarioName} against {result.Value.OpponentAiFile} score {result.Value.GetScore(result.Key):P0}.");
            }

            Message("All games finished");
            runner.Stop();

            var scores = new Dictionary<KeyValuePair<string, string>, double>();

            foreach (var result in results)
            {
                var kvp = new KeyValuePair<string, string>(result.Value.ScenarioName, result.Value.OpponentAiFile);
                var score = result.Value.GetScore(result.Key);

                if (!scores.ContainsKey(kvp))
                {
                    scores.Add(kvp, 0);
                }

                scores[kvp] += score / GAMES_PER_SCENARIO;
            }

            foreach (var score in scores)
            {
                Message($"Test {score.Key.Key} against {score.Key.Value}: {score.Value:P0}");
            }
        }
    }
}
