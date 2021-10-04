using AoE2Lib;
using AoE2Lib.Bots.GameElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            Units[unit] = controller;
        }

        internal override void Update()
        {
            foreach (var unit in Unary.GameState.MyPlayer.Units.Where(u => u.Targetable))
            {
                var cmdid = (CmdId)unit[ObjectData.CMDID];
                switch (cmdid)
                {
                    case CmdId.FISHING_SHIP:
                    case CmdId.LIVESTOCK_GAIA:
                    case CmdId.MILITARY:
                    case CmdId.MONK:
                    case CmdId.TRADE:
                    case CmdId.TRANSPORT:
                    case CmdId.VILLAGER:
                        {
                            if (!Units.ContainsKey(unit))
                            {
                                Units[unit] = new IdlerController(unit, Unary);
                                Unary.Log.Info($"New unit control {unit.Id}");
                            }
                        }; break;
                }
            }

            var controllers = GetControllers<UnitController>();
            foreach (var controller in controllers)
            {
                controller.Unit.RequestUpdate();
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
