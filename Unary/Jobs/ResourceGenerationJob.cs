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

        public ResourceGenerationJob(Unary unary) : base(unary)
        {
        }

        public override sealed void Update()
        {
            UpdateResourceGeneration();
        }

        protected abstract void UpdateResourceGeneration();
    }
}
