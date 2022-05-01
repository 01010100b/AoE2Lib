using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unary.Behaviours
{
    internal class EatBehaviour : Behaviour
    {
        private Controller EatingSpot { get; set; } = null;

        protected internal override bool Perform()
        {
            if (EatingSpot != null)
            {
                if (EatingSpot.Unit.Targetable == false
                    || EatingSpot.GetBehaviour<EatingSpotBehaviour>().Target == null)
                {
                    EatingSpot = null;
                }

                if (EatingSpot == null)
                {
                    FindEatingSpot();
                }

                if (EatingSpot != null)
                {
                    Controller.Unit.Target(EatingSpot.GetBehaviour<EatingSpotBehaviour>().Target);
                    
                }

                return true;
            }
            else
            {
                FindEatingSpot();

                if (EatingSpot == null)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

        private void FindEatingSpot()
        {
            var spots = Controller.Manager.EatingSpots.ToList();

            if (spots.Count > 0)
            {
                var eaters = Controller.Manager.GetControllers().Where(c =>
                {
                    var b = c.GetBehaviour<EatBehaviour>();

                    if (b == null)
                    {
                        return false;
                    }
                    else
                    {
                        return b.EatingSpot != null;
                    }
                }).ToList();

                spots.Sort((a, b) => a.Unit.Position.DistanceTo(Controller.Unit.Position)
                    .CompareTo(b.Unit.Position.DistanceTo(Controller.Unit.Position)));

                foreach (var spot in spots)
                {
                    var current = eaters.Count(c => c.GetBehaviour<EatBehaviour>().EatingSpot.Equals(spot));

                    if (current < Controller.Unary.Settings.MaxEatingGroupSize)
                    {
                        EatingSpot = spot;

                        return;
                    }
                }
            }
        }
    }
}
