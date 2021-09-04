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
        public override int UnitCapacity => 1;
        public readonly Unit Building;

        public BuildOperation(Unary unary, Unit building) : base(unary)
        {
            if (building == null)
            {
                throw new ArgumentNullException(nameof(building));
            }

            Building = building;
        }

        public override void Update()
        {
            Unary.Log.Debug($"Building {Building.Id}");

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
