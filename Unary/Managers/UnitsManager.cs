using AoE2Lib;
using AoE2Lib.Bots.GameElements;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unary.Algorithms;
using Unary.UnitControllers;

namespace Unary.Managers
{
    class UnitsManager : Manager
    {
        private readonly Dictionary<Unit, UnitController> Units = new();
        
        public UnitsManager(Unary unary) : base(unary)
        {

        }

        public List<T> GetControllers<T>() where T : UnitController
        {
            return Units.Values.OfType<T>().Cast<T>().ToList();
        }

        public void SetController(Unit unit, UnitController controller)
        {
            if (controller == null)
            {
                Units.Remove(unit);
            }
            else
            {
                Units[unit] = controller;
            }
        }

        internal override void Update()
        {
            UpdateControllers();
        }

        private void UpdateControllers()
        {
            var sw = new Stopwatch();
            sw.Start();

            foreach (var unit in Unary.GameState.MyPlayer.Units.Where(u => u.Targetable))
            {
                if (!Units.ContainsKey(unit))
                {
                    Units[unit] = new IdlerController(unit, Unary);
                }
            }

            Unary.Log.Info($"Creating unit controllers took {sw.ElapsedMilliseconds} ms");

            var times = new Dictionary<Type, TimeSpan>();
            var counts = new Dictionary<Type, int>();

            var controllers = GetControllers<UnitController>();
            foreach (var controller in controllers)
            {
                sw.Restart();

                if (controller.Unit.Targetable)
                {
                    controller.Update();
                }
                else
                {
                    Units.Remove(controller.Unit);
                }

                var type = controller.GetType();
                
                if (!times.ContainsKey(type))
                {
                    times.Add(type, TimeSpan.Zero);
                    counts.Add(type, 0);
                }

                counts[type]++;
                times[type] += sw.Elapsed;
            }

            var types = times.Keys.ToList();
            types.Sort((a, b) => a.Name.CompareTo(b.Name));

            foreach (var type in types)
            {
                var time = times[type];
                var count = counts[type];
                var tpc = time / count;

                Unary.Log.Info($"{type.Name} has {count} count and took {time.TotalMilliseconds:F0} ms ({tpc.TotalMilliseconds:F2} each)");
            }
        }
    }
}
