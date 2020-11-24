using AoE2Lib;
using AoE2Lib.Bots;
using AoE2Lib.Bots.Modules;
using AoE2Lib.Mods;
using AoE2Lib.Utils;
using Protos.Expert.Fact;
using Quaternary.Algorithms;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Quaternary.Modules.MapAnalysisModule;

namespace Quaternary.Modules
{
    class WallingModule : Module
    {
        public class Wall
        {
            // this will be set to true after the wall has been generated
            public bool IsGenerated { get; internal set; }

            // the following lists will be filled when the wall is generated
            public readonly List<AnalysisTile> Pieces = new List<AnalysisTile>();
            public readonly List<KeyValuePair<AnalysisTile, UnitDef>> Gates = new List<KeyValuePair<AnalysisTile, UnitDef>>();

            internal readonly List<Point> Goals = new List<Point>();
            internal readonly int Optimizations;
            
            private AnalysisTile[] Tiles;
            private Point Offset;

            internal Wall(IEnumerable<Point> goals, int optimizations)
            {
                Goals.AddRange(goals);
                Optimizations = optimizations;
            }

            internal bool IsOnMap(Point point)
            {
                var size = (int)Math.Round(Math.Sqrt(Tiles.Length));
                var p = new Point(point.X - Offset.X, point.Y - Offset.Y);

                return p.X >= 0 && p.X < size && p.Y >= 0 && p.Y < size;
            }

            internal AnalysisTile GetTile(Point point)
            {
                var index = GetIndex(point);

                return Tiles[index];
            }

            internal void SetTile(AnalysisTile tile)
            {
                var index = GetIndex(tile.Point);
                Tiles[index] = tile;
            }

            internal void SetSize(int size)
            {
                Tiles = new AnalysisTile[size * size];
            }

            internal void SetOffset(Point offset)
            {
                Offset = offset;
            }

            internal void Reset()
            {
                Pieces.Clear();
                Gates.Clear();
            }

            private int GetIndex(Point point)
            {
                var p = new Point(point.X - Offset.X, point.Y - Offset.Y);

                return ((int)Math.Round(Math.Sqrt(Tiles.Length)) * p.X) + p.Y;
            }
        }

        internal class Neighbourhood
        {
            public enum Type
            {
                CLEAR, BLOCKED
            }

            public int Hash { get; private set; }
            public bool Valid { get; private set; }
            public bool Remove { get; private set; }
            public bool MoveXPos { get; private set; }
            public bool MoveXNeg { get; private set; }
            public bool MoveYPos { get; private set; }
            public bool MoveYNeg { get; private set; }
            public bool GateHorizontal { get; private set; }
            public bool GateVertical { get; private set; }

            public Type GetType(int x, int y)
            {
                var index = GetIndex(x, y);

                var a = Hash >> index;
                a &= 1;

                return (Type)a;
            }

            public void SetType(int x, int y, Type type)
            {
                var index = GetIndex(x, y);

                var a = 1;
                a <<= index;
                Hash &= ~a;

                a = (int)type;
                a <<= index;
                Hash |= a;
            }

            public void Recheck()
            {
                Valid = GetType(1, 1) == Type.BLOCKED;

                Remove = true;
                MoveXPos = true;
                MoveXNeg = true;
                MoveYPos = true;
                MoveYNeg = true;

                var components = GetClearComponents();
                if (components.Count > 1)
                {
                    SetType(1, 1, Type.CLEAR);
                    var ccomponents = GetClearComponents();
                    SetType(1, 1, Type.BLOCKED);

                    if (SpilledComponents(components, ccomponents))
                    {
                        Remove = false;
                    }

                    for (int i = 0; i < 4; i++)
                    {
                        var x = 2;
                        var y = 1;

                        if (i == 1)
                        {
                            x = 0;
                        }
                        else if (i == 2)
                        {
                            x = 1;
                            y = 2;
                        }
                        else if (i == 3)
                        {
                            x = 1;
                            y = 0;
                        }

                        var prev = GetType(x, y);
                        SetType(1, 1, Type.CLEAR);
                        SetType(x, y, Type.BLOCKED);
                        ccomponents = GetClearComponents();
                        SetType(1, 1, Type.BLOCKED);
                        SetType(x, y, prev);

                        if (SpilledComponents(components, ccomponents) || prev == Type.BLOCKED)
                        {
                            if (x == 2)
                            {
                                MoveXPos = false;
                            }
                            else if (x == 0)
                            {
                                MoveXNeg = false;
                            }
                            else if (y == 2)
                            {
                                MoveYPos = false;
                            }
                            else if (y == 0)
                            {
                                MoveYNeg = false;
                            }
                        }
                    }
                }

                if (GetType(0, 1) == Type.CLEAR && GetType(2, 1) == Type.CLEAR)
                {
                    GateVertical = true;
                }

                if (GetType(1, 0) == Type.CLEAR && GetType(1, 2) == Type.CLEAR)
                {
                    GateHorizontal = true;
                }
            }

