using AoE2Lib.Bots.GameElements;
using System;
using System.Collections.Generic;
using System.Text;
using Unary.Managers;

namespace Unary.Operations
{
    class GatherOperation : Operation
    {
        public readonly List<Unit> Resources = new List<Unit>();
        public Unit Dropsite { get; set; }

        public GatherOperation(OperationsManager manager) : base(manager)
        {

        }

        public override void Update()
        {
            throw new NotImplementedException();
        }
    }
}
