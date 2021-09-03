using AoE2Lib;
using AoE2Lib.Bots;
using AoE2Lib.Bots.GameElements;
using System;
using System.Collections.Generic;
using System.Text;

namespace Unary.Operations
{
    internal class BuildOperation : Operation
    {
        public override Position Position => Building != null ? Building.Position : Unary.GameState.MyPosition;

        public readonly Unit Building;

        public BuildOperation(Unary unary, Unit building) : base(unary)
        {
            Building = building;
        }

        public override void Update()
        {
            Unary.Log.Info($"Building {Building.Id}");

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
