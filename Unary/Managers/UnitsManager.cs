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

        private readonly Dictionary<Unit, Controller> Controllers = new();
        private readonly HashSet<Job> Jobs = new();

        public UnitsManager(Unary unary) : base(unary)
        {

        }

        public bool TryGetController(Unit unit, out Controller controller) => Controllers.TryGetValue(unit, out controller);

        public IEnumerable<Controller> GetControllers() => Controllers.Values;

        public IEnumerable<Job> GetJobs() => Jobs;

        public void AddJob(Job job)
        {
            Jobs.Add(job);
            Unary.Log.Info($"Created job {job}");
        }

        public void RemoveJob(Job job)
        {
            Jobs.Remove(job);
            Unary.Log.Info($"Removed job {job}");
        }

        protected internal override void Update()
        {
            UpdateJobs();
            UpdateControllers();
        }

        private void UpdateJobs()
        {
            var times = ObjectPool.Get(() => new Dictionary<Type, KeyValuePair<int, TimeSpan>>(), x => x.Clear());
            var jobs = ObjectPool.Get(() => new List<KeyValuePair<Type, KeyValuePair<int, TimeSpan>>>(), x => x.Clear());
            var sw = new Stopwatch();

            foreach (var job in Jobs)
            {
                var type = job.GetType();

                if (!times.ContainsKey(type))
                {
                    times.Add(type, new KeyValuePair<int, TimeSpan>(0, TimeSpan.Zero));
                }

                sw.Restart();
                job.Update();
                var time = sw.Elapsed;
                var kvp = times[type];
                times[type] = new KeyValuePair<int, TimeSpan>(kvp.Key + 1, kvp.Value + time);
            }

            jobs.AddRange(times);
            jobs.Sort((a, b) => b.Value.Value.CompareTo(a.Value.Value));
            foreach (var job in jobs)
            {
                Unary.Log.Info($"{job.Key.Name} ran {job.Value.Key} times for a total of {job.Value.Value.TotalMilliseconds:N2} ms");
            }

            ObjectPool.Add(times);
            ObjectPool.Add(jobs);
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
    }
}
