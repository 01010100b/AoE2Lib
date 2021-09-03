using AoE2Lib;
using AoE2Lib.Bots;
using AoE2Lib.Bots.GameElements;
using System;
using System.Collections.Generic;
using System.Text;

namespace Unary.Operations
{
    class GatherOperation : Operation
    {
        public override Position Position => throw new NotImplementedException();
        public override int UnitCapacity => throw new NotImplementedException();

        public readonly Unit Dropsite;
        public readonly Resource Resource;

        public GatherOperation(Unary unary, Unit dropsite, Resource resource) : base(unary)
        {
            if (dropsite == null)
            {
                throw new ArgumentNullException(nameof(dropsite));
            }
            else if (resource != Resource.WOOD && resource != Resource.FOOD && resource != Resource.GOLD && resource != Resource.STONE)
            {
                throw new ArgumentOutOfRangeException(nameof(resource));
            }

            Dropsite = dropsite;
            Resource = resource;
        }

        public override void Update()
        {
            throw new NotImplementedException();
        }
    }
}
