using AoE2Lib;
using AoE2Lib.Bots.GameElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unary.UnitControllers
{
    class HunterController : UnitController
    {
        public HunterController(Unit unit, Unary unary) : base(unit, unary)
        {

        }

        protected override void Tick()
        {
            var target = ChooseTarget();

            if (target != null)
            {
                if (Unit[ObjectData.TARGET_ID] != target.Id)
                {
                    Unit.Target(target);
                }
            }
            else
            {
                Unary.Log.Debug($"Hunter {Unit.Id} can not find meat");
            }
        }

        private Unit ChooseTarget()
        {
            const double KILL_RANGE = 3;
            Unit target = null;

            // kill close live animals
            foreach (var meat in Unary.EconomyManager.GetMeat().Where(u => u.Position.DistanceTo(Unary.GameState.MyPosition) <= KILL_RANGE && u[ObjectData.HITPOINTS] > 0))
            {
                if (meat[ObjectData.CLASS] == (int)UnitClass.PreyAnimal || meat[ObjectData.CLASS] == (int)UnitClass.PredatorAnimal)
                {
                    target = meat;

                    break;
                }
            }

            // eat dead animals
            if (target == null)
            {
                foreach (var meat in Unary.EconomyManager.GetMeat().Where(u => u.Position.DistanceTo(Unary.GameState.MyPosition) <= 5 && u[ObjectData.HITPOINTS] == 0))
                {
                    if (target == null)
                    {
                        target = meat;
                    }
                    else if (meat[ObjectData.CARRY] < target[ObjectData.CARRY])
                    {
                        target = meat;
                    }
                }
            }

            // kill sheep
            if (target == null)
            {
                foreach (var meat in Unary.EconomyManager.GetMeat().Where(u => u.Position.DistanceTo(Unary.GameState.MyPosition) <= KILL_RANGE && u[ObjectData.HITPOINTS] > 0))
                {
                    if (target == null)
                    {
                        target = meat;
                    }
                    else if (meat.Position.DistanceTo(Unary.GameState.MyPosition) < target.Position.DistanceTo(Unary.GameState.MyPosition))
                    {
                        target = meat;
                    }
                }
            }

            return target;
        }
    }
}
