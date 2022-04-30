using AoE2Lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unary.Behaviours
{
    internal class FightAnimalBehaviour : Behaviour
    {
        protected internal override bool Perform()
        {
            var target = Controller.Unit.GetTarget();

            if (target != null && target[ObjectData.CLASS] == (int)UnitClass.PredatorAnimal && target[ObjectData.HITPOINTS] > 0)
            {
                Controller.Unary.Log.Debug($"Unit {Controller.Unit.Id} fighting animal {target.Id}");

                Controller.Unit.RequestUpdate();
                target.RequestUpdate();

                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
