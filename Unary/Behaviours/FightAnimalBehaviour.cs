using AoE2Lib;
using AoE2Lib.Bots;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unary.Behaviours
{
    internal class FightAnimalBehaviour : Behaviour
    {
        protected override bool Tick(bool perform)
        {
            if (Controller.Unary.GameState.TryGetUnit(Controller.Unit[ObjectData.TARGET_ID], out var target))
            {
                if (target != null && target[ObjectData.CLASS] == (int)UnitClass.PredatorAnimal && target[ObjectData.HITPOINTS] > 0)
                {
                    Controller.Unary.Log.Debug($"Unit {Controller.Unit.Id} fighting animal {target.Id}");

                    Controller.Unit.RequestUpdate();
                    target.RequestUpdate();

                    return true;
                }
            }

            return false;
        }
    }
}
