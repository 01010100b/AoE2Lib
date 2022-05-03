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
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GameRunner
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void ButtonDev_Click(object sender, EventArgs e)
        {
            int t = (int)ComboGameType.SelectedItem;
            Debug.WriteLine(t);

            return;
            var process = Process.GetProcessesByName("WK")[0];
            var instance = new AoEInstance(process);

            var game = new Game()
            {
                VictoryType = 1
            };

            var player1 = new Player()
            {
                PlayerNumber = 1,
                IsHuman = false,
                AiFile = "Barbarian",
                Civilization = 1,
                Color = 1,
                Team = 0
            };

            var player2 = new Player()
            {
                PlayerNumber = 2,
                IsHuman = false,
                AiFile = "Promi",
                Civilization = 2,
                Color = 2,
                Team = 0
            };

            game.Players.Add(player1);
            game.Players.Add(player2);

            instance.RunGame(game);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            ComboGameType.DataSource = Enum.GetValues(typeof(GameType));
            ComboMapType.DataSource = Enum.GetValues(typeof(MapType));
            ComboMapSize.DataSource = Enum.GetValues(typeof(MapSize));
            ComboDifficulty.DataSource = Enum.GetValues(typeof(Difficulty));
            ComboStartingResources.DataSource = Enum.GetValues(typeof(StartingResources));
            ComboRevealMap.DataSource = Enum.GetValues(typeof(RevealMap));
            ComboStartingAge.DataSource = Enum.GetValues(typeof(StartingAge));
            ComboVictoryType.DataSource = Enum.GetValues(typeof(VictoryType));

            ComboPopulationCap.Items.Clear();
            for (int i = 25; i <= 250; i += 25)
            {
                ComboPopulationCap.Items.Add(i);
            }
            ComboPopulationCap.SelectedIndex = 0;

            ComboPlayer1Civ.DataSource = Enum.GetValues(typeof(Civilization));
            ComboPlayer2Civ.DataSource = Enum.GetValues(typeof(Civilization));
            ComboPlayer3Civ.DataSource = Enum.GetValues(typeof(Civilization));
            ComboPlayer4Civ.DataSource = Enum.GetValues(typeof(Civilization));
            ComboPlayer5Civ.DataSource = Enum.GetValues(typeof(Civilization));
            ComboPlayer6Civ.DataSource = Enum.GetValues(typeof(Civilization));
            ComboPlayer7Civ.DataSource = Enum.GetValues(typeof(Civilization));
            ComboPlayer8Civ.DataSource = Enum.GetValues(typeof(Civilization));

            ComboPlayer1Team.DataSource = Enum.GetValues(typeof(Team));
            ComboPlayer2Team.DataSource = Enum.GetValues(typeof(Team));
            ComboPlayer3Team.DataSource = Enum.GetValues(typeof(Team));
            ComboPlayer4Team.DataSource = Enum.GetValues(typeof(Team));
            ComboPlayer5Team.DataSource = Enum.GetValues(typeof(Team));
            ComboPlayer6Team.DataSource = Enum.GetValues(typeof(Team));
            ComboPlayer7Team.DataSource = Enum.GetValues(typeof(Team));
            ComboPlayer8Team.DataSource = Enum.GetValues(typeof(Team));

            ComboPlayer1Color.DataSource = Enum.GetValues(typeof(Color));
            ComboPlayer2Color.DataSource = Enum.GetValues(typeof(Color));
            ComboPlayer3Color.DataSource = Enum.GetValues(typeof(Color));
            ComboPlayer4Color.DataSource = Enum.GetValues(typeof(Color));
            ComboPlayer5Color.DataSource = Enum.GetValues(typeof(Color));
            ComboPlayer6Color.DataSource = Enum.GetValues(typeof(Color));
            ComboPlayer7Color.DataSource = Enum.GetValues(typeof(Color));
            ComboPlayer8Color.DataSource = Enum.GetValues(typeof(Color));

            LoadSettings();

            LoadPlayersFromFolder();
        }

        private void LoadSettings()
        {
            var file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.json");
            if (File.Exists(file))
            {
                var settings = JsonSerializer.Deserialize<Settings>(File.ReadAllText(file), new JsonSerializerOptions() { WriteIndented = true });

                TextExe.Text = settings?.Exe;
                TextAiFolder.Text = settings?.AiFolder;
                ComboGameType.SelectedItem = settings?.GameType;
                TextScenario.Text = settings?.Scenario;
                ComboMapType.SelectedItem = settings?.MapType;
                ComboMapSize.SelectedItem = settings?.MapSize;
                ComboDifficulty.SelectedItem = settings?.Difficulty;
                ComboStartingResources.SelectedItem = settings?.StartingResources;
                ComboRevealMap.SelectedItem = settings?.RevealMap;
                ComboStartingAge.SelectedItem = settings?.StartingAge;
                ComboVictoryType.SelectedItem = settings?.VictoryType;
                TextVictoryValue.Text = settings?.VictoryValue.ToString();
                ComboPopulationCap.SelectedItem = settings?.PopulationLimit;
                CheckTeamsTogether.Checked = settings?.TeamsTogether ?? false;
                CheckLockTeams.Checked = settings?.LockTeams ?? false;
                CheckAllTech.Checked = settings?.AllTechs ?? false;
                CheckRecorded.Checked = settings?.Recorded ?? false;
            }
        }

        private void SaveSettings()
        {
            var settings = new Settings()
            {
                Exe = TextExe.Text,
                AiFolder = TextAiFolder.Text,
                GameType = (GameType)ComboGameType.SelectedItem,
                Scenario = TextScenario.Text,
                MapType = (MapType)ComboMapType.SelectedItem,
                MapSize = (MapSize)ComboMapSize.SelectedItem,
                Difficulty = (Difficulty)ComboDifficulty.SelectedItem,
                StartingResources = (StartingResources)ComboStartingResources.SelectedItem,
                RevealMap = (RevealMap)ComboRevealMap.SelectedItem,
                StartingAge = (StartingAge)ComboStartingAge.SelectedItem,
                VictoryType = (VictoryType)ComboVictoryType.SelectedItem,
                VictoryValue = int.Parse(TextVictoryValue.Text),
                PopulationLimit = (int)ComboPopulationCap.SelectedItem,
                TeamsTogether = CheckTeamsTogether.Checked,
                LockTeams = CheckLockTeams.Checked,
                AllTechs = CheckAllTech.Checked,
                Recorded = CheckRecorded.Checked
            };

            var file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.json");
            File.WriteAllText(file, JsonSerializer.Serialize(settings, new JsonSerializerOptions() { WriteIndented = true }));
        }

        private void LoadPlayersFromFolder()
        {
            var names = new List<string>();

            if (Directory.Exists(TextAiFolder.Text))
            {
                foreach (var file in Directory.EnumerateFiles(TextAiFolder.Text, "*.ai"))
                {
                    var player = Path.GetFileNameWithoutExtension(file);
                    names.Add(player);

                    Debug.WriteLine(player);
                }
            }

            names.Sort();

            names.Insert(0, "*Human");
            names.Insert(0, "*Closed");

            var players = new ComboBox[] { ComboPlayer1Name, ComboPlayer2Name, ComboPlayer3Name, ComboPlayer4Name, ComboPlayer5Name, ComboPlayer6Name, ComboPlayer7Name, ComboPlayer8Name };

            foreach (var player in players)
            {
                player.Items.Clear();
                player.Items.AddRange(names.ToArray());
                player.SelectedIndex = 0;
            }

        }

        private void StartGame()
        {
            if (!File.Exists(TextExe.Text))
            {
                throw new Exception("Exe does not exist");
            }
            else if (!Directory.Exists(TextAiFolder.Text))
            {
                throw new Exception("Ai folder does not exist");
            }

            // set up game

            var game = new Game()
            {
                GameType = (int)ComboGameType.SelectedItem,
                ScenarioName = TextScenario.Text,
                MapType = (int)ComboMapType.SelectedItem,
                MapSize = (int)ComboMapSize.SelectedItem,
                Difficulty = (int)ComboDifficulty.SelectedItem,
                StartingResources = (int)ComboStartingResources.SelectedItem,
                PopulationLimit = (int)ComboPopulationCap.SelectedItem,
                RevealMap = (int)ComboRevealMap.SelectedItem,
                StartingAge = (int)ComboStartingAge.SelectedItem,
                VictoryType = (int)ComboVictoryType.SelectedItem,
                VictoryValue = int.Parse(TextVictoryValue.Text),
                TeamsTogether = CheckTeamsTogether.Checked,
                LockTeams = CheckLockTeams.Checked,
                AllTechs = CheckAllTech.Checked,
                Recorded = CheckRecorded.Checked,
            };

            var players = new ComboBox[] { ComboPlayer1Name, ComboPlayer2Name, ComboPlayer3Name, ComboPlayer4Name, ComboPlayer5Name, ComboPlayer6Name, ComboPlayer7Name, ComboPlayer8Name };
            var civs = new ComboBox[] { ComboPlayer1Civ, ComboPlayer2Civ, ComboPlayer3Civ, ComboPlayer4Civ, ComboPlayer5Civ, ComboPlayer6Civ, ComboPlayer7Civ, ComboPlayer8Civ };
            var colors = new ComboBox[] { ComboPlayer1Color, ComboPlayer2Color, ComboPlayer3Color, ComboPlayer4Color, ComboPlayer5Color, ComboPlayer6Color, ComboPlayer7Color, ComboPlayer8Color };
            var teams = new ComboBox[] { ComboPlayer1Team, ComboPlayer2Team, ComboPlayer3Team, ComboPlayer4Team, ComboPlayer5Team, ComboPlayer6Team, ComboPlayer7Team, ComboPlayer8Team };
            
            for (int i = 0; i < players.Length; i++)
            {
                var name = (string)players[i].SelectedItem;

                if (name != "*Closed")
                {
                    var player = new Player() { PlayerNumber = i + 1 };

                    if (name == "*Human")
                    {
                        player.IsHuman = true;
                    }
                    else
                    {
                        player.IsHuman = false;
                        player.AiFile = name;
                    }

                    player.Civilization = (int)civs[i].SelectedItem;
                    player.Color = (int)colors[i].SelectedItem;
                    player.Team = (int)teams[i].SelectedItem;

                    game.Players.Add(player);
                }
            }

            if (game.Players.Count < 2)
            {
                throw new Exception("Need at least 2 players");
            }

            SaveSettings();

            // run on aoe

            var process = Process.Start(TextExe.Text);
            Thread.Sleep(10 * 1000);

            var instance = new AoEInstance(process);
            instance.RunGame(game);
        }

        private void ButtonSetAiFolder_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                var result = fbd.ShowDialog();

                if (result == DialogResult.OK && Directory.Exists(fbd.SelectedPath))
                {
                    TextAiFolder.Text = fbd.SelectedPath;
                    LoadPlayersFromFolder();
                }
            }
        }

        private void ComboGameType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if ((GameType)ComboGameType.SelectedItem == GameType.SCENARIO)
            {
                TextScenario.Enabled = true;
            }
            else
            {
                TextScenario.Text = "";
                TextScenario.Enabled = false;
            }
        }

        private void ComboVictoryType_SelectedIndexChanged(object sender, EventArgs e)
        {
            var selected = (VictoryType)ComboVictoryType.SelectedItem;

            if (selected == VictoryType.SCORE || selected == VictoryType.TIME_LIMIT)
            {
                TextVictoryValue.Enabled = true;
            }
            else
            {
                TextVictoryValue.Text = "0";
                TextVictoryValue.Enabled = false;
            }
        }

        private void ButtonSetExe_Click(object sender, EventArgs e)
        {
            using (var fd = new OpenFileDialog())
            {
                fd.Filter = "exe files|*.exe";
                var result = fd.ShowDialog();

                if (result == DialogResult.OK && File.Exists(fd.FileName))
                {
                    TextExe.Text = fd.FileName;
                }
            }
        }

        private void ButtonStart_Click(object sender, EventArgs e)
        {
            StartGame();
        }
    }
}
