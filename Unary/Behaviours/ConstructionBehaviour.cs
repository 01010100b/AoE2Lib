using AoE2Lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unary.Behaviours
{
    internal class ConstructionBehaviour : Behaviour
    {
        private readonly List<Controller> Builders = new();

        protected override bool Perform()
        {
            throw new NotImplementedException();
        }

        private int GetMaxBuilders(Controller controller)
        {
            if (controller.Unit[ObjectData.STATUS] == 0)
            {
                var type = controller.Unit[ObjectData.BASE_TYPE];
                var mod = controller.Unary.Mod;

                if (type == mod.Farm || type == mod.LumberCamp || type == mod.MiningCamp)
                {
                    if (controller.Unary.GameState.GameTime - controller.Unit.FirstUpdateGameTime > TimeSpan.FromMinutes(1))
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

        private int GetTotalBuilders()
        {
            throw new NotImplementedException();
        }
    }
}
