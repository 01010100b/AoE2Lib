using AoE2Lib;
using AoE2Lib.Bots;
using AoE2Lib.Bots.GameElements;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using Unary.UnitControllers;

namespace Unary.Managers
{
    class BuildingManager : Manager
    {
        private static readonly Point[] TC_FARM_DELTAS = new[] { new Point(2, 3), new Point(-1, 3), new Point(3, 0), new Point(3, -3), new Point(-4, 2), new Point(-4, -1), new Point(0, -4), new Point(-3, -4) };
        private static readonly Point[] MILL_FARM_DELTAS = new[] { new Point(-1, 2), new Point(2, -1), new Point(2, 2), new Point(-3, -1), new Point(-1, -3) };

        private readonly HashSet<Tile> ObstructedTiles = new();
        private readonly HashSet<Tile> ExcludedTiles = new();
        private readonly List<Tile> FarmPlacements = new();
        private readonly Dictionary<Unit, List<Tile>> WoodPlacements = new();
        private readonly Dictionary<Unit, List<Tile>> GoldPlacements = new();
        private readonly Dictionary<Unit, List<Tile>> StonePlacements = new();
        private readonly List<Unit> Foundations = new();

        public BuildingManager(Unary unary) : base(unary)
        {

        }

        public bool IsObstructed(Tile tile)
        {
            if (!tile.IsOnLand)
            {
                return true;
            }

            return ObstructedTiles.Contains(tile);
        }

        public IEnumerable<Tile> GetObstructedTiles()
        {
            return ObstructedTiles;
        }

        public bool IsExcluded(Tile tile)
        {
            if (!tile.IsOnLand)
            {
                return true;
            }

            return ExcludedTiles.Contains(tile);
        }

        public IReadOnlyList<Tile> GetFarmPlacements()
        {
            return FarmPlacements;
        }

        public List<Tile> GetDropsitePlacements(Resource resource)
        {
            var dict = WoodPlacements;
            switch (resource)
            {
                case Resource.WOOD: dict = WoodPlacements; break;
                case Resource.GOLD: dict = GoldPlacements; break;
                case Resource.STONE: dict = StonePlacements; break;
                default: throw new ArgumentOutOfRangeException(nameof(resource));
            }

            var tiles = new List<Tile>();
            foreach (var places in dict.Values)
            {
                tiles.AddRange(places);
            }

            return tiles;
        }

        public IReadOnlyList<Unit> GetFoundations()
        {
            return Foundations;
        }

