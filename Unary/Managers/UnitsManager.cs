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
                            }
                        }; break;
                }

                if (IsDropsite(unit))
                {
                    if (!Units.ContainsKey(unit))
                    {
                        Units[unit] = new DropsiteController(unit, Unary);
                    }
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

        private bool IsDropsite(Unit unit)
        {
            if (unit[ObjectData.BASE_TYPE] == Unary.Mod.TownCenter)
            {
                return true;
            }
            else if (unit[ObjectData.BASE_TYPE] == Unary.Mod.Mill)
            {
                return true;
            }
            else if (unit[ObjectData.BASE_TYPE] == Unary.Mod.LumberCamp)
            {
                return true;
            }
            else if (unit[ObjectData.BASE_TYPE] == Unary.Mod.MiningCamp)
            {
                return true;
            }
            else
            {
                return false;
            }    
        }
    }
}
