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
using Unary.UnitControllers.BuildingControllers;
using Unary.UnitControllers.MilitaryControllers;
using Unary.UnitControllers.VillagerControllers;

namespace Unary.Managers
{
    class OldUnitsManager : Manager
    {
        private readonly Dictionary<Unit, UnitController> ControlledUnits = new();
        
        public OldUnitsManager(Unary unary) : base(unary)
        {

        }

        public List<T> GetControllers<T>() where T : UnitController
        {
            return ControlledUnits.Values.OfType<T>().Cast<T>().ToList();
        }

        public void SetController(Unit unit, UnitController controller)
        {
            if (controller == null)
            {
                ControlledUnits.Remove(unit);
            }
            else
            {
                ControlledUnits[unit] = controller;
            }
        }

        internal override void Update()
        {
            CreateControllers();
            UpdateControllers();
        }

        private void CreateControllers()
        {
            foreach (var unit in Unary.GameState.MyPlayer.Units.Where(u => u.Targetable))
            {
                if (!ControlledUnits.ContainsKey(unit))
                {
                    new IdlerController(unit, Unary);
                }
            }
        }

        private void UpdateControllers()
        {
            var sw = new Stopwatch();
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
                    ControlledUnits.Remove(controller.Unit);
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
