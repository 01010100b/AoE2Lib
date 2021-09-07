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

namespace Unary
{
    public partial class Form1 : Form
    {
        private AoEInstance Instance { get; set; }
        private readonly Dictionary<int, Unary> Players = new Dictionary<int, Unary>();

        public Form1()
        {
            InitializeComponent();
        }

        private void ButtonConnect_Click(object sender, EventArgs e)
        {
            var name = TextProcess.Text;
            Message($"Connecting to process {name}...");

            try
            {
                var process = Process.GetProcessesByName(name)[0];
                Instance = new AoEInstance(process);
                Message($"Connected to process {process.Id}");
            }
            catch (Exception ex)
            {
                Message($"Failed to find/connect process with name {name}");
                Program.Log.Exception(ex);

                throw new Exception($"Failed to find process with name {name}", ex);
            }

            ButtonConnect.Enabled = false;
            TextProcess.Enabled = false;
            ButtonStart.Enabled = true;
            NumericPlayer.Enabled = true;
        }

        private void ButtonStart_Click(object sender, EventArgs e)
        {
            var player = (int)NumericPlayer.Value;
            Message($"Starting for player {player}...");

            if (Players.ContainsKey(player))
            {
                Message($"Player {player} is already playing.");

                return;
            }

            var bot = new Unary();
            Instance.StartBot(bot, player);
            Players.Add(player, bot);
            Message($"Started player {player}");
            ButtonStop.Enabled = true;
        }

        private void ButtonStop_Click(object sender, EventArgs e)
        {
            ButtonStop.Enabled = false;
            Cursor = Cursors.WaitCursor;

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

            Cursor = Cursors.Default;
        }

        private void ButtonDev_Click(object sender, EventArgs e)
        {
            var form = new FormSimulations();
            form.Show();
        }

        private void Message(string message)
        {
            Program.Log.Debug(message);

            var lines = TextMessages.Lines.ToList();
            lines.Add(message);
            TextMessages.Lines = lines.ToArray();
        }
    }
}
