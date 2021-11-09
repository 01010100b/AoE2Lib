using AoE2Lib;
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

                TestStrategy();

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

        private static void TestStrategy()
        {
            var gatherers = new[]
            {
                Resource.FOOD, Resource.FOOD, Resource.FOOD, Resource.FOOD, Resource.FOOD, Resource.FOOD,
                Resource.WOOD, Resource.WOOD, Resource.WOOD, Resource.WOOD,
                Resource.FOOD, Resource.FOOD, Resource.FOOD, Resource.FOOD, Resource.FOOD, Resource.FOOD, Resource.FOOD, Resource.FOOD, Resource.FOOD,
                Resource.WOOD, Resource.WOOD, Resource.WOOD, Resource.WOOD,
                Resource.GOLD, Resource.GOLD, Resource.GOLD, Resource.GOLD, Resource.GOLD
            };

            var strat = new Strategy
            {
                Name = "FC Knights",
            };

            strat.Gatherers.AddRange(gatherers);
            strat.ExtraFoodPercentage = 50;
            strat.ExtraWoodPercentage = 5;
            strat.ExtraGoldPercentage = 40;
            strat.ExtraStonePercentage = 5;

            strat.BuildOrder.Add(new BuildOrderCommand() { Type = BuildOrderCommandType.RESEARCH, Id = 101 }); // feudal age
            strat.BuildOrder.Add(new BuildOrderCommand() { Type = BuildOrderCommandType.UNIT, Id = 12 }); // barracks
            strat.BuildOrder.Add(new BuildOrderCommand() { Type = BuildOrderCommandType.RESEARCH, Id = 22 }); // loom
            strat.BuildOrder.Add(new BuildOrderCommand() { Type = BuildOrderCommandType.UNIT, Id = 101 }); // stable
            strat.BuildOrder.Add(new BuildOrderCommand() { Type = BuildOrderCommandType.UNIT, Id = 103 }); // blacksmith
            strat.BuildOrder.Add(new BuildOrderCommand() { Type = BuildOrderCommandType.RESEARCH, Id = 102 }); // castle age
            strat.BuildOrder.Add(new BuildOrderCommand() { Type = BuildOrderCommandType.RESEARCH, Id = 202 }); // double bit axe
            strat.BuildOrder.Add(new BuildOrderCommand() { Type = BuildOrderCommandType.RESEARCH, Id = 14 }); // horse collar
            strat.BuildOrder.Add(new BuildOrderCommand() { Type = BuildOrderCommandType.UNIT, Id = 101 }); // second stable
            strat.BuildOrder.Add(new BuildOrderCommand() { Type = BuildOrderCommandType.RESEARCH, Id = 203 }); // bow saw

            strat.PrimaryUnits.Add(38); // knight

            var str = Strategy.Serialize(strat);
            var file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Strategies", strat.Name + ".json");
            File.WriteAllText(file, str);

            Debug.WriteLine(str);
            
        }
    }
}
