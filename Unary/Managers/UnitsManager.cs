using AoE2Lib;
using AoE2Lib.Bots;
using AoE2Lib.Bots.GameElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unary.Behaviours;

namespace Unary.Managers
{
    // controllers
    internal class UnitsManager : Manager
    {

        private readonly Dictionary<Unit, Controller> Controllers = new();

        public UnitsManager(Unary unary) : base(unary)
        {

        }

        public bool TryGetController(Unit unit, out Controller controller) => Controllers.TryGetValue(unit, out controller);

        public IEnumerable<Controller> GetControllers() => Controllers.Values;

        protected internal override void Update()
        {
            UpdateControllers();
        }

        private void UpdateControllers()
        {
            foreach (var unit in Unary.GameState.MyPlayer.Units.Where(u => u.Targetable))
            {
                if (!Controllers.ContainsKey(unit))
                {
                    var controller = new Controller(unit, Unary);
                    Controllers.Add(unit, controller);
                }
            }

            var times = ObjectPool.Get(() => new Dictionary<Type, KeyValuePair<int, TimeSpan>>(), x => x.Clear());
            var controllers = ObjectPool.Get(() => new List<Controller>(), x => x.Clear());
            var behaviours = ObjectPool.Get(() => new List<KeyValuePair<Type, KeyValuePair<int, TimeSpan>>>(), x => x.Clear());

            controllers.AddRange(Controllers.Values);

            foreach (var controller in controllers)
            {
                if (controller.Exists)
                {
                    controller.Tick(times);
                }
                else
                {
                    Controllers.Remove(controller.Unit);
                }
            }

            behaviours.AddRange(times);
            behaviours.Sort((a, b) => b.Value.Value.CompareTo(a.Value.Value));

            foreach (var behaviour in behaviours)
            {
                Unary.Log.Info($"{behaviour.Key.Name} ran {behaviour.Value.Key} times for a total of {behaviour.Value.Value.TotalMilliseconds:N2} ms");
            }

            ObjectPool.Add(times);
            ObjectPool.Add(controllers);
            ObjectPool.Add(behaviours);
        }
    }
}
