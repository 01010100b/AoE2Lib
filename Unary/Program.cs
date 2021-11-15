using AoE2Lib;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Unary.Strategy;
using static Unary.Strategy.BuildOrderCommand;

namespace Unary
{
    static class Program
    {
        internal static readonly Log Log = new Log(Path.Combine(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName), "Unary.log"));
        
        public static void Serialize<T>(T obj, string file)
        {
            var serializer = new JsonSerializer
            {
                Formatting = Formatting.Indented
            };
            serializer.Converters.Add(new StringEnumConverter());

            using (var writer = File.CreateText(file))
            using (var json = new JsonTextWriter(writer))
            {
                serializer.Serialize(json, obj);
                writer.Flush();
            }
        }

        public static T Deserialize<T>(string file)
        {
            var serializer = new JsonSerializer
            {
                Formatting = Formatting.Indented
            };
            serializer.Converters.Add(new StringEnumConverter());

            using (var reader = File.OpenText(file))
            using (var json = new JsonTextReader(reader))
            {
                return serializer.Deserialize<T>(json);
            }
        }

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

                //TestStrategy();

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
            finally
            {
                Log.Dispose();
                MessageBox.Show("An unexpected error occurred. Unary will now exit. See the log for details.", "Unexpected error");
            }
        }
    }
}
