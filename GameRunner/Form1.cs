using AoE2Lib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
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
        }
    }
}
