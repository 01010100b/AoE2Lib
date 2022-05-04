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

        protected override bool Tick(bool perform)
        {
            if (!perform)
            {
                if (TimeSinceLastPerformed > TimeSpan.FromMinutes(1))
                {
                    EatingSpot = null;
                }

                return false;
            }

            if (EatingSpot != null)
            {
                if (EatingSpot.Exists == false
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
                        var odds = 1d / Math.Max(1, spot.Unit.Position.DistanceTo(Controller.Unit.Position));

                        if (Controller.Unary.Rng.NextDouble() < odds)
                        {
                            EatingSpot = spot;
                        }
                        
                        return;
                    }
                }
            }
        }
    }
}
