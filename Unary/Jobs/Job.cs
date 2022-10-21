using AoE2Lib.Bots;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unary.Jobs
{
    internal abstract class Job
    {
        public abstract string Name { get; }
        public abstract Position Location { get; }
        public int WorkerCount => Workers.Count;

        protected readonly Unary Unary;
        private readonly HashSet<Controller> Workers = new();

        public Job(Unary unary)
        {
            Unary = unary;
        }

        public abstract double GetPay(Controller worker);
        public abstract void Update();
        protected abstract void OnWorkerJoining(Controller worker);
        protected abstract void OnWorkerLeaving(Controller worker);

        public IEnumerable<Controller> GetWorkers() => Workers;

        public bool HasWorker(Controller worker) => Workers.Contains(worker);

        public void Join(Controller worker)
        {
            worker.CurrentJob = this;
            OnWorkerJoining(worker);
            Workers.Add(worker);
            Unary.Log.Info($"Unit {worker.Unit.Id} taking job {Name}");
        }

        public void Leave(Controller worker)
        {
            worker.CurrentJob = null;
            OnWorkerLeaving(worker);
            Workers.Remove(worker);
            Unary.Log.Info($"Unit {worker.Unit.Id} leaving job {Name}");
        }

        public void Close()
        {
            foreach (var worker in Workers)
            {
                worker.CurrentJob = null;
                OnWorkerLeaving(worker);
            }

            Workers.Clear();
            Unary.Log.Info($"Job {Name} closed.");
        }

        protected bool ShouldRareTick(int rate) => Unary.ShouldRareTick(this, rate);
    }
}
