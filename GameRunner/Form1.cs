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
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GameRunner
{
    public partial class Form1 : Form
    {
        private readonly List<string> Players = new List<string>() { "*Closed", "*Human" };

        public Form1()
        {
            InitializeComponent();
        }

        private void ButtonDev_Click(object sender, EventArgs e)
        {
            var process = Process.GetProcessesByName("WK")[0];
            var instance = new AoEInstance(process);

            var game = new Game()
            {
                VictoryType = 1
            };

            var player1 = new Game.Player()
            {
                PlayerNumber = 1,
                IsHuman = false,
                AiFile = "Barbarian",
                Civilization = 1,
                Color = 1,
                Team = 0
            };

            var player2 = new Game.Player()
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

            instance.Run(game);
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

            LoadPlayersFromFolder(null);
        }

        private void LoadPlayersFromFolder(string folder)
        {
            Players.Clear();

            if (Directory.Exists(folder))
            {
                foreach (var file in Directory.EnumerateFiles(folder, "*.ai"))
                {
                    var player = Path.GetFileNameWithoutExtension(file);
                    Players.Add(player);

                    Debug.WriteLine(player);
                }

                Players.Sort();
            }
            
            Players.Insert(0, "*Human");
            Players.Insert(0, "*Closed");

            ComboPlayer1Name.DataSource = null;
            ComboPlayer1Name.DataSource = Players;
            ComboPlayer2Name.DataSource = null;
            ComboPlayer2Name.DataSource = Players;
            ComboPlayer3Name.DataSource = null;
            ComboPlayer3Name.DataSource = Players;
            ComboPlayer4Name.DataSource = null;
            ComboPlayer4Name.DataSource = Players;
            ComboPlayer5Name.DataSource = null;
            ComboPlayer5Name.DataSource = Players;
            ComboPlayer6Name.DataSource = null;
            ComboPlayer6Name.DataSource = Players;
            ComboPlayer7Name.DataSource = null;
            ComboPlayer7Name.DataSource = Players;
            ComboPlayer8Name.DataSource = null;
            ComboPlayer8Name.DataSource = Players;
        }

        private void ButtonSetAiFolder_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                var result = fbd.ShowDialog();

                if (result == DialogResult.OK && Directory.Exists(fbd.SelectedPath))
                {
                    TextAiFolder.Text = fbd.SelectedPath;
                    LoadPlayersFromFolder(fbd.SelectedPath);
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
                TextVictoryValue.Text = "";
                TextVictoryValue.Enabled = false;
            }
        }
    }
}
