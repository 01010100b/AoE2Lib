using AoE2Lib.Bots;
using AoE2Lib.Bots.GameElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unary.Behaviours;

namespace Unary.Jobs
{
    internal abstract class ResourceGenerationJob : Job
    {
        public readonly Controller Dropsite;
        public abstract Resource Resource { get; }
        public override Position Location => Dropsite.Unit.Position;

        private readonly List<KeyValuePair<TimeSpan, double>> Dropoffs = new();
        private readonly Dictionary<Controller, int> Carried = new();

        public ResourceGenerationJob(Unary unary, Controller dropsite) : base(unary)
        {
            Dropsite = dropsite;
        }

        public double GetRate()
        {
            var span = TimeSpan.FromMinutes(5);
            
            if (Dropoffs.Count < 10 || WorkerCount == 0)
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

            if (last <= first)
            {
                return -1;
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
            if (WorkerCount == 0)
            {
                Dropoffs.Clear();
                Carried.Clear();
            }

            if (!Dropsite.CanControl)
            {
                Close();

                return;
            }

            foreach (var worker in GetWorkers())
            {
                if (!Carried.ContainsKey(worker))
                {
                    Carried.Add(worker, worker.Unit[ObjectData.CARRY]);
                }

                var carry = worker.Unit[ObjectData.CARRY];

                if (carry < Carried[worker])
                {
                    var diff = (double)Carried[worker] - carry;
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
