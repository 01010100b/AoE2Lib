using AoE2Lib;
using AoE2Lib.Bots.GameElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unary.UnitControllers
{
    class FarmerController : UnitController
    {
        public readonly Tile Tile;

        public FarmerController(Tile tile, Unit unit, Unary unary) : base(unit, unary)
        {
            Tile = tile;
        }

        protected override void Tick()
        {
            var farm = Tile.Units.FirstOrDefault(u => u.Targetable && u[ObjectData.BASE_TYPE] == Unary.Mod.Farm);

            if (farm != null)
            {
                if (Unit[ObjectData.TARGET_ID] != farm.Id)
                {
                    Unit.Target(farm);
                }
            }
        }
    }
}
