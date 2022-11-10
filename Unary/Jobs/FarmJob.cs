using AoE2Lib;
using AoE2Lib.Bots;
using AoE2Lib.Bots.GameElements;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unary.Behaviours;
using Unary.Managers;

namespace Unary.Jobs
{
    internal class FarmJob : ResourceGenerationJob
    {
        public override Resource Resource => Resource.FOOD;
        public override int MaxWorkers => FarmTiles.Count;
        public override string Name => $"Farming at {Location}";

        private readonly List<Tile> FarmTiles = new();
        private int OpenSpots { get; set; } = 1;

        public FarmJob(Unary unary, Controller dropsite) : base(unary, dropsite)
        {
        }
        protected override void Initialize()
        {
        }

        protected override double GetResourcePay(Controller worker)
        {
            if (!worker.HasBehaviour<FarmingBehaviour>())
            {
                return -1;
            }
            else if (WorkerCount > MaxWorkers)
            {
                return -1;
            }
            else if (HasWorker(worker))
            {
                return 1;
            }
            else if (Vacancies < 1)
            {
                return -1;
            }
            else if (OpenSpots > 0)
            {
                return -1;
            }
            else
            {
                return 1;
            }
        }

        protected override void OnWorkerJoining(Controller worker)
        {
        }

        protected override void OnWorkerLeaving(Controller worker)
        {
            if (worker.TryGetBehaviour<FarmingBehaviour>(out var behaviour))
            {
                behaviour.Tile = null;
            }
        }

        protected override void OnClosed()
        {
        }

        protected override void UpdateResourceGeneration()
        {
            FarmTiles.Clear();
            FarmTiles.AddRange(GetFarmTiles());

            var assignments = ObjectPool.Get(() => new Dictionary<Tile, Controller>(), x => x.Clear());

            foreach (var worker in GetWorkers())
            {
                if (worker.TryGetBehaviour<FarmingBehaviour>(out var behaviour))
                {
                    if (behaviour.Tile != null)
                    {
                        if (!FarmTiles.Contains(behaviour.Tile))
                        {
                            behaviour.Tile = null;
                        }
                        else if (assignments.ContainsKey(behaviour.Tile))
                        {
                            behaviour.Tile = null;
                        }
                        else
                        {
                            assignments.Add(behaviour.Tile, worker);
                        }
                    }
                }
            }

            foreach (var worker in GetWorkers())
            {
                if (worker.TryGetBehaviour<FarmingBehaviour>(out var behaviour))
                {
                    if (behaviour.Tile == null)
                    {
                        foreach (var tile in FarmTiles)
                        {
                            if (!assignments.ContainsKey(tile))
                            {
                                behaviour.Tile = tile;
                                assignments.Add(tile, worker);

                                break;
                            }
                        }
                    }
                }
            }

            OpenSpots = 0;
            var civ = Unary.CivInfo;

            foreach (var tile in assignments.Keys)
            {
                var farm = tile.Units.FirstOrDefault(x => x[ObjectData.BASE_TYPE] == civ.FarmId);

                if (farm == null)
                {
                    OpenSpots++;
                }
            }

            ObjectPool.Add(assignments);
        }

        private IEnumerable<Tile> GetFarmTiles()
        {
            var civ = Unary.CivInfo;

            if (Unary.GameState.TryGetUnitType(civ.FarmId, out var farm))
            {
                if (!farm.Available)
                {
                    yield break;
                }

                foreach (var tile in Unary.TownManager.GetFarmTiles(Dropsite.Unit))
                {
                    var current = tile.Units.Count(x => x[ObjectData.BASE_TYPE] == civ.FarmId);

                    if (current == 0)
                    {
                        if (Unary.MapManager.CanBuild(farm, tile, false))
                        {
                            current = 1;
                        }
                    }

                    if (current > 0)
                    {
                        yield return tile;
                    }
                }
            }
        }
    }
}
