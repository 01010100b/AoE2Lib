using AoE2Lib;
using AoE2Lib.Bots;
using AoE2Lib.Bots.Modules;
using Quaternary.Algorithms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace Quaternary.Modules
{
    class MapAnalysisModule : Module
    {
        public struct AnalysisTile
        {
            public readonly Point Point;
            public readonly bool Explored;
            public readonly int Elevation;
            public readonly int Terrain;
            public readonly bool Obstructed;
            public readonly Resource Resource;

            internal AnalysisTile(Point point, bool explored, int elevation, int terrain, bool obstructed, Resource resource)
            {
                Point = point;
                Explored = explored;
                Elevation = elevation;
                Terrain = terrain;
                Obstructed = obstructed;
                Resource = resource;
            }
        }

        public int Width { get; private set; } = 0;
        public int Height { get; private set; } = 0;
        public IReadOnlyDictionary<Resource, List<List<AnalysisTile>>> Clumps => _Clumps;
        private readonly Dictionary<Resource, List<List<AnalysisTile>>> _Clumps = new Dictionary<Resource, List<List<AnalysisTile>>>();
        private AnalysisTile[] Tiles { get; set; } = new AnalysisTile[0];

        public bool IsOnMap(Point point)
        {
            var index = GetIndex(point);

            return index >= 0 && index < Tiles.Length;
        }

        public AnalysisTile GetTile(Point point)
        {
            if (!IsOnMap(point))
            {
                throw new Exception($"Point {point} is not on the map.");
            }

            var index = GetIndex(point);

            return Tiles[index];
        }

        public IEnumerable<AnalysisTile> GetTiles()
        {
            return Tiles;
        }

        private void SetTile(AnalysisTile tile)
        {
            if (!IsOnMap(tile.Point))
            {
                throw new Exception($"Point {tile.Point} is not on the map.");
            }

            var index = GetIndex(tile.Point);
            Tiles[index] = tile;
        }

        private int GetIndex(Point point)
        {
            return (point.X * Height) + point.Y;
        }

        protected override IEnumerable<Command> RequestUpdate()
        {
            return Enumerable.Empty<Command>();
        }

        protected override void Update()
        {
            var map = Bot.GetModule<MapModule>();

            if (map.Width != Width || map.Height != Height)
            {
                Width = map.Width;
                Height = map.Height;
                Tiles = new AnalysisTile[Width * Height];
            }

            foreach (var tile in map.GetTiles())
            {
                var point = tile.Point;
                var explored = tile.Explored;
                var elevation = tile.Elevation;
                var terrain = tile.Terrain;

                SetTile(new AnalysisTile(point, explored, elevation, terrain, false, Resource.NONE));
            }

            var units = Bot.GetModule<UnitsModule>();

            foreach (var unit in units.Units.Values.Where(u => u.Exists))
            {
                if (PlacementModule.IsUnitClassObstruction(unit.Class))
                {
                    var tile = GetTile(unit.Position);
                    var ntile = new AnalysisTile(tile.Point, tile.Explored, tile.Elevation, tile.Terrain, true, tile.Resource);
                    SetTile(ntile);
                }

                if (unit.Class == UnitClass.Tree)
                {
                    var tile = GetTile(unit.Position);
                    SetTile(new AnalysisTile(tile.Point, tile.Explored, tile.Elevation, tile.Terrain, tile.Obstructed, Resource.WOOD));
                }
                else if (unit.Class == UnitClass.BerryBush || unit.Class == UnitClass.ShoreFish || unit.Class == UnitClass.OceanFish || unit.Class == UnitClass.DeepSeaFish)
                {
                    var tile = GetTile(unit.Position);
                    SetTile(new AnalysisTile(tile.Point, tile.Explored, tile.Elevation, tile.Terrain, tile.Obstructed, Resource.FOOD));
                }
                else if (unit.Class == UnitClass.GoldMine)
                {
                    var tile = GetTile(unit.Position);
                    SetTile(new AnalysisTile(tile.Point, tile.Explored, tile.Elevation, tile.Terrain, tile.Obstructed, Resource.GOLD));
                }
                else if (unit.Class == UnitClass.StoneMine)
                {
                    var tile = GetTile(unit.Position);
                    SetTile(new AnalysisTile(tile.Point, tile.Explored, tile.Elevation, tile.Terrain, tile.Obstructed, Resource.STONE));
                }
                else if (unit.Class == UnitClass.PreyAnimal)
                {
                    var tile = GetTile(unit.Position);
                    SetTile(new AnalysisTile(tile.Point, tile.Explored, tile.Elevation, tile.Terrain, tile.Obstructed, Resource.DEER));
                }
                else if (unit.Class == UnitClass.PredatorAnimal)
                {
                    var tile = GetTile(unit.Position);
                    SetTile(new AnalysisTile(tile.Point, tile.Explored, tile.Elevation, tile.Terrain, tile.Obstructed, Resource.BOAR));
                }
            }

            var obstructions = new Dictionary<Resource, HashSet<AnalysisTile>>();

            foreach (var tile in GetTiles())
            {
                if (!obstructions.ContainsKey(tile.Resource))
                {
                    obstructions.Add(tile.Resource, new HashSet<AnalysisTile>());
                }

                if (tile.Obstructed)
                {
                    obstructions[tile.Resource].Add(tile);
                }
            }

            _Clumps.Clear();
            foreach (var resource in obstructions.Keys)
            {
                var clumps = new List<List<AnalysisTile>>();
                var tiles = obstructions[resource];

                while (tiles.Count > 0)
                {
                    var clump = new List<AnalysisTile>();

                    var start = tiles.First();
                    foreach (var point in FloodFill.GetRegion(start.Point, true, p => IsPointIncluded(p, resource)))
                    {
                        var tile = GetTile(point);
                        clump.Add(tile);
                        tiles.Remove(tile);
                    }

                    clumps.Add(clump);
                }

                _Clumps.Add(resource, clumps);
            }

            Bot.Log.Info($"MapAnalysisModule: updated {Tiles.Length} tiles with {Clumps.Values.Sum(c => c.Count)} clumps");
        }

        private bool IsPointIncluded(Point point, Resource resource)
        {
            if (point.X < 0 || point.X >= Width || point.Y < 0 || point.Y >= Height)
            {
                return false;
            }

            var tile = GetTile(point);

            return tile.Obstructed && tile.Resource == resource;
        }
    }
}
