using AoE2Lib;
using AoE2Lib.Bots;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unary.Behaviours
{
    internal class ConstructionSpotBehaviour : Behaviour
    {
        public int MaxBuilders => GetMaxBuilders();

        protected override bool Tick(bool perform)
        {
            if (MaxBuilders > 0)
            {
                Controller.Unit.RequestUpdate();
            }

            return false;
        }

        private int GetMaxBuilders()
        {
            if (Controller.Unit[ObjectData.STATUS] == 0)
            {
                var type = Controller.Unit[ObjectData.BASE_TYPE];
                var mod = Controller.Unary.Mod;

                if (type == mod.Farm || type == mod.WoodCamp || type == mod.MiningCamp)
                {
                    if (Controller.Unary.GameState.GameTime - Controller.Unit.FirstUpdateGameTime > TimeSpan.FromMinutes(1))
                    {
                        return 1;
                    }
                    else
                    {
                        return 0;
                    }
                }
                else
                {
                    return 1;
                }
            }
            else
            {
                return 0;
            }
        }
    }
}
