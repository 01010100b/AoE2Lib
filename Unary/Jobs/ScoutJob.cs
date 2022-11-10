using AoE2Lib.Bots;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unary.Behaviours;

namespace Unary.Jobs
{
    internal class ScoutJob : Job
    {
        public override string Name => "Scouting job";
        public override Position Location => Unary.TownManager.MyPosition;
        public override int MaxWorkers => 1;

        public ScoutJob(Unary unary) : base(unary)
        {
        }

        public override double GetPay(Controller worker)
        {
            if (!worker.HasBehaviour<ScoutingBehaviour>())
            {
                return -1;
            }
            else if (HasWorker(worker))
            {
                return 100;
            }
            else if (Vacancies < 1)
            {
                return -1;
            }
            else
            {
                return 100;
            }
        }
        protected override void Initialize()
        {
        }

        protected override void Update()
        {
            if (Unary.GameState.Map.TryGetTile(Unary.TownManager.MyPosition, out var tile))
            {
                foreach (var worker in GetWorkers())
                {
                    if (worker.TryGetBehaviour<ScoutingBehaviour>(out var behaviour))
                    {
                        behaviour.Focus = tile;
                        behaviour.Radius = 0;
                    }
                }
            }
        }

        protected override void OnClosed()
        {
        }

        protected override void OnWorkerJoining(Controller worker)
        {
        }

        protected override void OnWorkerLeaving(Controller worker)
        {
            if (worker.TryGetBehaviour<ScoutingBehaviour>(out var behaviour))
            {
                behaviour.Focus = null;
                behaviour.Radius = 0;
            }
        }
    }
}
