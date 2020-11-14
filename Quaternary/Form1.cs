using AoE2Lib;
using AoE2Lib.Bots;
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

namespace Quaternary
{
    public partial class Form1 : Form
    {
        private BotManager BotManager { get; set; }

        public Form1()
        {
            InitializeComponent();
        }

        private void ButtonStart_Click(object sender, EventArgs e)
        {
            ButtonStart.Enabled = false;
            Refresh();

            var process = Process.GetProcessesByName("AoE2DE_s")[0];
            var instance = new GameInstance(process);

            BotManager = new BotManager(instance);
            BotManager.RegisterBot<Quaternary>();

            BotManager.Start();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            BotManager?.Exit();
        }

        private void ButtonExit_Click(object sender, EventArgs e)
        {
            ButtonExit.Enabled = false;
            Refresh();
            Close();
        }
    }
}