        public bool CanBuildAt(UnitType type, Tile tile, bool ignore_exclusion = false)
        {
            var footprint = GetUnitFootprint(type[ObjectData.BASE_TYPE], tile, 0);

            for (int x = footprint.X; x < footprint.Right; x++)
            {
                for (int y = footprint.Y; y < footprint.Bottom; y++)
                {
                    if (!Unary.GameState.Map.IsOnMap(x, y))
                    {
                        return false;
                    }

                    var t = Unary.GameState.Map.GetTile(x, y);

                    if (IsObstructed(t))
                    {
                        return false;
                    }

                    if (IsExcluded(t) && !ignore_exclusion)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public Rectangle GetUnitFootprint(int base_type_id, Tile tile, int exclusion_zone_size)
        {
            var width = Unary.Mod.GetBuildingSize(base_type_id);
            var height = Unary.Mod.GetBuildingSize(base_type_id);
            var x = tile.X;
            var y = tile.Y;

            width += 2 * exclusion_zone_size;
            height += 2 * exclusion_zone_size;

            var x_start = x - (width / 2);
            var x_end = x + (width / 2);
            if (width % 2 == 0)
            {
                x_end--;
            }

            var y_start = y - (height / 2);
            var y_end = y + (height / 2);
            if (height % 2 == 0)
            {
                y_end--;
            }

            return new Rectangle(x_start, y_start, x_end - x_start + 1, y_end - y_start + 1);
        }

        public int GetExclusionZoneSize(int base_type_id)
        {
            if (base_type_id == Unary.Mod.TownCenter || base_type_id == Unary.Mod.TownCenterFoundation || base_type_id == Unary.Mod.Mill)
            {
                return 3;
            }
            else if (base_type_id == Unary.Mod.LumberCamp || base_type_id == Unary.Mod.MiningCamp || base_type_id == Unary.Mod.Dock)
            {
                return 3;
            }
            else
            {
                return 1;
            }
        }

        internal override void Update()
        {
            UpdateStrategicNumbers();
            UpdateTiles();
            UpdateFarmPlacements();
            UpdateBuilders();
        }

        private void UpdateStrategicNumbers()
        {
            Unary.GameState.SetStrategicNumber(StrategicNumber.DISABLE_BUILDER_ASSISTANCE, 1);
            Unary.GameState.SetStrategicNumber(StrategicNumber.ENABLE_NEW_BUILDING_SYSTEM, 1);
            Unary.GameState.SetStrategicNumber(StrategicNumber.INITIAL_EXPLORATION_REQUIRED, 0);

            Unary.GameState.SetStrategicNumber(StrategicNumber.CAP_CIVILIAN_BUILDERS, 4);
            Unary.GameState.SetStrategicNumber(StrategicNumber.MAXIMUM_TOWN_SIZE, 20);
            Unary.GameState.SetStrategicNumber(StrategicNumber.MINING_CAMP_MAX_DISTANCE, 30);
            Unary.GameState.SetStrategicNumber(StrategicNumber.LUMBER_CAMP_MAX_DISTANCE, 30);
            Unary.GameState.SetStrategicNumber(StrategicNumber.MILL_MAX_DISTANCE, 30);

            if (Unary.GameState.GetTechnology(101).State == ResearchState.COMPLETE)
            {
                Unary.GameState.SetStrategicNumber(StrategicNumber.CAP_CIVILIAN_BUILDERS, 8);
                Unary.GameState.SetStrategicNumber(StrategicNumber.MAXIMUM_TOWN_SIZE, 25);
                Unary.GameState.SetStrategicNumber(StrategicNumber.MINING_CAMP_MAX_DISTANCE, 35);
                Unary.GameState.SetStrategicNumber(StrategicNumber.LUMBER_CAMP_MAX_DISTANCE, 35);
                Unary.GameState.SetStrategicNumber(StrategicNumber.MILL_MAX_DISTANCE, 35);
            }

            if (Unary.GameState.GetTechnology(102).State == ResearchState.COMPLETE)
            {
                Unary.GameState.SetStrategicNumber(StrategicNumber.CAP_CIVILIAN_BUILDERS, 12);
                Unary.GameState.SetStrategicNumber(StrategicNumber.MAXIMUM_TOWN_SIZE, 30);
                Unary.GameState.SetStrategicNumber(StrategicNumber.MINING_CAMP_MAX_DISTANCE, 40);
                Unary.GameState.SetStrategicNumber(StrategicNumber.LUMBER_CAMP_MAX_DISTANCE, 40);
                Unary.GameState.SetStrategicNumber(StrategicNumber.MILL_MAX_DISTANCE, 40);
            }

            if (Unary.GameState.GetTechnology(103).State == ResearchState.COMPLETE)
            {
                Unary.GameState.SetStrategicNumber(StrategicNumber.CAP_CIVILIAN_BUILDERS, 16);
                Unary.GameState.SetStrategicNumber(StrategicNumber.MAXIMUM_TOWN_SIZE, 35);
                Unary.GameState.SetStrategicNumber(StrategicNumber.MINING_CAMP_MAX_DISTANCE, 50);
                Unary.GameState.SetStrategicNumber(StrategicNumber.LUMBER_CAMP_MAX_DISTANCE, 50);
                Unary.GameState.SetStrategicNumber(StrategicNumber.MILL_MAX_DISTANCE, 50);
            }
        }

        private void UpdateTiles()
        {
            ObstructedTiles.Clear();
            ExcludedTiles.Clear();

            var map = Unary.GameState.Map;
            foreach (var player in Unary.GameState.GetPlayers())
            {
                foreach (var unit in player.Units.Where(u => BlocksConstruction(u)))
                {
                    var footprint = GetUnitFootprint(unit[ObjectData.BASE_TYPE], unit.Tile, 0);
                    for (int x = footprint.X; x < footprint.Right; x++)
                    {
                        for (int y = footprint.Y; y < footprint.Bottom; y++)
                        {
                            if (map.IsOnMap(x, y))
                            {
                                ObstructedTiles.Add(map.GetTile(x, y));
                            }
                        }
                    }

                    if (unit[ObjectData.CMDID] == (int)CmdId.CIVILIAN_BUILDING || unit[ObjectData.CMDID] == (int)CmdId.MILITARY_BUILDING)
                    {
                        var excl = GetExclusionZoneSize(unit[ObjectData.BASE_TYPE]);
                        footprint = GetUnitFootprint(unit[ObjectData.BASE_TYPE], unit.Tile, excl);
                        for (int x = footprint.X; x < footprint.Right; x++)
                        {
                            for (int y = footprint.Y; y < footprint.Bottom; y++)
                            {
                                if (map.IsOnMap(x, y))
                                {
                                    ExcludedTiles.Add(map.GetTile(x, y));
                                }
                            }
                        }
                    }
                }
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
                    return a.FirstUpdateGameTime.CompareTo(b.FirstUpdateGameTime);
                }
            });

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

                        if (CanBuildAt(farm, tile, true))
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
                if (res.GetHashCode() % 100 == Unary.GameState.Tick % 100)
                {
                    dict.Remove(res);
                }
            }

            var updates = 0;
            foreach (var res in resources)
            {
                if (updates >= 10)
                {
                    break;
                }

                if (!dict.ContainsKey(res))
                {
                    var tiles = new List<Tile>();

                    foreach (var tile in Unary.GameState.Map.GetTilesInRange(res.Position, 5))
                    {
                        if (CanBuildAt(building, tile))
                        {
                            tiles.Add(tile);
                        }
                    }

                    dict.Add(res, tiles);
                    updates++;
                }
            }
        }

