using AoE2Lib.Bots;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unary.Behaviours;

namespace Unary.Jobs
{
    internal abstract class ManagementJob : Job
    {
        public override int MaxWorkers => 0;
        public override Position Location => Unary.TownManager.MyPosition;

        protected ManagementJob(Unary unary) : base(unary)
        {
        }

        public override double GetPay(Controller worker) => -1;

        protected override void OnWorkerJoining(Controller worker)
        {
        }

        protected override void OnWorkerLeaving(Controller worker)
        {
        }
    }
}
