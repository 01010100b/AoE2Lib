using AoE2Lib.Bots.GameElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unary.Behaviours;

namespace Unary.Managers
{
    // controllers, squads
    internal class UnitsManager : Manager
    {
        private const int GROUPS = 10;

        public IEnumerable<Controller> Controllers => UnitControllers.Values;

        private readonly Dictionary<Unit, Controller> UnitControllers = new();

        public UnitsManager(Unary unary) : base(unary)
        {

        }

        internal override void Update()
        {
            foreach (var unit in Unary.GameState.MyPlayer.Units.Where(u => u.Targetable))
            {
                if (!UnitControllers.ContainsKey(unit))
                {
                    var controller = new Controller(unit, Unary);
                    UnitControllers.Add(unit, controller);
                }
            }

            foreach (var controller in Controllers.ToList())
            {
                if (controller.Unit.Targetable)
                {
                    controller.Tick();
                }
                else
                {
                    UnitControllers.Remove(controller.Unit);
                }
            }
        }
    }
}
