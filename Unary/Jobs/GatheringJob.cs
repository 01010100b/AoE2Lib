using AoE2Lib;
using AoE2Lib.Bots;
using AoE2Lib.Bots.GameElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unary.Behaviours;

namespace Unary.Jobs
{
    internal class GatheringJob : ResourceGenerationJob
    {
        public readonly Unit Dropsite;
        public override Resource Resource => GatheredResource;
        public override int MaxWorkers => Math.Min(Math.Min(6, Resources.Count * 2), Unary.EconomyManager.GetDesiredGatherers(Resource));
        public override string Name => $"Gathering {Resource} at {Location}";
        public override Position Location => Dropsite.Position;

        private readonly Resource GatheredResource;
        private readonly Dictionary<Tile, int> PathDistances = new();
        private readonly List<KeyValuePair<Tile, Unit>> Resources = new();
        private readonly Dictionary<Tile, int> MaxOccupancies = new();

        public GatheringJob(Unary unary, Unit dropsite, Resource resource) : base(unary)
        {
            Dropsite = dropsite;
            GatheredResource = resource;
        }

        public override double GetPay(Controller worker)
        {
            if (!worker.HasBehaviour<GatherBehaviour>())
            {
                return -1;
            }
            else if (WorkerCount > MaxWorkers)
            {
                return -1;
            }
            else if (HasWorker(worker))
            {
                if (worker.TryGetBehaviour<GatherBehaviour>(out var behaviour))
                {
                    return GetPay(behaviour.Tile);
                }
                else
                {
                    return -1;
                }
            }
            else if (Vacancies < 1)
            {
                return -1;
            }
            else
            {
                var index = Math.Min(WorkerCount / 2, Resources.Count - 1);

                if (index >= 0)
                {
                    return GetPay(Resources[index].Key);
                }
                else
                {
                    return -1;
                }
            }
        }
        public override void OnRemoved()
        {
        }

        protected override void OnWorkerJoining(Controller worker)
        {
        }

        protected override void OnWorkerLeaving(Controller worker)
        {
            if (worker.TryGetBehaviour<GatherBehaviour>(out var behaviour))
            {
                behaviour.Target = null;
                behaviour.Tile = null;
            }
        }

        protected override void UpdateResourceGeneration()
        {
            UpdatePathDistances();
            UpdateResources();
            UpdateWorkers();
        }

        private void UpdatePathDistances()
        {
            PathDistances.Clear();
            var width = (int)Math.Round(Unary.Mod.GetUnitWidth(Unary.GameState.MyPlayer.Civilization, Dropsite[ObjectData.BASE_TYPE]));
            var footprint = Utils.GetUnitFootprint(Dropsite.Position.PointX, Dropsite.Position.PointY, width, width, 1);
            var x = footprint.X;
            var y = footprint.Y;
            var map = Unary.GameState.Map;

            for (y = footprint.Y; y < footprint.Bottom; y++)
            {
                if (map.TryGetTile(x, y, out var tile))
                {
                    if (Unary.MapManager.CanReach(tile))
                    {
                        PathDistances[tile] = 0;
                    }
                }
            }

            x = footprint.Right - 1;
            for (y = footprint.Y; y < footprint.Bottom; y++)
            {
                if (map.TryGetTile(x, y, out var tile))
                {
                    if (Unary.MapManager.CanReach(tile))
                    {
                        PathDistances[tile] = 0;
                    }
                }
            }

            y = footprint.Y;
            for (x = footprint.X; x < footprint.Right; x++)
            {
                if (map.TryGetTile(x, y, out var tile))
                {
                    if (Unary.MapManager.CanReach(tile))
                    {
                        PathDistances[tile] = 0;
                    }
                }
            }

            y = footprint.Bottom - 1;
            for (x = footprint.X; x < footprint.Right; x++)
            {
                if (map.TryGetTile(x, y, out var tile))
                {
                    if (Unary.MapManager.CanReach(tile))
                    {
                        PathDistances[tile] = 0;
                    }
                }
            }

            Algorithms.AddAllPathDistances(PathDistances, GetPathNeighbours, Unary.Settings.MaxDropsiteDistance - 1);
        }

