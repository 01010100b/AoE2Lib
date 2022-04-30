using AoE2Lib;
using AoE2Lib.Bots;
using AoE2Lib.Bots.GameElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unary.Behaviours;

namespace Unary.Squads
{
    internal class EatingSquad : Squad
    {
        public override int Priority => 10;

        private Position Focus { get; set; }

        protected override void Tick()
        {
            Focus = Unary.GameState.MyPosition;

            var target = KillAnimal();

            if (target == null)
            {
                target = EatAnimal();
            }

            if (target == null)
            {
                target = KillSheep();
            }

            if (target == null)
            {
                foreach (var controller in Controllers)
                {
                    controller.SetSquad(null);
                }

                return;
            }

            if (Controllers.Count < Unary.Settings.MaxEatingGroup)
            {
                var villagers = Unary.UnitsManager.Villagers.Where(c => c.Squad == null).ToList();

                if (villagers.Count >= 2)
                {
                    villagers[0].SetSquad(this);
                }
            }

            foreach (var controller in Controllers)
            {
                controller.Unit.Target(target);
                controller.Unit.RequestUpdate();
            }

            target.RequestUpdate();
        }

        private Unit KillAnimal()
        {
            return null;
        }

        private Unit EatAnimal()
        {
            var animals = Unary.GameState.Map.GetTilesInRange(Focus, Unary.Settings.MaxEatingRange).SelectMany(t => t.Units)
                .Where(u => u.Targetable && u[ObjectData.HITPOINTS] == 0 && u[ObjectData.CARRY] > 0)
                .ToList();

            if (animals.Count > 0)
            {
                animals.Sort((a, b) => a[ObjectData.CARRY].CompareTo(b[ObjectData.CARRY]));

                return animals[0];
            }
            else
            {
                return null;
            }
        }

        private Unit KillSheep()
        {
            var sheep = Unary.GameState.MyPlayer.Units
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
    }
}
