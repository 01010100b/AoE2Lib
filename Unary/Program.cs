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
            try
            {
                AppDomain.CurrentDomain.UnhandledException += (sender, e) => Handle(e.ExceptionObject);
                Application.ThreadException += (sender, e) => Handle(e.Exception);

                Log.Level = Log.LEVEL_DEBUG;
                Log.Info($"Using AoE2Lib {typeof(AoEInstance).Assembly.GetName().Version}");
                Log.Info($"Started Unary {typeof(Program).Assembly.GetName().Version}");
                Log.Info($"Directory: {Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName)}");

                Application.SetHighDpiMode(HighDpiMode.SystemAware);
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new Form1());
            }
            catch (Exception e)
            {
                Handle(e);
            }
        }

        private static void Handle(object exc)
        {
            Log.Error("Unhandled exception");

            try
            {
                if (exc is Exception e)
                {
                    Log.Exception(e);
                }
                else
                {
                    Log.Error(exc);
                }
            }
            catch
            {

            }

            MessageBox.Show("An unexpected error occurred. Unary will now exit. See the log for details.", "Unexpected error");
        }
    }
}
