﻿using AoE2Lib;
using AoE2Lib.Bots;
using AoE2Lib.Bots.GameElements;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using Unary.UnitControllers;
using Unary.UnitControllers.VillagerControllers;

namespace Unary.Managers
{
    class OldBuildingManager : Manager
    {
        public static readonly Point[] TC_FARM_DELTAS = { new Point(2, 3), new Point(-1, 3), new Point(3, 0), new Point(3, -3), new Point(-4, 2), new Point(-4, -1), new Point(0, -4), new Point(-3, -4) };
        public static readonly Point[] MILL_FARM_DELTAS = { new Point(-1, 2), new Point(2, -1), new Point(2, 2), new Point(-3, -1), new Point(-1, -3) };

        public double MaximumTownSize { get; private set; } = 40;

        private readonly HashSet<Tile> InsideTiles = new(); 
        private readonly List<Tile> FarmPlacements = new();
        private readonly Dictionary<Unit, List<KeyValuePair<Tile, double>>> FoodPlacements = new();
        private readonly Dictionary<Unit, List<KeyValuePair<Tile, double>>> WoodPlacements = new();
        private readonly Dictionary<Unit, List<KeyValuePair<Tile, double>>> GoldPlacements = new();
        private readonly Dictionary<Unit, List<KeyValuePair<Tile, double>>> StonePlacements = new();
        private readonly Dictionary<int, HashSet<Tile>> BuildingPlacements = new();

        public OldBuildingManager(Unary unary) : base(unary)
        {
            BuildingPlacements.Add(1, new HashSet<Tile>());
            BuildingPlacements.Add(2, new HashSet<Tile>());
            BuildingPlacements.Add(3, new HashSet<Tile>());
            BuildingPlacements.Add(4, new HashSet<Tile>());
            BuildingPlacements.Add(5, new HashSet<Tile>());
        }

        public bool IsInside(Tile tile)
        {
            // inside town
            return InsideTiles.Contains(tile);
        }

        public IReadOnlyList<Tile> GetFarmPlacements()
        {
            return FarmPlacements;
        }

        public List<KeyValuePair<Tile, double>> GetDropsitePlacements(Resource resource)
        {
            var dict = WoodPlacements;
            var site = Unary.Mod.LumberCamp;
            switch (resource)
            {
                case Resource.FOOD: dict = FoodPlacements; site = Unary.Mod.Mill; break;
                case Resource.WOOD: dict = WoodPlacements; site = Unary.Mod.LumberCamp; break;
                case Resource.GOLD: dict = GoldPlacements; site = Unary.Mod.MiningCamp; break;
                case Resource.STONE: dict = StonePlacements; site = Unary.Mod.MiningCamp; break;
                default: throw new ArgumentOutOfRangeException(nameof(resource));
            }
            var size = Unary.Mod.GetBuildingWidth(site);

            var used = new HashSet<Tile>();
            var tiles = new List<KeyValuePair<Tile, double>>();
            foreach (var places in dict.Values)
            {
                foreach (var place in places)
                {
                    if (!used.Contains(place.Key))
                    {
                        used.Add(place.Key);

                        if (CanBuildAt(size, size, place.Key))
                        {
                            tiles.Add(place);
                        }
                    }
                }
            }

            return tiles;
        }

        public IEnumerable<Tile> GetBuildingPlacements(UnitType building)
        {
            var size = Unary.Mod.GetBuildingWidth(building[ObjectData.BASE_TYPE]);

            if (BuildingPlacements.TryGetValue(size, out HashSet<Tile> tiles))
            {
                return tiles.Where(t => CanBuildAt(size, size, t));
            }
            else
            {
                return Enumerable.Empty<Tile>();
            }
        }

