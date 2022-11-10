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
        public override int GetPriority() => 20000;

        protected override bool Tick(bool perform)
        {
            if (perform && Unary.GameState.TryGetUnit(Unit[ObjectData.TARGET_ID], out var target))
            {
                if (target[ObjectData.CLASS] == (int)UnitClass.PredatorAnimal && target[ObjectData.HITPOINTS] > 0)
                {
                    Unit.RequestUpdate();
                    target.RequestUpdate();

                    return true;
                }
            }

            return false;
        }
    }
}