        private void UpdateResources()
        {
            Resources.Clear();

            foreach (var tile in PathDistances.Keys)
            {
                foreach (var neighbour in tile.GetNeighbours())
                {
                    foreach (var unit in neighbour.Units.Where(u => u.Targetable && u[ObjectData.CARRY] > 0))
                    {
                        if (Unary.EconomyManager.GetResourceGatheredFrom(unit) == Resource)
                        {
                            Resources.Add(new(tile, unit));
                        }
                    }
                }
            }

            Resources.Sort((a, b) => PathDistances[a.Key].CompareTo(PathDistances[b.Key]));

            var tiles = ObjectPool.Get(() => new List<Tile>(), x => x.Clear());
            tiles.AddRange(MaxOccupancies.Keys);

            foreach (var tile in tiles)
            {
                if (Unary.ShouldRareTick(tile, 101))
                {
                    MaxOccupancies.Remove(tile);
                }
            }

            ObjectPool.Add(tiles);
        }

        private void UpdateWorkers()
        {
            var retask = false;

            foreach (var worker in GetWorkers())
            {
                if (worker.TryGetBehaviour<GatherBehaviour>(out var behaviour))
                {
                    if (behaviour.Target == null || !behaviour.Target.Targetable)
                    {
                        behaviour.Target = null;
                        behaviour.Tile = null;
                        retask = true;
                    }
                }
            }

            if (retask)
            {
                var occupancy = ObjectPool.Get(() => new Dictionary<Tile, int>(), x => x.Clear());

                foreach (var gatherer in Unary.UnitsManager.Gatherers)
                {
                    if (gatherer.TryGetBehaviour<GatherBehaviour>(out var behaviour))
                    {
                        var tile = behaviour.Tile;

                        if (tile != null)
                        {
                            if (!occupancy.ContainsKey(tile))
                            {
                                occupancy.Add(tile, 0);
                            }

                            occupancy[tile]++;
                        }
                    }
                }

                foreach (var worker in GetWorkers())
                {
                    if (worker.TryGetBehaviour<GatherBehaviour>(out var behaviour))
                    {
                        if (behaviour.Target == null)
                        {
                            foreach (var resource in Resources)
                            {
                                var tile = resource.Key;
                                var target = resource.Value;

                                if (!occupancy.ContainsKey(tile))
                                {
                                    occupancy.Add(tile, 0);
                                }

                                var occ = occupancy[tile];
                                var max = GetMaxOccupancy(tile);

                                if (occ < max)
                                {
                                    behaviour.Target = target;
                                    behaviour.Tile = tile;
                                    occupancy[tile]++;

                                    break;
                                }
                            }
                        }
                    }
                }

                ObjectPool.Add(occupancy);
            }
        }

        private double GetPay(Tile tile)
        {
            var carry = 10;
            var rate = 0.4;
            var speed = 0.8;
            var distance = Unary.Settings.MaxDropsiteDistance;
            if (tile != null && PathDistances.TryGetValue(tile, out var d))
            {
                distance = d;
            }

            return Utils.GetGatherRate(rate, distance + 0.5, speed, carry);
        }

        private int GetMaxOccupancy(Tile tile)
        {
            if (!MaxOccupancies.ContainsKey(tile))
            {
                var free = 0;

                foreach (var neighbour in tile.GetNeighbours())
                {
                    if (Unary.MapManager.CanReach(neighbour))
                    {
                        free++;
                    }
                }

                var max = Math.Min(2, free);
                MaxOccupancies.Add(tile, max);
            }

            return MaxOccupancies[tile];
        }

        private readonly List<Tile> __PathNeighbours = new();
        private IReadOnlyList<Tile> GetPathNeighbours(Tile tile)
        {
            var neighbours = tile.GetNeighbours();
            __PathNeighbours.Clear();

            for (int i = 0; i < neighbours.Count; i++)
            {
                var neighbour = neighbours[i];
                var access = neighbour.IsOnLand && !Unary.MapManager.IsPassageBlocked(neighbour);

                if (access || neighbour.Center.DistanceTo(Unary.TownManager.MyPosition) < 3)
                {
                    __PathNeighbours.Add(neighbour);
                }
            }

            return __PathNeighbours;
        }
    }
}
