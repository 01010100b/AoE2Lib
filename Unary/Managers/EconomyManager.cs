using AoE2Lib;
using AoE2Lib.Bots;
using AoE2Lib.Bots.GameElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unary.Behaviours;
using Unary.Jobs;
using YTY.AocDatLib;

namespace Unary.Managers
{
    internal class EconomyManager : Manager
    {
        private static readonly Resource[] KnownResources = { Resource.WOOD, Resource.FOOD, Resource.GOLD, Resource.STONE };
        
        private EatJob EatingJob { get; set; } = null;
        private readonly Dictionary<Resource, List<Unit>> Resources = new();
        private readonly Dictionary<Resource, List<KeyValuePair<Tile, double>>> DropsitePositions = new();

        public EconomyManager(Unary unary) : base(unary)
        {
            foreach (var resource in KnownResources)
            {
                Resources.Add(resource, new());
                DropsitePositions.Add(resource, new());
            }
        }

        public int GetCurrentGatherers(Resource resource)
        {
            var total = 0;

            foreach (var job in Unary.JobManager.GetJobs().OfType<ResourceGenerationJob>())
            {
                total += job.WorkerCount;
            }

            return total;
        }

        public int GetDesiredGatherers(Resource resource)
        {
            return Unary.StrategyManager.GetDesiredGatherers(resource);
        }

        public Resource GetResourceGatheredFrom(Unit unit)
        {
            return (UnitClass)unit[ObjectData.CLASS] switch
            {
                UnitClass.BerryBush => Resource.FOOD,
                UnitClass.Tree => Resource.WOOD,
                UnitClass.GoldMine => Resource.GOLD,
                UnitClass.StoneMine => Resource.STONE,
                _ => Resource.NONE,
            };
        }

        protected internal override void Update()
        {
            var actions = ObjectPool.Get(() => new List<Action>(), x => x.Clear());
            actions.Add(UpdateResources);
            actions.Add(UpdateDropsitePositions);
            actions.Add(UpdateDropsites);
            actions.Add(UpdateEatingJob);
            actions.Add(UpdateGatheringJobs);
            actions.Add(DoStats);

            Run(actions);
            ObjectPool.Add(actions);
        }

        private void UpdateResources()
        {
            foreach (var lst in Resources.Values)
            {
                lst.Clear();
            }

            foreach (var unit in Unary.GameState.Gaia.Units.Where(u => u.Targetable && u[ObjectData.CARRY] > 0))
            {
                var resource = GetResourceGatheredFrom(unit);

                if (Resources.ContainsKey(resource))
                {
                    Resources[resource].Add(unit);
                }
            }

            foreach (var kvp in Resources)
            {
                Unary.Log.Debug($"{kvp.Key} has {kvp.Value.Count} piles remaining");
            }
        }

        private void UpdateDropsitePositions()
        {
            foreach (var resource in KnownResources)
            {
                if (TryGetDropsite(resource, out var dropsite))
                {
                    var positions = DropsitePositions[resource];
                    positions.RemoveAll(x => Unary.ShouldRareTick(x.Key, 301));

                    var units = Resources[resource];

                    if (units.Count == 0)
                    {
                        continue;
                    }

                    var unit = units[Unary.Rng.Next(units.Count)];
                    var resources = ObjectPool.Get(() => new List<Unit>(), x => x.Clear());
                    resources.AddRange(units.Where(x => x.Position.DistanceTo(unit.Position) < 10));

                    foreach (var tile in Unary.GameState.Map.GetTilesInRange(unit.Position, 5))
                    {
                        if (Unary.MapManager.CanBuild(dropsite, tile))
                        {
                            var score = GetScore(dropsite, tile, resources);
                            positions.Add(new(tile, score));
                        }
                    }

                    ObjectPool.Add(resources);
                }
            }
        }

        private void UpdateDropsites()
        {
            var capacities = ObjectPool.Get(() => new Dictionary<Resource, int>(), x => x.Clear());

            foreach (var resource in KnownResources)
            {
                capacities.Add(resource, 0);
            }

            foreach (var job in Unary.JobManager.GetJobs().OfType<ResourceGenerationJob>())
            {
                if (!capacities.ContainsKey(job.Resource))
                {
                    capacities.Add(job.Resource, 0);
                }

                capacities[job.Resource] += job.Vacancies;
            }

            if (TryGetDropsite(Resource.WOOD, out var lumber_camp))
            {
                if (TryGetDropsite(Resource.FOOD, out var mill))
                {
                    if (lumber_camp.CountTotal < 1)
                    {
                        if (GetDesiredGatherers(Resource.WOOD) > 0)
                        {
                            BuildDropsite(lumber_camp, DropsitePositions[Resource.WOOD]);
                        }
                    }
                    else if (mill.CountTotal < 1)
                    {
                        if (GetDesiredGatherers(Resource.FOOD) > 0)
                        {
                            BuildDropsite(mill, DropsitePositions[Resource.FOOD]);
                        }
                    }
                    else
                    {
                        if (capacities[Resource.WOOD] < 1)
                        {
                            BuildDropsite(lumber_camp, DropsitePositions[Resource.WOOD]);
                        }
                    }
                }
            }

            ObjectPool.Add(capacities);
        }

