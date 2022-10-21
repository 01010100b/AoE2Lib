using AoE2Lib.Bots;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unary.Jobs
{
    internal abstract class ResourceGenerationJob : Job
    {
        public abstract Resource Resource { get; }

        private readonly List<KeyValuePair<TimeSpan, double>> Dropoffs = new();
        private readonly Dictionary<Controller, double> Carried = new();

        public ResourceGenerationJob(Unary unary) : base(unary)
        {
        }

        public double GetRate(TimeSpan span)
        {
            if (Dropoffs.Count < 10)
            {
                return -1;
            }

            var start = Unary.GameState.GameTime - span;
            var first = TimeSpan.MaxValue;
            var last = TimeSpan.MinValue;
            var total = 0d;

            foreach (var dropoff in Dropoffs)
            {
                if (dropoff.Key >= start)
                {
                    if (dropoff.Key < first)
                    {
                        first = dropoff.Key;
                    }

                    if (dropoff.Key > last)
                    {
                        last = dropoff.Key;
                    }

                    total += dropoff.Value;
                }
            }

            if (total > 0)
            {
                var seconds = (last - first).TotalSeconds;
                total /= seconds;
            }

            return total;
        }

        public override sealed void Update()
        {
            foreach (var worker in GetWorkers())
            {
                if (!Carried.ContainsKey(worker))
                {
                    Carried.Add(worker, -1);
                }

                var carry = worker.Unit[ObjectData.CARRY];

                if (carry < Carried[worker])
                {
                    var diff = Carried[worker] - carry;
                    diff /= Math.Max(1, WorkerCount);
                    Dropoffs.Add(new KeyValuePair<TimeSpan, double>(Unary.GameState.GameTime, diff));
                }

                Carried[worker] = carry;
            }

            if (ShouldRareTick(61))
            {
                foreach (var worker in GetWorkers())
                {
                    if (worker.CurrentJob != this)
                    {
                        Carried.Remove(worker);
                    }
                }
            }

            UpdateResourceGeneration();
        }

        protected abstract void UpdateResourceGeneration();
    }
}
