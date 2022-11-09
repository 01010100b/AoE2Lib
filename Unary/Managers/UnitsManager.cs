using AoE2Lib;
using AoE2Lib.Bots;
using AoE2Lib.Bots.GameElements;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.Design.Behavior;
using Unary.Behaviours;
using Unary.Jobs;

namespace Unary.Managers
{
    // controllers
    internal class UnitsManager : Manager
    {
        public IEnumerable<Controller> Gatherers => Unary.GetCached(GetGatherers);
        public IEnumerable<Unit> Enemies => Unary.GetCached(GetEnemies);

        private readonly Dictionary<Unit, Controller> Controllers = new();

        public UnitsManager(Unary unary) : base(unary)
        {

        }

        public bool TryGetController(Unit unit, out Controller controller) => Controllers.TryGetValue(unit, out controller);

        public IEnumerable<Controller> GetControllers() => Controllers.Values;

        protected internal override void Update()
        {
            var actions = ObjectPool.Get(() => new List<Action>(), x => x.Clear());
            actions.Add(UpdateControllers);

            Run(actions);
            ObjectPool.Add(actions);
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
                if (controller.CanControl)
                {
                    controller.Tick(times);
                }
                else
                {
                    Controllers.Remove(controller.Unit);

                    if (controller.CurrentJob != null)
                    {
                        controller.CurrentJob.Leave(controller);
                    }
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

        private List<Controller> GetGatherers()
        {
            var gatherers = ObjectPool.Get(() => new List<Controller>(), x => x.Clear());

            foreach (var controller in GetControllers())
            {
                if (controller.HasBehaviour<GatheringBehaviour>())
                {
                    gatherers.Add(controller);
                }
            }

            return gatherers;
        }

        private List<Unit> GetEnemies()
        {
            var targets = ObjectPool.Get(() => new List<Unit>(), x => x.Clear());

            foreach (var unit in Unary.GameState.CurrentEnemies.SelectMany(p => p.Units).Where(u => u.Targetable))
            {
                targets.Add(unit);
            }

            return targets;
        }
    }
}
