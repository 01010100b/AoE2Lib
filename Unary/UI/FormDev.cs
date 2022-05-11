using AoE2Lib.Bots;
using AoE2Lib.Games;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
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
    public partial class FormDev : Form
    {
        private readonly UISettings UISettings;
        private readonly Ladder Ladder;

        public FormDev()
        {
            InitializeComponent();

            var file = Path.Combine(Program.Folder, "uisettings.json");
            UISettings = Program.Deserialize<UISettings>(file);
            Ladder = new Ladder();
        }

        private void Message(string message)
        {
            Invoke(() =>
            {
                Program.Log.Info($"UI: {message}");

                var lines = TextMessages.Lines.ToList();
                lines.Add($"{DateTime.Now}: {message}");
                TextMessages.Lines = lines.ToArray();
            });
        }

        private void ButtonScenarios_Click(object sender, EventArgs e)
        {
            const int GAMES_PER_SCENARIO = 20;

            ButtonScenarios.Enabled = false;
            ButtonLadder.Enabled = false;
            Refresh();

            var thread = new Thread(() =>
            {
                Message("start running scenarios...");

                var exe = UISettings.ExePath;
                var opponents = new List<string>() { "Null", "ArcherMicroTest_E" };
                var scenarios = new List<Scenario>();

                for (int i = 0; i < GAMES_PER_SCENARIO; i++)
                {
                    foreach (var opponent in opponents)
                    {
                        foreach (var scenario in Scenarios.GetCombatRangedTests())
                        {
                            scenario.OpponentAiFile = opponent;
                            scenarios.Add(scenario);
                        }
                    }
                }

                var games = new Dictionary<Game, Scenario>();
                var runs = new List<KeyValuePair<Game, Dictionary<int, Bot>>>();

                foreach (var scenario in scenarios)
                {
                    var game = scenario.CreateGame("Unary");
                    var unary = new Unary();
                    var dict = new Dictionary<int, Bot>() { { 1, unary } };

                    games.Add(game, scenario);
                    runs.Add(new KeyValuePair<Game, Dictionary<int, Bot>>(game, dict));
                }

                InstanceRunner.RunGames(exe, runs);

                Message("done running scenarios");

                var scores = new Dictionary<string, double>();
                var counts = new Dictionary<string, int>();

                foreach (var game in games)
                {
                    var name = game.Value.ToString();
                    var score = game.Value.GetScore(game.Key);

                    if (!scores.ContainsKey(name))
                    {
                        scores.Add(name, 0);
                        counts.Add(name, 0);
                    }

                    scores[name] += score;
                    counts[name]++;
                }

                foreach (var score in scores)
                {
                    var name = score.Key;
                    var result = score.Value / counts[name];

                    Message($"Test {name}: {result:P0}");
                }
            });

            thread.IsBackground = true;
            thread.Start();
        }

        private void ButtonLadder_Click(object sender, EventArgs e)
        {
            ButtonScenarios.Enabled = false;
            ButtonLadder.Enabled = false;
            ButtonStopLadder.Enabled = true;
            Refresh();

            var thread = new Thread(() =>
            {
                Message("Start running ladder...");

                var exe = UISettings.ExePath;
                Ladder.Run(exe);

                Message("Stopped running ladder");
            });

            thread.IsBackground = true;
            thread.Start();
        }

        private void ButtonStopLadder_Click(object sender, EventArgs e)
        {
            ButtonStopLadder.Enabled = false;
            Refresh();

            Ladder.Stop();
            Message("Sending stop command to ladder...");
        }
    }
}
