using AoE2Lib.Bots;
using AoE2Lib.Bots.GameElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unary.Managers;

namespace Unary.Behaviours
{
    internal class FarmingBehaviour : Behaviour
    {
        public Tile Tile { get; set; } = null;

        public override int GetPriority() => 400;

        protected override bool Tick(bool perform)
        {
            if (perform && Tile != null)
            {
                var id = Unary.CivInfo.FarmId;
                var farm = Tile.Units.Where(x => x[AoE2Lib.Bots.ObjectData.BASE_TYPE] == id).FirstOrDefault();

                if (farm != null)
                {
                    if (Unit[ObjectData.TARGET_ID] != farm.Id)
                    {
                        Unit.Target(farm);
                    }

                    return true;
                }
                else if (Unary.GameState.TryGetUnitType(id, out var type))
                {
                    Unary.ProductionManager.Build(type, new[] { Tile }, 10000, 3, ProductionManager.Priority.FARM);
                }
            }

            return false;
        }
    }
}
