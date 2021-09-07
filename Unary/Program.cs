using AoE2Lib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Unary
{
    static class Program
    {
        internal static readonly Log Log = new Log(Path.Combine(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName), "Unary.log"));
        
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Log.Level = Log.DEBUG;
            Log.Info($"Started Unary {typeof(Program).Assembly.GetName().Version}");

            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