        public bool CanBuildAt(int width, int height, Tile tile, bool ignore_exclusion = false)
        {
            var footprint = Utils.GetUnitFootprint(tile.X, tile.Y, width, height, 0);

            for (int x = footprint.X; x < footprint.Right; x++)
            {
                for (int y = footprint.Y; y < footprint.Bottom; y++)
                {
                    if (!Unary.GameState.Map.IsOnMap(x, y))
                    {
                        return false;
                    }

                    var t = Unary.GameState.Map.GetTile(x, y);

                    if (!t.Explored)
                    {
                        return false;
                    }

                    if (!t.IsOnLand)
                    {
                        return false;
                    }

                    if (!Unary.OldMapManager.CanReach(t))
                    {
                        return false;
                    }

                    if (Unary.OldMapManager.IsConstructionBlocked(t))
                    {
                        return false;
                    }

                    if (Unary.OldMapManager.IsConstructionExcluded(t) && !ignore_exclusion)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        internal override void Update()
        {
            UpdateStrategicNumbers();
            UpdateTiles();
            UpdateFarmPlacements();
            UpdateDropsitePlacements(Resource.FOOD);
            UpdateDropsitePlacements(Resource.WOOD);
            UpdateDropsitePlacements(Resource.GOLD);
            UpdateDropsitePlacements(Resource.STONE);
            UpdateBuildingPlacements();
        }

        private void UpdateStrategicNumbers()
        {
            Unary.GameState.SetStrategicNumber(StrategicNumber.DISABLE_BUILDER_ASSISTANCE, 1);
            Unary.GameState.SetStrategicNumber(StrategicNumber.ENABLE_NEW_BUILDING_SYSTEM, 1);
            Unary.GameState.SetStrategicNumber(StrategicNumber.INITIAL_EXPLORATION_REQUIRED, 0);

            Unary.GameState.SetStrategicNumber(StrategicNumber.CAP_CIVILIAN_BUILDERS, -1);
            Unary.GameState.SetStrategicNumber(StrategicNumber.MAXIMUM_TOWN_SIZE, 20);
            Unary.GameState.SetStrategicNumber(StrategicNumber.MINING_CAMP_MAX_DISTANCE, 30);
            Unary.GameState.SetStrategicNumber(StrategicNumber.LUMBER_CAMP_MAX_DISTANCE, 30);
            Unary.GameState.SetStrategicNumber(StrategicNumber.MILL_MAX_DISTANCE, 30);

            if (Unary.GameState.GetTechnology(101).State == ResearchState.COMPLETE)
            {
                Unary.GameState.SetStrategicNumber(StrategicNumber.MAXIMUM_TOWN_SIZE, 25);
                Unary.GameState.SetStrategicNumber(StrategicNumber.MINING_CAMP_MAX_DISTANCE, 35);
                Unary.GameState.SetStrategicNumber(StrategicNumber.LUMBER_CAMP_MAX_DISTANCE, 35);
                Unary.GameState.SetStrategicNumber(StrategicNumber.MILL_MAX_DISTANCE, 35);
            }

            if (Unary.GameState.GetTechnology(102).State == ResearchState.COMPLETE)
            {
                Unary.GameState.SetStrategicNumber(StrategicNumber.MAXIMUM_TOWN_SIZE, 30);
                Unary.GameState.SetStrategicNumber(StrategicNumber.MINING_CAMP_MAX_DISTANCE, 40);
                Unary.GameState.SetStrategicNumber(StrategicNumber.LUMBER_CAMP_MAX_DISTANCE, 40);
                Unary.GameState.SetStrategicNumber(StrategicNumber.MILL_MAX_DISTANCE, 40);
            }

            if (Unary.GameState.GetTechnology(103).State == ResearchState.COMPLETE)
            {
                Unary.GameState.SetStrategicNumber(StrategicNumber.MAXIMUM_TOWN_SIZE, 35);
                Unary.GameState.SetStrategicNumber(StrategicNumber.MINING_CAMP_MAX_DISTANCE, 50);
                Unary.GameState.SetStrategicNumber(StrategicNumber.LUMBER_CAMP_MAX_DISTANCE, 50);
                Unary.GameState.SetStrategicNumber(StrategicNumber.MILL_MAX_DISTANCE, 50);
            }
        }

        private void UpdateTiles()
        {
            InsideTiles.Clear();
            var map = Unary.GameState.Map;

            if (!map.IsOnMap(Unary.GameState.MyPosition))
            {
                return;
            }

            foreach (var tile in map.GetTilesInRange(Unary.GameState.MyPosition, MaximumTownSize))
            {
                InsideTiles.Add(tile);
            }
        }

        private void UpdateFarmPlacements()
        {
            FarmPlacements.Clear();

            var farm = Unary.GameState.GetUnitType(Unary.Mod.Farm);
            var dropsites = Unary.GameState.MyPlayer.Units
                .Where(u => u.Targetable && (u[ObjectData.BASE_TYPE] == Unary.Mod.TownCenter || u[ObjectData.BASE_TYPE] == Unary.Mod.Mill))
                .ToList();

            dropsites.Sort((a, b) =>
            {
                if (a[ObjectData.BASE_TYPE] == Unary.Mod.TownCenter && b[ObjectData.BASE_TYPE] == Unary.Mod.Mill)
                {
                    return -1;
                }
                else if (a[ObjectData.BASE_TYPE] == Unary.Mod.Mill && b[ObjectData.BASE_TYPE] == Unary.Mod.TownCenter)
                {
                    return 1;
                }
                else
                {
                    return a.Position.DistanceTo(Unary.GameState.MyPosition).CompareTo(b.Position.DistanceTo(Unary.GameState.MyPosition));
                }
            });

            var width = Unary.Mod.GetBuildingWidth(Unary.Mod.Farm);
            var height = width;

            foreach (var dropsite in dropsites)
            {
                var deltas = TC_FARM_DELTAS;
                if (dropsite[ObjectData.BASE_TYPE] == Unary.Mod.Mill)
                {
                    deltas = MILL_FARM_DELTAS;
                }

                foreach (var delta in deltas)
                {
                    var x = dropsite.Position.PointX + delta.X;
                    var y = dropsite.Position.PointY + delta.Y;

                    if (Unary.GameState.Map.IsOnMap(x, y))
                    {
                        var tile = Unary.GameState.Map.GetTile(x, y);

                        if (CanBuildAt(width, height, tile, true))
                        {
                            FarmPlacements.Add(tile);
                        }
                    }
                }
            }
        }

        private void UpdateDropsitePlacements(Resource resource)
        {
            var type = UnitClass.Tree;
            var site = Unary.Mod.LumberCamp;
            var dict = WoodPlacements;
            switch (resource)
            {
                case Resource.FOOD: type = UnitClass.BerryBush; site = Unary.Mod.Mill; dict = FoodPlacements; break;
                case Resource.WOOD: type = UnitClass.Tree; site = Unary.Mod.LumberCamp; dict = WoodPlacements; break;
                case Resource.GOLD: type = UnitClass.GoldMine; site = Unary.Mod.MiningCamp; dict = GoldPlacements; break;
                case Resource.STONE: type = UnitClass.StoneMine; site = Unary.Mod.MiningCamp; dict = StonePlacements; break;
                default: throw new ArgumentOutOfRangeException(nameof(resource));
            }

            var resources = Unary.GameState.Gaia.Units.Where(u => u.Targetable && u[ObjectData.CLASS] == (int)type).ToList();
            resources.Sort((a, b) => a.Position.DistanceTo(Unary.GameState.MyPosition).CompareTo(b.Position.DistanceTo(Unary.GameState.MyPosition)));

            var building = Unary.GameState.GetUnitType(site);

            foreach (var res in dict.Keys.ToList())
            {
                if (res.GetHashCode() % 101 == Unary.GameState.Tick % 101 || !res.Targetable)
                {
                    dict.Remove(res);
                }
                else
                {
                    var lst = dict[res];
                    lst.RemoveAll(kvp => !Unary.OldMapManager.CanReach(kvp.Key));
                }
            }

            var width = Unary.Mod.GetBuildingWidth(building[ObjectData.BASE_TYPE]);
            var height = width;

            var updates = 0;
            foreach (var res in resources)
            {
                if (updates >= 10)
                {
                    break;
                }

                if (!dict.ContainsKey(res))
                {
                    var tiles = new List<KeyValuePair<Tile, double>>();

                    foreach (var tile in Unary.GameState.Map.GetTilesInRange(res.Position, 4))
                    {
                        if (CanBuildAt(width, height, tile))
                        {
                            var score = GetDropsiteScore(type, tile);
                            if (score != double.MinValue)
                            {
                                tiles.Add(new KeyValuePair<Tile, double>(tile, score));
                            }
                        }
                    }

                    dict.Add(res, tiles);
                    updates++;
                }
            }
        }

        private void UpdateBuildingPlacements()
        {
            if (InsideTiles.Count == 0)
            {
                return;
            }

            Unary.Log.Debug($"Inside tiles {InsideTiles.Count}");

            var inside = InsideTiles.ToList();

            foreach (var kvp in BuildingPlacements)
            {
                var size = kvp.Key;
                var tiles = kvp.Value;

                tiles.RemoveWhere(t => t.GetHashCode() % 101 == Unary.GameState.Tick % 101 || !Unary.OldMapManager.CanReach(t));

                for (int i = 0; i < 100; i++)
                {
                    var t = inside[Unary.Rng.Next(inside.Count)];

                    if (CanBuildAt(size, size, t))
                    {
                        tiles.Add(t);
                    }
                }

                Unary.Log.Debug($"Got {tiles.Count} placements for size {size}");
            }
        }

        private double GetDropsiteScore(UnitClass resource, Tile tile)
        {
            var min_d = 0d;
            var range = 3d;
            if (resource == UnitClass.GoldMine || resource == UnitClass.StoneMine)
            {
                min_d = 2.2;
                range = 4;
            }

            var score = 0d;
            foreach (var t in Unary.GameState.Map.GetTilesInRange(tile.Position, range))
            {
                foreach (var unit in t.Units.Where(u => u.Targetable && u[ObjectData.CLASS] == (int)resource))
                {
                    score += Math.Max(0, range - t.Center.DistanceTo(tile.Position));

                    if (tile.Position.DistanceTo(t.Center) < min_d)
                    {
                        score = double.MinValue;
                    }
                }
            }

            if (score < 1)
            {
                return double.MinValue;
            }

            score -= Unary.Settings.DropsiteDistanceCost * tile.Position.DistanceTo(Unary.GameState.MyPosition);

            return score;
        }
    }
}