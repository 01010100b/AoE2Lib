using AoE2Lib.Bots;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unary.Jobs;

namespace Unary.Behaviours
{
    internal class DropsiteBehaviour : Behaviour
    {
        private readonly List<ResourceGenerationJob> Jobs = new();

        public override int GetPriority() => 1;

        protected override bool Tick(bool perform)
        {
            foreach (var resource in Unary.CivInfo.GetDropsiteResources(Unit[ObjectData.BASE_TYPE]))
            {
                if (!Jobs.OfType<GatherJob>().Where(x => x.Resource == resource).Any())
                {
                    var job = new GatherJob(Unary, Controller, resource);
                    Jobs.Add(job);
                }

                if (resource == Resource.FOOD)
                {
                    if (!Jobs.OfType<FarmJob>().Any())
                    {
                        var job = new FarmJob(Unary, Controller);
                        Jobs.Add(job);
                    }
                }

                if (Unit[ObjectData.BASE_TYPE] == Unary.CivInfo.TownCenterId)
                {
                    if (!Jobs.OfType<EatJob>().Any())
                    {
                        var job = new EatJob(Unary, Controller);
                        Jobs.Add(job);
                    }
                }
            }

            return false;
        }
    }
}
