using AoE2Lib.Bots;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unary.Jobs
{
    internal class EatingJob : ResourceGenerationJob
    {
        public override Resource Resource => Resource.FOOD;
        public override string Name => $"Eating at {Location}";
        public override Position Location => throw new NotImplementedException();

        public EatingJob(Unary unary) : base(unary)
        {
        }

        public override double GetPay(Controller worker)
        {
            throw new NotImplementedException();
        }

        protected override void OnWorkerJoining(Controller worker)
        {
            throw new NotImplementedException();
        }

        protected override void OnWorkerLeaving(Controller worker)
        {
            throw new NotImplementedException();
        }

        protected override void UpdateResourceGeneration()
        {
            throw new NotImplementedException();
        }
    }
}
