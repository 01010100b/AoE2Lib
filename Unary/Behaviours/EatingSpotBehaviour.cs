using AoE2Lib;
using AoE2Lib.Bots;
using AoE2Lib.Bots.GameElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unary.Behaviours
{
    internal class EatingSpotBehaviour : Behaviour
    {
        public Unit Target { get; private set; } = null;
        public Position Focus { get; private set; }

        protected internal override bool Tick(bool perform)
        {
            Focus = Controller.Unary.GameState.MyPosition;

            Target = KillAnimal();

            if (Target == null)
            {
                Target = EatMeat();
            }

            if (Target == null)
            {
                Target = KillSheep();
            }

            return false;
        }

        private Unit KillAnimal()
        {
            return null;
        }

        private Unit EatMeat()
        {
            var meat = Controller.Unary.GameState.Map.GetUnitsInRange(Focus, Controller.Unary.Settings.MaxEatingRange)
                .Where(u => IsMeat(u))
                .ToList();

            if (meat.Count > 0)
            {
                meat.Sort((a, b) => a[ObjectData.CARRY].CompareTo(b[ObjectData.CARRY]));

                return meat[0];
            }
            else
            {
                return null;
            }
        }

        private Unit KillSheep()
        {
            var sheep = Controller.Unary.GameState.Map.GetUnitsInRange(Focus, Controller.Unary.Settings.KillSheepRange)
                .Where(u => u.Targetable && u[ObjectData.HITPOINTS] > 0 && u[ObjectData.CMDID] == (int)CmdId.LIVESTOCK_GAIA)
                .ToList();

            if (sheep.Count > 0)
            {
                sheep.Sort((a, b) => a.Position.DistanceTo(Focus).CompareTo(b.Position.DistanceTo(Focus)));

                return sheep[0];
            }
            else
            {
                return null;
            }
        }

        private bool IsMeat(Unit unit)
        {
            if (unit.Targetable == false || unit[ObjectData.HITPOINTS] > 0 || unit[ObjectData.CARRY] <= 0)
            {
                return false;
            }

            switch ((UnitClass)unit[ObjectData.CLASS])
            {
                case UnitClass.ControlledAnimal:
                case UnitClass.DomesticAnimal:
                case UnitClass.Livestock:
                case UnitClass.PredatorAnimal:
                case UnitClass.PreyAnimal: return true;
                default: return false;
            }
        }
    }
}
