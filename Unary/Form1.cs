using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Protos;
using Protos.Expert;
using Protos.Expert.Action;
using Protos.Expert.Fact;
using Reloaded.Injector;
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
using static Protos.AIModuleAPI;
using static Protos.Expert.ExpertAPI;

namespace Unary
{
    public partial class Form1 : Form
    {
        private bool Connected { get; set; } = false;
        private Bot Bot { get; set; }

        public Form1()
        {
            InitializeComponent();
        }

        private void ButtonTest_Click(object sender, EventArgs e)
        {
            ButtonTest.Enabled = false;
            Refresh();

            if (!Connected)
            {
                var dll = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "aimodule.dll");
                var process = Process.GetProcessesByName("AoE2DE_s")[0];

                using (var injector = new Injector(process))
                {
                    injector.Inject(dll);
                }

                Thread.Sleep(3000);

                Connected = true;
            }
            
            if (Bot == null)
            {
                Bot = new Bot();
            }

            Bot.Start(1);

            ButtonStop.Enabled = true;
        }

        private void ButtonStop_Click(object sender, EventArgs e)
        {
            ButtonStop.Enabled = false;
            Refresh();

            Bot?.Stop();

            ButtonTest.Enabled = true;
        }
    }
}
