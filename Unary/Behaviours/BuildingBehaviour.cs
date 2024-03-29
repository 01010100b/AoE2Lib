﻿using AoE2Lib.Bots;
using AoE2Lib.Bots.GameElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unary.Behaviours
{
    internal class BuildingBehaviour : Behaviour
    {
        public Unit Construction { get; set; } = null;

        public override int GetPriority() => 800;

        protected override bool Tick(bool perform)
        {
            if (perform && Construction != null)
            {
                if (Unit[ObjectData.TARGET_ID] != Construction.Id)
                {
                    Unit.Target(Construction);
                }

                Construction.RequestUpdate();

                if (!Construction.Targetable)
                {
                    Construction = null;
                }

                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
