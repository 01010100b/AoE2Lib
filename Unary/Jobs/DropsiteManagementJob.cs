using AoE2Lib;
using AoE2Lib.Bots;
using AoE2Lib.Bots.GameElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unary.Managers;

namespace Unary.Jobs
{
    internal class DropsiteManagementJob : ManagementJob
    {
        public override string Name => "Dropsite management";

        private readonly Dictionary<Resource, List<Unit>> Resources = new();
        private readonly Dictionary<Resource, Dictionary<Tile, double>> DropsitePositions = new();

        public DropsiteManagementJob(Unary unary) : base(unary)
        {
        }

        protected override void Initialize()
        {
            foreach (var resource in Unary.CivInfo.GatheredResources)
            {
                Resources.Add(resource, new());
                DropsitePositions.Add(resource, new());
            }
        }

        protected override void OnClosed()
        {
        }

        protected override void Update()
        {
            UpdateResources();
            UpdateDropsitePositions();
            UpdateDropsites();
        }

        private void UpdateResources()
        {
            foreach (var lst in Resources.Values)
            {
                lst.Clear();
            }

            var civ = Unary.CivInfo;

            foreach (var unit in Unary.GameState.Gaia.Units.Where(u => u.Targetable && u[ObjectData.CARRY] > 0))
            {
                var resource = civ.GetResourceGatheredFrom(unit);

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
            foreach (var resource in DropsitePositions.Keys)
            {
                if (TryGetDropsite(resource, out var dropsite))
                {
                    var positions = DropsitePositions[resource];

                    var tiles = ObjectPool.Get(() => new List<Tile>(), x => x.Clear());
                    tiles.AddRange(positions.Keys);

                    foreach (var tile in tiles)
                    {
                        if (Unary.ShouldRareTick(tile, 301))
                        {
                            positions.Remove(tile);
                        }
                    }

                    ObjectPool.Add(tiles);

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
                            positions[tile] = score;
                        }
                    }

                    ObjectPool.Add(resources);
                }
            }
        }

        private void UpdateDropsites()
        {
            if (!TryInitialDropsites())
            {
                return;
            }

            var vacancies = ObjectPool.Get(() => new Dictionary<Resource, int>(), x => x.Clear());

            foreach (var resource in Resources.Keys)
            {
                vacancies.Add(resource, 0);
            }

            foreach (var job in Unary.JobManager.GetJobs().OfType<ResourceGenerationJob>())
            {
                if (!vacancies.ContainsKey(job.Resource))
                {
                    vacancies.Add(job.Resource, 0);
                }

                vacancies[job.Resource] += job.Vacancies;
            }

            foreach (var kvp in vacancies)
            {
                if (kvp.Value < 1)
                {
                    if (TryGetDropsite(kvp.Key, out var dropsite))
                    {
                        if (GetDesiredGatherers(kvp.Key) > 0)
                        {
                            BuildDropsite(dropsite, DropsitePositions[kvp.Key]);

                            return;
                        }
                    }
                }
            }

            ObjectPool.Add(vacancies);
        }

        private bool TryInitialDropsites()
        {
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

                        return false;
                    }
                    else if (mill.CountTotal < 1)
                    {
                        if (GetDesiredGatherers(Resource.FOOD) > 0)
                        {
                            BuildDropsite(mill, DropsitePositions[Resource.FOOD]);
                        }

                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private bool TryGetDropsite(Resource resource, out UnitType dropsite)
        {
            var id = Unary.CivInfo.GetDropsiteId(resource);

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
            var civ = Unary.CivInfo;
            var range = 0.5 * civ.GetUnitTileWidth(dropsite.Id);
            range += 3;
            var score = 0d;

            foreach (var resource in resources)
            {
                var distance = resource.Tile.Center.DistanceTo(tile.Position);

                if (distance <= range)
                {
                    score += 1 / distance;
                }

                if (resource[ObjectData.CARRY] > 300 && distance < 2)
                {
                    score -= 100;
                }
            }

            var home = Math.Max(20, tile.Position.DistanceTo(Unary.TownManager.MyPosition));

            score /= home / 10;

            return score;
        }

        private int GetDesiredGatherers(Resource resource)
        {
            return Unary.ProductionManager.GetDesiredGatherers(resource);
        }

        private void BuildDropsite(UnitType dropsite, Dictionary<Tile, double> tiles)
        {
            if (tiles.Count == 0)
            {
                Unary.Log.Warning($"Failed to build dropsite {dropsite[ObjectData.BASE_TYPE]}");

                return;
            }

            var construction = Unary.JobManager.GetSingleJob<ConstructionManagementJob>();
            var bt = construction.GetBuildTiles(dropsite, tiles);
            Unary.ProductionManager.Build(dropsite, bt, int.MaxValue, 1, ProductionManager.Priority.DROPSITE);
        }
    }
}
