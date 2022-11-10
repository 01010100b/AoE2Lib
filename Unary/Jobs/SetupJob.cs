using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unary.Jobs
{
    internal class SetupJob : ManagementJob
    {
        public override string Name => "Initial setup";

        public SetupJob(Unary unary) : base(unary)
        {
        }

        protected override void Initialize()
        {
            _ = new ScoutJob(Unary);
            _ = new BuildersManagementJob(Unary);
            _ = new FarmingManagementJob(Unary);
            _ = new ConstructionManagementJob(Unary);
            _ = new DropsiteManagementJob(Unary);
        }

        protected override void OnClosed()
        {
        }

        protected override void Update()
        {
        }
    }
}
