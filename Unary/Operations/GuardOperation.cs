using AoE2Lib;
using AoE2Lib.Bots.GameElements;
using AoE2Lib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unary.Managers;

namespace Unary.Operations
{
    class GuardOperation : Operation
    {
        public readonly List<Unit> GuardedUnits = new List<Unit>();

        public GuardOperation(OperationsManager manager) : base(manager)
        {

        }

        public override void Update()
        {
            if (Units.Count() == 0)
            {
                return;
            }

            GuardedUnits.RemoveAll(u => !u.Targetable);

            if (GuardedUnits.Count == 0)
            {
                return;
            }

            var units = Units.ToList();
            for (int i = 0; i < units.Count; i++)
            {
                var unit = units[i];
                var guard = GuardedUnits[unit.Id % GuardedUnits.Count];
                var pos = guard.Position + Position.FromPolar(2 * Math.PI * i / units.Count, 2);

                if (unit.Position.DistanceTo(pos) > 0.3)
                {
                    unit.TargetPosition(pos, UnitAction.MOVE, null, null);
                }

                guard.RequestUpdate();
            }

            foreach (var unit in Units)
            {
                
            }
        }
    }
}
