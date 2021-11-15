using AoE2Lib;
using AoE2Lib.Bots.GameElements;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unary.Algorithms;
using Unary.UnitControllers;

namespace Unary.Managers
{
    class UnitsManager : Manager
    {
        private readonly Dictionary<Unit, UnitController> Units = new();
        
        public UnitsManager(Unary unary) : base(unary)
        {

        }

        public List<T> GetControllers<T>() where T : UnitController
        {
            return Units.Values.OfType<T>().Cast<T>().ToList();
        }

        public void SetController(Unit unit, UnitController controller)
        {
            if (controller == null)
            {
                Units.Remove(unit);
            }
            else
            {
                Units[unit] = controller;
            }
        }

        internal override void Update()
        {
            UpdateControllers();
        }

        private void UpdateControllers()
        {
            foreach (var unit in Unary.GameState.MyPlayer.Units.Where(u => u.Targetable))
            {
                if (!Units.ContainsKey(unit))
                {
                    Units[unit] = new IdlerController(unit, Unary);
                }
            }

            var controllers = GetControllers<UnitController>();
            foreach (var controller in controllers)
            {
                if (controller.Unit.Targetable)
                {
                    controller.Update();
                }
                else
                {
                    Units.Remove(controller.Unit);
                }
            }
        }
    }
}
