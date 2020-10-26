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
using System.Threading.Tasks;
using System.Windows.Forms;

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
        }
    }
}
