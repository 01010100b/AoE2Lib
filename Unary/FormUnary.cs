using AoE2Lib;
using AoE2Lib.Games;
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
            Program.Log.Debug($"Unary UI: {message}");

            var lines = TextMessages.Lines.ToList();
            lines.Add(message);
            TextMessages.Lines = lines.ToArray();
        }

        private void EnsureInstance()
        {
            if (Instance != null)
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

            EnsureInstance();

            var thread = new Thread(() => RunGames()) { IsBackground = true };
            thread.Start();
        }

        private void RunGames()
        {
            const int NUM = 10;
            Debug.WriteLine("Running auto games");

            var unary = new Unary(Program.Settings);
            Instance.StartBot(unary, 1);
            // "MC1 (1) archer vs archer (britons)", "MC1 (2) xbow vs xbox (ballistics)"
            // "Test", "ArcherMicroTest_E"
            var tests = new[] { "MC1 (1) archer vs archer (britons)" };
            var civs = new[] { Civilization.BRITONS, Civilization.FRANKS };
            var opponents = new[] { "ArcherMicroTest_E" };

            for (int i = 0; i < tests.Length; i++)
            {
                for (int j = 0; j < opponents.Length; j++)
                {
                    var test = tests[i];
                    var civ = civs[i];
                    var file = opponents[j];
                    var wins = 0d;
                    var my_score = 0d;
                    var opponent_score = 0d;

                    for (int k = 0; k < NUM; k++)
                    {
                        Debug.WriteLine($"Running {k}/{NUM} test {test} against {file}");
                        RunTest(test, civ, file, out var me, out var opponent);

                        if (me.Alive)
                        {
                            wins++;
                        }

                        my_score += me.Score;
                        opponent_score += opponent.Score;
                    }

                    wins /= NUM;
                    my_score /= NUM;
                    opponent_score /= NUM;

                    Invoke(() => Message($"TEST {test} against {file} result: {wins:P0} wins, avg score me: {my_score:N0} opponent: {opponent_score:N0}"));
                }
            }

            Thread.Sleep(3000);
            unary.Stop();
            Instance.Kill();
            Instance = null;
        }

        private void RunTest(string scenario, Civilization civilization, string opponent_file, out Player me, out Player opponent)
        {
            var game = new Game()
            {
                GameType = GameType.SCENARIO,
                ScenarioName = scenario,
                MapType = MapType.RANDOM_MAP,
                MapSize = MapSize.TINY,
                Difficulty = Difficulty.HARD,
                StartingResources = StartingResources.STANDARD,
                PopulationLimit = 200,
                RevealMap = RevealMap.NORMAL,
                StartingAge = StartingAge.STANDARD,
                VictoryType = VictoryType.CONQUEST,
                VictoryValue = 0,
                TeamsTogether = true,
                LockTeams = true,
                AllTechs = false,
                Recorded = false
            };

            me = new Player()
            {
                PlayerNumber = 1,
                IsHuman = false,
                AiFile = "Test",
                Civilization = (int)civilization,
                Color = AoE2Lib.Games.Color.COLOR_1,
                Team = Team.NO_TEAM
            };

            opponent = new Player()
            {
                PlayerNumber = 2,
                IsHuman = false,
                AiFile = opponent_file,
                Civilization = (int)civilization,
                Color = AoE2Lib.Games.Color.COLOR_2,
                Team = Team.NO_TEAM
            };

            game.AddPlayer(me);
            game.AddPlayer(opponent);

            Debug.WriteLine("Starting game");
            Instance.StartGame(game);

            while (!game.Finished)
            {
                Thread.Sleep(1000);
            }

            Debug.WriteLine("Done running game, waiting 3 seconds");
            Thread.Sleep(3000);
        }
    }
}