        private void UpdateEatingJob()
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
                            Unary.JobManager.AddJob(EatingJob);
                        }
                    }
                }
            }
            else if (!EatingJob.Dropsite.CanControl)
            {
                EatingJob.Close();
                EatingJob = null;
            }
        }

        private void UpdateGatheringJobs()
        {
            var jobs = ObjectPool.Get(() => new Dictionary<Controller, List<GatherJob>>(), x => x.Clear());

            foreach (var controller in Unary.UnitsManager.GetControllers())
            {
                var type = controller.Unit[ObjectData.BASE_TYPE];
                var mod = Unary.Mod;
                if (type == mod.TownCenter || type == mod.Mill || type == mod.LumberCamp || type == mod.GoldMiningCamp
                    || type == mod.StoneMiningCamp)
                {
                    jobs.Add(controller, new());
                }
            }

            foreach (var job in Unary.JobManager.GetJobs().OfType<GatherJob>())
            {
                if (jobs.TryGetValue(job.Dropsite, out var current))
                {
                    current.Add(job);
                }
            }

            foreach (var kvp in jobs)
            {
                var type = kvp.Key.Unit[ObjectData.BASE_TYPE];

                if (type == Unary.Mod.TownCenter && kvp.Value.Count < KnownResources.Length)
                {
                    foreach (var resource in KnownResources)
                    {
                        if (!kvp.Value.Select(x => x.Resource).Contains(resource))
                        {
                            var job = new GatherJob(Unary, kvp.Key, resource);
                            Unary.JobManager.AddJob(job);
                        }
                    }
                }
                else if (type == Unary.Mod.Mill && kvp.Value.Count < 1)
                {
                    var job = new GatherJob(Unary, kvp.Key, Resource.FOOD);
                    Unary.JobManager.AddJob(job);
                }
                else if (type == Unary.Mod.LumberCamp && kvp.Value.Count < 1)
                {
                    var job = new GatherJob(Unary, kvp.Key, Resource.WOOD);
                    Unary.JobManager.AddJob(job);
                }
                else if (type == Unary.Mod.GoldMiningCamp && kvp.Value.Count < 1)
                {
                    var job = new GatherJob(Unary, kvp.Key, Resource.GOLD);
                    Unary.JobManager.AddJob(job);
                }
                else if (type == Unary.Mod.StoneMiningCamp && kvp.Value.Count < 1)
                {
                    var job = new GatherJob(Unary, kvp.Key, Resource.STONE);
                    Unary.JobManager.AddJob(job);
                }
            }

            ObjectPool.Add(jobs);
        }

        private void DoStats()
        {
            var counts = ObjectPool.Get(() => new Dictionary<Resource, int>(), x => x.Clear());
            var total = ObjectPool.Get(() => new Dictionary<Resource, double>(), x => x.Clear());

            foreach (var job in Unary.JobManager.GetJobs().OfType<ResourceGenerationJob>())
            {
                var resource = job.Resource;
                var rate = job.GetRate();

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

            foreach (var resource in KnownResources)
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

                Unary.Log.Debug($"{resource} gather rate {rate:N2}/min");
            }

            ObjectPool.Add(counts);
            ObjectPool.Add(total);
        }

        private bool TryGetDropsite(Resource resource, out UnitType dropsite)
        {
            var id = -1;

            switch (resource)
            {
                case Resource.FOOD: id = Unary.Mod.Mill; break;
                case Resource.WOOD: id = Unary.Mod.LumberCamp; break;
                case Resource.GOLD: id = Unary.Mod.GoldMiningCamp; break;
                case Resource.STONE: id = Unary.Mod.StoneMiningCamp; break;
            }

            if (id >= 0 && Unary.GameState.TryGetUnitType(id, out var type))
            {
                dropsite = type;

                return true;
            }
            else
            {
                dropsite = default;

                return false;
            }
        }

        private double GetScore(UnitType dropsite, Tile tile, List<Unit> resources)
        {
            var civ = Unary.Mod.GetCivInfo(Unary.GameState.MyPlayer.Civilization);
            var range = 0.5 * civ.GetUnitWidth(dropsite.Id);
            range += 3;
            var score = 0d;

            foreach (var resource in resources)
            {
                var distance = resource.Tile.Center.DistanceTo((Position)tile.Position);

                if (distance <= range)
                {
                    score += 1 / distance;
                }     
            }

            var home = Math.Max(20, tile.Position.DistanceTo(Unary.TownManager.MyPosition));

            score /= home / 10;

            return score;
        }

        private void BuildDropsite(UnitType dropsite, List<KeyValuePair<Tile, double>> positions)
        {
            if (positions.Count == 0)
            {
                return;
            }

            positions.Sort((a, b) => b.Value.CompareTo(a.Value));
            var tiles = positions.Select(x => x.Key).Where(x => Unary.MapManager.CanBuild(dropsite, x)).Take(100);
            Unary.ProductionManager.Build(dropsite, tiles, int.MaxValue, 1, ProductionManager.Priority.DROPSITE);
        }
    }
}
