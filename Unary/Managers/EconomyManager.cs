using AoE2Lib;
using AoE2Lib.Bots;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unary.Jobs;

namespace Unary.Managers
{
    internal class EconomyManager : Manager
    {
        private static readonly List<Resource> Resources = new List<Resource>() { Resource.FOOD, Resource.WOOD, Resource.GOLD, Resource.STONE };
        
        private EatingJob EatingJob { get; set; } = null;

        public EconomyManager(Unary unary) : base(unary)
        {
        }

        public int GetDesiredGatherers(Resource resource)
        {
            return Unary.StrategyManager.GetDesiredGatherers(resource);
        }

        protected internal override void Update()
        {
            UpdateJobs();
            DoStats();
        }

        private void UpdateJobs()
        {
            if (EatingJob == null)
            {
                var pos = Unary.TownManager.MyPosition;

                foreach (var controller in Unary.UnitsManager.GetControllers())
                {
                    if (controller.Unit[ObjectData.BASE_TYPE] == Unary.Mod.TownCenter)
                    {
                        if (controller.Unit.Position.DistanceTo(pos) < 3)
                        {
                            EatingJob = new(Unary, controller);
                            Unary.UnitsManager.AddJob(EatingJob);
                        }
                    }
                }
            }
            else if (!EatingJob.TC.Exists)
            {
                EatingJob.Close();
                Unary.UnitsManager.RemoveJob(EatingJob);
                EatingJob = null;
            }
        }

        private void DoStats()
        {
            var counts = ObjectPool.Get(() => new Dictionary<Resource, int>(), x => x.Clear());
            var total = ObjectPool.Get(() => new Dictionary<Resource, double>(), x => x.Clear());

            foreach (var job in Unary.UnitsManager.GetJobs().OfType<ResourceGenerationJob>())
            {
                var resource = job.Resource;
                var rate = job.GetRate(TimeSpan.FromMinutes(5));

                if (rate >= 0)
                {
                    if (!counts.ContainsKey(resource))
                    {
                        counts.Add(resource, 0);
                        total.Add(resource, 0);
                    }

                    counts[resource]++;
                    total[resource] += rate;
                }
            }

            foreach (var resource in Resources)
            {
                var count = 0;
                var rate = 0d;

                if (counts.ContainsKey(resource))
                {
                    count = counts[resource];
                    rate = total[resource];
                }

                rate /= Math.Max(1, count);
                rate *= 60;

                Unary.Log.Info($"{resource} gather rate {rate:N2}/min");
            }

            ObjectPool.Add(counts);
            ObjectPool.Add(total);
        }
    }
}