        private void UpdateBuilders()
        {
            Foundations.Clear();

            foreach (var unit in Unary.GameState.MyPlayer.Units.Where(u => u.Targetable && u[ObjectData.STATUS] == 0))
            {
                if (unit[ObjectData.CMDID] == (int)CmdId.CIVILIAN_BUILDING || unit[ObjectData.CMDID] == (int)CmdId.MILITARY_BUILDING)
                {
                    Foundations.Add(unit);
                }
            }

            var max_builders = Math.Min(4, Foundations.Count);
            var builders = Unary.UnitsManager.GetControllers<BuilderController>().Count;
            if (builders < max_builders)
            {
                var foundation = Foundations[Unary.Rng.Next(Foundations.Count)];
                var gatherers = Unary.UnitsManager.GetControllers<GathererController>();
                gatherers.Sort((a, b) => a.Unit.Position.DistanceTo(foundation.Position).CompareTo(b.Unit.Position.DistanceTo(foundation.Position)));

                if (gatherers.Count > 0)
                {
                    var builder = gatherers[0].Unit;
                    var ctrl = new BuilderController(builder, Unary);
                    Unary.UnitsManager.SetController(builder, ctrl);
                }
            }
        }

        private bool BlocksConstruction(Unit unit)
        {
            if (!unit.Targetable)
            {
                return false;
            }

            if (unit[ObjectData.SPEED] > 0)
            {
                return false;
            }

            return true;
        }
    }
}