            public List<HashSet<Point>> GetClearComponents()
            {
                var components = new List<HashSet<Point>>();

                for (int x = 0; x < 3; x++)
                {
                    for (int y = 0; y < 3; y++)
                    {
                        if (GetType(x, y) == Type.CLEAR)
                        {
                            var point = new Point(x, y);

                            if (components.Count(c => c.Contains(point)) == 0)
                            {
                                var component = new HashSet<Point>();
                                component.Add(point);

                                foreach (var interior in FloodFill.GetRegion(point, false, p => IsInNeighbourhood(p) && GetType(p.X, p.Y) == Type.CLEAR))
                                {
                                    component.Add(interior);
                                }

                                components.Add(component);
                            }
                        }
                    }
                }

                return components;
            }

            public override bool Equals(object obj)
            {
                if (obj is Neighbourhood other)
                {
                    return Hash == other.Hash;
                }
                else
                {
                    return false;
                }
            }

            public override int GetHashCode()
            {
                return Hash.GetHashCode();
            }

            private int GetIndex(int x, int y)
            {
                return (x * 3) + y;
            }

            private IEnumerable<Point> GetNeighbours(Point point)
            {
                var x = point.X;
                var y = point.Y;

                if (x > 0)
                {
                    yield return new Point(x - 1, y);
                }
                if (x < 2)
                {
                    yield return new Point(x + 1, y);
                }
                if (y > 0)
                {
                    yield return new Point(x, y - 1);
                }
                if (y < 2)
                {
                    yield return new Point(x, y + 1);
                }
            }

            private bool IsInNeighbourhood(Point point)
            {
                return point.X >= 0 && point.X < 3 && point.Y >= 0 && point.Y < 3;
            }

