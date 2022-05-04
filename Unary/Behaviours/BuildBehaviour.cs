using AoE2Lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unary.Behaviours
{
    internal class BuildBehaviour : Behaviour
    {
        private Controller Construction { get; set; } = null;

        protected override bool Tick(bool perform)
        {
            if (!perform)
            {
                if (TimeSinceLastPerformed > TimeSpan.FromMinutes(1))
                {
                    Construction = null;
                }

                return false;
            }

            if (Construction != null)
            {
                if (Construction.Exists == false
                    || Construction.GetBehaviour<ConstructionBehaviour>().MaxBuilders == 0)
                {
                    Construction = null;
                    FindConstruction(true);
                }

                if (Construction != null)
                {
                    Controller.Unit.Target(Construction.Unit);
                }

                return true;
            }
            else
            {
                FindConstruction(false);

                if (Construction == null)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

        private void FindConstruction(bool always)
        {
            var constructions = Controller.Manager.Constructions.ToList();
            
            if (constructions.Count > 0)
            {
                var builders = Controller.Manager.GetControllers().Where(c =>
                {
                    var b = c.GetBehaviour<BuildBehaviour>();

                    if (b == null)
                    {
                        return false;
                    }
                    else
                    {
                        return b.Construction != null;
                    }
                }).ToList();

                constructions.Sort((a, b) => a.Unit.Position.DistanceTo(Controller.Unit.Position)
                    .CompareTo(b.Unit.Position.DistanceTo(Controller.Unit.Position)));

                foreach (var construction in constructions)
                {
                    var current = builders.Count(c => c.GetBehaviour<BuildBehaviour>().Construction.Equals(construction));

                    if (current < construction.GetBehaviour<ConstructionBehaviour>().MaxBuilders)
                    {
                        var odds = 1d / Math.Max(1, construction.Unit.Position.DistanceTo(Controller.Unit.Position));

                        if (always || Controller.Unary.Rng.NextDouble() < odds)
                        {
                            Construction = construction;
                        }

                        return;
                    }
                }
            }
        }
    }
}
