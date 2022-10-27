using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unary.Jobs;

namespace Unary.Managers
{
    internal class MilitaryManager : Manager
    {
        private readonly DefendTownJob DefendTownJob;

        public MilitaryManager(Unary unary) : base(unary)
        {
            DefendTownJob = new(unary);
            unary.UnitsManager.AddJob(DefendTownJob);
        }

        protected internal override void Update()
        {
            //throw new NotImplementedException();
        }
    }
}
