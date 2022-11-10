using AoE2Lib;
using AoE2Lib.Bots.GameElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unary.Managers;

namespace Unary.Jobs
{
    internal class FarmingManagementJob : ManagementJob
    {
        public override string Name => "Farming management";

        private readonly Dictionary<Tile, double> MillPositions = new();
        private TimeSpan LastFarmUnavailableTime { get; set; } = TimeSpan.Zero;

        public FarmingManagementJob(Unary unary) : base(unary)
        {
        }

        protected override void Initialize()
        {
        }

        protected override void OnClosed()
        {
        }

        protected override void Update()
        {
            UpdateMillPositions();
            BuildMills();
        }

        private void UpdateMillPositions()
        {
            var tiles = ObjectPool.Get(() => new List<Tile>(), x => x.Clear());
            tiles.AddRange(MillPositions.Keys);

            foreach (var tile in tiles)
            {
                if (Unary.ShouldRareTick(tile, 301))
                {
                    MillPositions.Remove(tile);
                }
            }

            ObjectPool.Add(tiles);

            var civ = Unary.CivInfo;
            var size = civ.GetUnitTileWidth(civ.MillId);
            var inside = Unary.TownManager.GetInsideTiles();

            if (inside.Count == 0)
            {
                return;
            }

            if (Unary.GameState.TryGetUnitType(civ.MillId, out var mill))
            {
                if (Unary.GameState.TryGetUnitType(civ.FarmId, out var farm))
                {
                    for (int i = 0; i < 1; i++)
                    {
                        var tile = inside[Unary.Rng.Next(inside.Count)];

                        if (Unary.MapManager.CanBuild(mill, tile))
                        {
                            var score = -tile.Position.DistanceTo(Unary.TownManager.MyPosition) / 10;

                            foreach (var ftile in Unary.TownManager.GetFarmTiles(tile.X, tile.Y, size, size))
                            {
                                if (Unary.MapManager.CanBuild(farm, ftile, false))
                                {
                                    score++;
                                }
                            }

                            MillPositions[tile] = score;
                        }
                    }
                }
            }
        }

        private void BuildMills()
        {
            var civ = Unary.CivInfo;

            if (Unary.GameState.TryGetUnitType(civ.MillId, out var mill))
            {
                if (Unary.GameState.TryGetUnitType(civ.FarmId, out var farm))
                {
                    if (!farm.Available)
                    {
                        LastFarmUnavailableTime = Unary.GameState.GameTime;

                        return;
                    }

                    if ((Unary.GameState.GameTime - LastFarmUnavailableTime) < TimeSpan.FromSeconds(30))
                    {
                        return;
                    }

                    var vacancies = 0;

                    foreach (var job in Unary.JobManager.GetJobs().OfType<FarmJob>())
                    {
                        vacancies += job.Vacancies;
                    }

                    if (vacancies <= 0)
                    {
                        var construction = Unary.JobManager.GetSingleJob<ConstructionManagementJob>();
                        var tiles = construction.GetBuildTiles(mill, MillPositions);
                        Unary.ProductionManager.Build(mill, tiles, 1000, 1, ProductionManager.Priority.DROPSITE);
                    }
                }
            }
        }
    }
}
