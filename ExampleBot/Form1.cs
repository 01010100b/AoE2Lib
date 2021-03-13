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

namespace ExampleBot
{
    public partial class Form1 : Form
    {
        private AoEInstance Instance { get; set; }
        public Form1()
        {
            InitializeComponent();
        }

        private void ButtonConnect_Click(object sender, EventArgs e)
        {
            var name = TextProcess.Text.Replace(".exe", "");
            var process = Process.GetProcessesByName(name)[0];
            Instance = new AoEInstance(process);
        }

        private void ButtonStart_Click(object sender, EventArgs e)
        {
            var player = int.Parse(TextPlayer.Text);
            var bot = new ExampleBot();
            Instance.StartBot(bot, player);
        }
    }
}