            private bool SpilledComponents(List<HashSet<Point>> first, List<HashSet<Point>> second)
            {
                var old = new List<HashSet<Point>>();
                foreach (var component in second)
                {
                    old.Clear();
                    foreach (var p in component)
                    {
                        foreach (var c in first)
                        {
                            if (c.Contains(p) && !old.Contains(c))
                            {
                                old.Add(c);
                            }
                        }
                    }

                    if (old.Count > 1)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public IEnumerable<Point> GetGoals(Position center, double min_range, IEnumerable<List<AnalysisTile>> clumps)
        {
            yield return new Position(center.X - min_range, center.Y);
            yield return new Position(center.X + min_range, center.Y);
            yield return new Position(center.X, center.Y - min_range);
            yield return new Position(center.X, center.Y + min_range);

            var diag = 0.71 * min_range;
            yield return new Position(center.X - diag, center.Y - diag);
            yield return new Position(center.X + diag, center.Y - diag);
            yield return new Position(center.X - diag, center.Y + diag);
            yield return new Position(center.X + diag, center.Y + diag);

            var angles = new Dictionary<Point, double>();

            foreach (var clump in clumps)
            {
                if (clump[0].Resource != Resource.NONE && clump.Count < 10)
                {
                    var minx = int.MaxValue;
                    var maxx = int.MinValue;
                    var miny = int.MaxValue;
                    var maxy = int.MinValue;

                    foreach (var tile in clump)
                    {
                        minx = Math.Min(minx, tile.Point.X) - 2;
                        maxx = Math.Max(maxx, tile.Point.X) + 2;
                        miny = Math.Min(miny, tile.Point.Y) - 2;
                        maxy = Math.Max(maxy, tile.Point.Y) + 2;
                    }

                    yield return new Point(minx, miny);
                    yield return new Point(minx, maxy);
                    yield return new Point(maxx, miny);
                    yield return new Point(maxx, maxy);
                }
                else
                {
                    angles.Clear();

                    foreach (var tile in clump)
                    {
                        angles[tile.Point] = (tile.Point - center).Angle;
                    }

                    clump.Sort((a, b) => angles[a.Point].CompareTo(angles[b.Point]));

                    var first = clump[0].Point;
                    var last = clump[clump.Count - 1].Point;

                    if (angles[first] < Math.PI / 2 && angles[last] > 1.5 * Math.PI)
                    {
                        foreach (var point in angles.Keys.ToList())
                        {
                            if (angles[point] > Math.PI)
                            {
                                angles[point] -= 2 * Math.PI;
                            }
                        }

                        clump.Sort((a, b) => angles[a.Point].CompareTo(angles[b.Point]));

                        first = clump[0].Point;
                        last = clump[clump.Count - 1].Point;
                    }

                    yield return first;
                    yield return last;
                }
            }
        }

        public Wall GetWall(IEnumerable<Point> goals, int optimizations)
        {
            var map = Bot.GetModule<MapAnalysisModule>();
            var wall = new Wall(goals.Where(g => map.IsOnMap(g)), optimizations);

            if (wall.Goals.Count == 0)
            {
                return null;
            }

            var center = Position.Zero;
            foreach (var goal in wall.Goals)
            {
                center += goal;
            }
            center /= wall.Goals.Count;

            var range = wall.Goals.Max(g => center.DistanceTo(g));
            var size = (int)Math.Ceiling(range * 4);
            if (size < 10)
            {
                size = 10;
            }

            wall.SetSize(size);
            var offset = new Point(center.PointX - (size / 2), center.PointY - (size / 2));
            wall.SetOffset(offset);
            for (int x = offset.X; x < offset.X + size; x++)
            {
                for (int y = offset.Y; y < offset.Y + size; y++)
                {
                    var p = new Point(x, y);
                    if (map.IsOnMap(p))
                    {
                        wall.SetTile(map.GetTile(p));
                    }
                    else
                    {
                        var tile = new AnalysisTile(p, true, 0, 0, true, Resource.NONE);
                    }
                }
            }

            GenerateWall(wall);

            return wall;
        }

        protected override IEnumerable<Command> RequestUpdate()
        {
            return Enumerable.Empty<Command>();
        }

        protected override void Update()
        {

        }

        private void GenerateWall(Wall wall)
        {
            wall.Reset();

            wall.Pieces.AddRange(GrahamScan.GetConvexHull(wall.Goals).Select(p => wall.GetTile(p)));
            wall.Pieces.RemoveAll(t => t.Obstructed);

            Debug.WriteLine($"Wall length {wall.Pieces.Count}");

            GenerateGates(wall);

            wall.IsGenerated = true;
        }

        private void GenerateGates(Wall wall)
        {
            var rng = new Random(Guid.NewGuid().GetHashCode());
            var horizontal = Bot.Mod.GateHorizontal;
            var vertical = Bot.Mod.GateVertical;
            var map = Bot.GetModule<MapModule>();
            var placement = Bot.GetModule<PlacementModule>();

            while (wall.Gates.Count < 4)
            {
                var piece = wall.Pieces[rng.Next(wall.Pieces.Count)];

                if (wall.Gates.Count > 0 && wall.Gates.Min(g => ((Position)piece.Point).DistanceTo(g.Key.Point)) < 5)
                {
                    continue;
                }

                var p1 = new Point(piece.Point.X, piece.Point.Y - 1);
                var p2 = new Point(piece.Point.X, piece.Point.Y + 1);

                if (wall.IsOnMap(p1) && wall.GetTile(p1).Obstructed == false && wall.IsOnMap(p2) && wall.GetTile(p2).Obstructed == false)
                {
                    var good = true;
                    foreach (var p in wall.Pieces)
                    {
                        if (p.Point == p1 || p.Point == p2)
                        {
                            good = false;
                            break;
                        }
                    }

                    if (good && placement.CanBuildAtPosition(map, horizontal, piece.Point, 0, false))
                    {
                        wall.Gates.Add(new KeyValuePair<AnalysisTile, UnitDef>(piece, horizontal));
                        foreach (var pos in PlacementModule.GetFootprint(piece.Point, horizontal.Width, horizontal.Height, 0))
                        {
                            wall.Pieces.RemoveAll(p => p.Point.X == pos.PointX && p.Point.Y == pos.PointY);
                        }
                    }
                }
                else
                {
                    p1 = new Point(piece.Point.X - 1, piece.Point.Y);
                    p2 = new Point(piece.Point.X + 1, piece.Point.Y);

                    if (wall.IsOnMap(p1) && wall.GetTile(p1).Obstructed == false && wall.IsOnMap(p2) && wall.GetTile(p2).Obstructed == false)
                    {
                        var good = true;
                        foreach (var p in wall.Pieces)
                        {
                            if (p.Point == p1 || p.Point == p2)
                            {
                                good = false;
                                break;
                            }
                        }

                        if (good && placement.CanBuildAtPosition(map, vertical, piece.Point, 0, false))
                        {
                            wall.Gates.Add(new KeyValuePair<AnalysisTile, UnitDef>(piece, vertical));
                            foreach (var pos in PlacementModule.GetFootprint(piece.Point, vertical.Width, vertical.Height, 0))
                            {
                                wall.Pieces.RemoveAll(p => p.Point.X == pos.PointX && p.Point.Y == pos.PointY);
                            }
                        }
                    }
                }

                //Debug.WriteLine($"Checking neighbourhood");
            }
        }

        private Neighbourhood GetNeighbourhood(Wall wall, Point point)
        {
            var neighbourhood = new Neighbourhood();
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    var p = new Point(point.X + dx, point.Y + dy);

                    if (wall.IsOnMap(p))
                    {
                        var tile = wall.GetTile(p);

                        if (tile.Obstructed || wall.Pieces.Contains(tile))
                        {
                            neighbourhood.SetType(dx + 1, dy + 1, Neighbourhood.Type.BLOCKED);
                        }
                    }
                    else
                    {
                        neighbourhood.SetType(dx + 1, dy + 1, Neighbourhood.Type.BLOCKED);
                    }
                }
            }

            neighbourhood.Recheck();

            return neighbourhood;
        }
    }
}
