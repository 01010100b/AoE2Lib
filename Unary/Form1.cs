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
        public Form1()
        {
            InitializeComponent();
        }

        private void ButtonTest_Click(object sender, EventArgs e)
        {
            ButtonTest.Enabled = false;
            Refresh();

            var dll = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "aimodule.dll");
            var process = Process.GetProcessesByName("AoE2DE_s")[0];

            using (var injector = new Injector(process))
            {
                injector.Inject(dll);
            }

            Thread.Sleep(3000);

            var channel = new Channel("localhost:37412", ChannelCredentials.Insecure);
            var module_api = new AIModuleAPIClient(channel);
            var expert_api = new ExpertAPIClient(channel);

            var commands = new List<IMessage>()
            {
                new UpGetFact() { FactId = 25, FactParam = 83, GoalId = 99 },
                new Goal() { GoalId = 99 }
            };

            var commandlist = new CommandList();
            commandlist.PlayerNumber = 1;
            foreach (var command in commands)
            {
                commandlist.Commands.Add(Any.Pack(command));
            }

            var resultlist = expert_api.ExecuteCommandList(commandlist);
            Debug.WriteLine(resultlist);

            var result = resultlist.Results[1];
            Debug.WriteLine(result);
        }
    }
}
