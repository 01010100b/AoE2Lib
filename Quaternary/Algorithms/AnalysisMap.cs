using AoE2Lib.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Quaternary.Algorithms
{
    public class AnalysisMap
    {
        public static int WallDistance(Point a, Point b)
        {
            return Math.Max(Math.Abs(a.X - b.X), Math.Abs(a.Y - b.Y));
        }

        public static List<Point> GetLine(Point a, Point b)
        {
            List<Point> points = new List<Point>();

            // no slope (vertical line)
            if (a.X == b.X)
            {
                if (a.Y > b.Y)
                {
                    var temp = a;
                    a = b;
                    b = temp;
                }

                for (var y = a.Y; y <= b.Y; y++)
                {
                    Point p = new Point(a.X, y);
                    points.Add(p);
                }
            }
            else
            {
                var dx = Math.Abs(b.X - a.X);
                var sx = a.X < b.X ? 1 : -1;
                var dy = -Math.Abs(b.Y - a.Y);
                var sy = a.Y < b.Y ? 1 : -1;
                var err = dx + dy;

                while (true)
                {
                    points.Add(a);

                    if (Math.Max(Math.Abs(a.X - b.X), Math.Abs(a.Y - b.Y)) <= 1)
                    {
                        break;
                    }

                    var e2 = 2 * err;
                    if (e2 >= dy)
                    {
                        err += dy;
                        a.X += sx;
                    }
                    if (e2 <= dx)
                    {
                        err += dx;
                        a.Y += sy;
                    }
                }
            }

            return points;
        }

        public enum AnalysisTileType
        {
            NONE, WALL, WOOD, FOOD, GOLD, STONE, OBSTRUCTION, GOAL, INTERIOR
        }

        public struct AnalysisTile
        {
            public Point Point { get; set; }
            public bool IsResource => Type == AnalysisTileType.WOOD || Type == AnalysisTileType.FOOD || Type == AnalysisTileType.GOLD || Type == AnalysisTileType.STONE;
            public AnalysisTileType Type { get; set; }
        }

        public AnalysisTile[,] Tiles { get; set; } = null;
        public int Size
        {
            get
            {
                if (Tiles == null)
                {
                    return -1;
                }

                var size = (int)Math.Ceiling(Math.Sqrt(Tiles.Length));
                while (Tiles.Length > size * size)
                {
                    size--;
                }

                return size;
            }
        }

        public Point Center => new Point(Size / 2, Size / 2);

        private readonly Random RNG = new Random(Guid.NewGuid().GetHashCode() ^ DateTime.UtcNow.GetHashCode());

        public void Generate(int size)
        {
            Tiles = new AnalysisTile[size, size];
            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    Tiles[x, y] = new AnalysisTile() { Point = new Point(x, y) };
                }
            }

            GenerateResourceClump(AnalysisTileType.WOOD, 12, 18, 30, 50);
            GenerateResourceClump(AnalysisTileType.WOOD, 12, 18, 30, 50);
            for (int i = 0; i < 1; i++)
            {
                GenerateResourceClump(AnalysisTileType.WOOD, 16, 24, 40, 60);
            }

            GenerateResourceClump(AnalysisTileType.GOLD, 10, 16, 7, 7);
            GenerateResourceClump(AnalysisTileType.GOLD, 16, 24, 4, 4);
            GenerateResourceClump(AnalysisTileType.GOLD, 16, 24, 4, 4);

            GenerateResourceClump(AnalysisTileType.STONE, 10, 16, 5, 5);
            GenerateResourceClump(AnalysisTileType.STONE, 15, 20, 4, 4);

            GenerateResourceClump(AnalysisTileType.FOOD, 8, 12, 6, 6);
        }

        public List<HashSet<Point>> GetResourceClumps()
        {
            var clumps = new List<HashSet<Point>>();

            foreach (var tile in Tiles)
            {
                var pos = tile.Point;

                if (tile.IsResource && clumps.Count(c => c.Contains(pos)) == 0)
                {
                    var clump = new HashSet<Point>();
                    foreach (var p in FloodFill.GetInterior(pos, p => GetNeighbours(p, true), p => Tiles[p.X, p.Y].Type == tile.Type))
                    {
                        clump.Add(p);
                    }

                    clumps.Add(clump);
                }
            }

            return clumps;
        }

        public IEnumerable<Point> GetNeighbours(Point a, bool diagonal = false)
        {
            var size = Size;

            if (size < 1)
            {
                yield break;
            }

            var x = a.X - 1;
            var y = a.Y;
            if (x >= 0)
            {
                yield return new Point(x, y);
            }

            x = a.X + 1;
            y = a.Y;
            if (x < size)
            {
                yield return new Point(x, y);
            }

            x = a.X;
            y = a.Y - 1;
            if (y >= 0)
            {
                yield return new Point(x, y);
            }

            x = a.X;
            y = a.Y + 1;
            if (y < size)
            {
                yield return new Point(x, y);
            }

            if (diagonal)
            {
                x = a.X - 1;
                y = a.Y - 1;
                if (x >= 0 && y >= 0)
                {
                    yield return new Point(x, y);
                }

                x = a.X - 1;
                y = a.Y + 1;
                if (x >= 0 && y < size)
                {
                    yield return new Point(x, y);
                }

                x = a.X + 1;
                y = a.Y - 1;
                if (x < size && y >= 0)
                {
                    yield return new Point(x, y);
                }

                x = a.X + 1;
                y = a.Y + 1;
                if (x < size && y < size)
                {
                    yield return new Point(x, y);
                }
            }
        }

        private void GenerateResourceClump(AnalysisTileType resource, double min_distance, double max_distance, int min_count, int max_count)
        {
            var size = Size;
            if (size < 1)
            {
                return;
            }

            var center = Position.FromPoint(Center.X, Center.Y);
            var pos = Position.FromPoint(RNG.Next(size), RNG.Next(size));
            
            var resources = new HashSet<Point>();
            foreach (var tile in Tiles)
            {
                if (tile.IsResource)
                {
                    resources.Add(tile.Point);
                }
            }

            var md = double.MinValue;
            while (md < (resource == AnalysisTileType.WOOD ? 5 : 5))
            {
                pos = Position.FromPoint(RNG.Next(size), RNG.Next(size));

                while (pos.DistanceTo(center) < min_distance || pos.DistanceTo(center) > max_distance)
                {
                    pos = Position.FromPoint(RNG.Next(size), RNG.Next(size));
                }

                if (resources.Count == 0)
                {
                    break;
                }

                md = resources.Min(p => pos.DistanceTo(Position.FromPoint(p.X, p.Y)));
            }

            resources.Clear();

            resources.Add(new Point(pos.PointX, pos.PointY));

            var count = RNG.Next(min_count, max_count + 1);
            var neighbours = new List<Point>();
            while (resources.Count < count)
            {
                var p = resources.ElementAt(RNG.Next(resources.Count));

                neighbours.Clear();
                neighbours.AddRange(GetNeighbours(p, false));

                if (neighbours.Count > 0)
                {
                    for (int i = 0; i < 5; i++)
                    {
                        p = neighbours[RNG.Next(neighbours.Count)];

                        if (!resources.Contains(p))
                        {
                            resources.Add(p);
                            break;
                        }
                    }
                }
            }

            foreach (var p in resources)
            {
                var tile = Tiles[p.X, p.Y];
                tile.Type = resource;
                Tiles[p.X, p.Y] = tile;
            }
        }

        
    }
}
