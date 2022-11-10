using AoE2Lib.Bots;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unary.Behaviours;

namespace Unary.Jobs
{
    internal abstract class Job
    {
        public abstract string Name { get; }
        public abstract Position Location { get; }
        public abstract int MaxWorkers { get; }
        public int WorkerCount => Workers.Count;
        public int Vacancies => MaxWorkers - WorkerCount;

        protected readonly Unary Unary;

        private readonly HashSet<Controller> Workers = new();
        private bool Initialized { get; set; } = false;

        public Job(Unary unary)
        {
            Unary = unary;
            unary.JobManager.AddJob(this);
        }

        public abstract double GetPay(Controller worker);
        protected abstract void Initialize();
        protected abstract void Update();
        protected abstract void OnWorkerJoining(Controller worker);
        protected abstract void OnWorkerLeaving(Controller worker);
        protected abstract void OnClosed();

        public void Tick()
        {
            if (!Initialized)
            {
                Initialize();
                Initialized = true;
            }

            Update();
        }

        public IEnumerable<Controller> GetWorkers() => Workers;

        public bool HasWorker(Controller worker) => Workers.Contains(worker);

        public void Join(Controller worker)
        {
            if (worker.CurrentJob != null)
            {
                worker.CurrentJob.Leave(worker);
            }

            worker.CurrentJob = this;
            OnWorkerJoining(worker);
            Workers.Add(worker);
            Unary.Log.Info($"Unit {worker.Name} taking job {Name}");
        }

        public void Leave(Controller worker)
        {
            worker.CurrentJob = null;
            OnWorkerLeaving(worker);
            Workers.Remove(worker);
            Unary.Log.Info($"Unit {worker.Name} leaving job {Name}");
        }

        public void Close()
        {
            foreach (var worker in Workers)
            {
                worker.CurrentJob = null;
                OnWorkerLeaving(worker);
            }

            Workers.Clear();
            OnClosed();
            Unary.Log.Info($"Job {Name} closed.");
            Unary.JobManager.RemoveJob(this);
        }

        protected bool ShouldRareTick(int rate) => Unary.ShouldRareTick(this, rate);
    }
}
