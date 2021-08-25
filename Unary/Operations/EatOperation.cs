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
    class EatOperation : Operation
    {
        public Position Focus { get; set; }

        public EatOperation(OperationsManager manager) : base(manager)
        {

        }

        public override void Update()
        {
            var vills = Units.Where(u => u[ObjectData.CMDID] == (int)CmdId.VILLAGER).ToList();

            if (vills.Count == 0)
            {
                return;
            }

            var units = Manager.Unary.GameState.Map.GetTilesInRange(Focus.PointX, Focus.PointY, 3).SelectMany(t => t.Units.Where(u => u.Targetable)).ToList();
            var sheep = Units.FirstOrDefault(u => u[ObjectData.CLASS] == (int)UnitClass.Livestock && u[ObjectData.HITPOINTS] > 0);

            if (sheep != null && Focus.DistanceTo(sheep.Position) > 1)
            {
                sheep.TargetPosition(Focus, UnitAction.MOVE, UnitFormation.LINE, UnitStance.NO_ATTACK);
            }

            // kill boar/deer

            Unit target = null;

            units.Sort((a, b) => a.Id.CompareTo(b.Id));
            target = units
                .Where(u => u[ObjectData.HITPOINTS] > 0)
                .Where(u => u[ObjectData.CLASS] == (int)UnitClass.PreyAnimal
                    || (u[ObjectData.CLASS] == (int)UnitClass.PredatorAnimal && u[ObjectData.CARRY] > 0))
                .FirstOrDefault();

            if (target != null)
            {

            }

            // eat current meat

            if (target == null)
            {
                units.Sort((a, b) => a[ObjectData.CARRY].CompareTo(b[ObjectData.CARRY]));
                target = units.Where(u => u[ObjectData.HITPOINTS] == 0 && u[ObjectData.CARRY] > 0).FirstOrDefault();
            }

            // kill sheep

            if (target == null && sheep != null)
            {
                if (Focus.DistanceTo(sheep.Position) < 2)
                {
                    target = sheep;
                }
            }

            if (target != null)
            {
                foreach (var vill in vills)
                {
                    if (vill[ObjectData.TARGET_ID] != target.Id)
                    {
                        vill.TargetUnit(target, UnitAction.DEFAULT, UnitFormation.LINE, UnitStance.NO_ATTACK);
                    }
                }

                target.RequestUpdate();
            }
        }
    }
}
