using AoE2Lib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unary.Jobs;

namespace Unary.Managers
{
    internal class JobManager : Manager
    {
        private readonly HashSet<Job> Jobs = new();

        public JobManager(Unary unary) : base(unary)
        {
        }

        public IEnumerable<Job> GetJobs() => Jobs;

        public T GetSingleJob<T>() where T : Job
        {
            return Jobs.OfType<T>().Single();
        }

        internal void AddJob(Job job)
        {
            Jobs.Add(job);
            Unary.Log.Info($"Created job {job}");
        }

        internal void RemoveJob(Job job)
        {
            Jobs.Remove(job);
            Unary.Log.Info($"Removed job {job}");
        }

        protected internal override void Update()
        {
            var actions = ObjectPool.Get(() => new List<Action>(), x => x.Clear());
            actions.Add(UpdateJobs);
            Run(actions);
            ObjectPool.Add(actions);
        }

        private void UpdateJobs()
        {
            var times = ObjectPool.Get(() => new Dictionary<Type, KeyValuePair<int, TimeSpan>>(), x => x.Clear());
            var jobs = ObjectPool.Get(() => new List<Job>(), x => x.Clear());
            jobs.AddRange(Jobs);
            var sw = new Stopwatch();

            foreach (var job in jobs)
            {
                var type = job.GetType();

                if (!times.ContainsKey(type))
                {
                    times.Add(type, new KeyValuePair<int, TimeSpan>(0, TimeSpan.Zero));
                }

                sw.Restart();
                job.Tick();
                var time = sw.Elapsed;
                var kvp = times[type];
                times[type] = new KeyValuePair<int, TimeSpan>(kvp.Key + 1, kvp.Value + time);
            }

            foreach (var job in times.OrderByDescending(x => x.Value.Value))
            {
                Unary.Log.Info($"{job.Key.Name} ran {job.Value.Key} times for a total of {job.Value.Value.TotalMilliseconds:N2} ms");
            }

            ObjectPool.Add(times);
            ObjectPool.Add(jobs);
        }
    }
}
