using AoE2Lib;
using AoE2Lib.Bots.GameElements;
using System;
using System.Collections.Generic;
using System.Text;

namespace Unary.Operations
{
    internal class BuildOperation : Operation
    {
        public Unit Building { get; set; }

        public BuildOperation(Unary unary) : base(unary)
        {

        }

        public override void Update()
        {
            if (Building == null)
            {
                return;
            }

            Building.RequestUpdate();

            foreach (var unit in Units)
            {
                if (unit[ObjectData.TARGET_ID] != Building.Id)
                {
                    unit.TargetUnit(Building);
                }
            }
        }
    }
}
