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

namespace Unary
{
    static class Program
    {
        public static string Folder => AppDomain.CurrentDomain.BaseDirectory;
        internal static readonly Log Log = new(Path.Combine(Folder, "Unary.log"));
        
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
            var unary = new Unary(null);
            return;
            try
            {
                AppDomain.CurrentDomain.UnhandledException += (sender, e) => Handle(e.ExceptionObject);
                Application.ThreadException += (sender, e) => Handle(e.Exception);

                Log.Level = Log.LEVEL_DEBUG;
                Log.Info($"Using AoE2Lib {typeof(AoEInstance).Assembly.GetName().Version}");
                Log.Info($"Started Unary {typeof(Program).Assembly.GetName().Version}");
                Log.Info($"Directory: {AppDomain.CurrentDomain.BaseDirectory}");

                Application.SetHighDpiMode(HighDpiMode.SystemAware);
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new FormUnary());
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
                //MessageBox.Show("An unexpected error occurred. Unary will now exit. See the log for details.", "Unexpected error");
            }
        }
    }
}
