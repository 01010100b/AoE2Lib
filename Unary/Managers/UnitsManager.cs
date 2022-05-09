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
        //private const int GROUPS = 10;

        public IEnumerable<Controller> ConstructionSpots => Controllers.Values.Where(c =>
        {
            var b = c.GetBehaviour<ConstructionSpotBehaviour>();

            if (b == null)
            {
                return false;
            }
            else
            {
                return b.MaxBuilders > 0;
            }
        });
        public IEnumerable<Controller> EatingSpots => Controllers.Values.Where(c =>
        {
            var b = c.GetBehaviour<EatingSpotBehaviour>();

            if (b == null)
            {
                return false;
            }
            else
            {
                return b.Target != null;
            }
        });
        public IEnumerable<Controller> Combatants => Unary.GetCached(GetCombatants);

        private readonly Dictionary<Unit, Controller> Controllers = new();

        public UnitsManager(Unary unary) : base(unary)
        {

        }

        public bool TryGetController(Unit unit, out Controller controller) => Controllers.TryGetValue(unit, out controller);

        public IEnumerable<Controller> GetControllers() => Controllers.Values;

        internal override void Update()
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

            var times = new Dictionary<Type, KeyValuePair<int, TimeSpan>>();

            foreach (var controller in Controllers.Values.ToList())
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

            var behaviours = times.ToList();
            behaviours.Sort((a, b) => b.Value.Value.CompareTo(a.Value.Value));

            foreach (var behaviour in behaviours)
            {
                Unary.Log.Info($"{behaviour.Key.Name} ran {behaviour.Value.Key} times for a total of {behaviour.Value.Value.TotalMilliseconds:N2} ms");
            }
        }

        private List<Controller> GetCombatants()
        {
            var combatants = ObjectPool.Get(() => new List<Controller>(), x => x.Clear());

            foreach (var controller in GetControllers())
            {
                if (controller.TryGetBehaviour<CombatBehaviour>(out var behaviour))
                {
                    if (behaviour.TimeSinceLastPerformed < TimeSpan.FromSeconds(10))
                    {
                        combatants.Add(controller);
                    }
                }
            }

            return combatants;
        }
    }
}
