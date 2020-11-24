using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using static Quaternary.Algorithms.AnalysisMap;

namespace Quaternary.Algorithms
{
    public static class Walling
    {
        private class Neighbourhood
        {
            public int Hash { get; private set; }
            public bool Valid { get; private set; }
            public bool Remove { get; private set; }
            public bool MoveXPos { get; private set; }
            public bool MoveXNeg { get; private set; }
            public bool MoveYPos { get; private set; }
            public bool MoveYNeg { get; private set; }

            public enum Type
            {
                CLEAR, BLOCKED
            }

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

        public static bool Border { get; set; } = true;

        private static readonly Dictionary<int, Neighbourhood> Neighbourhoods = new Dictionary<int, Neighbourhood>();

        public static List<Point> GenerateWall(AnalysisMap map, IEnumerable<Point> goals, int optimizations = 3)
        {
            var wall = new HashSet<Point>();
            foreach (var pos in GrahamScan.GetConvexHull(goals))
            {
                wall.Add(pos);
            }
            wall.RemoveWhere(p => map.Tiles[p.X, p.Y].Type != AnalysisTileType.NONE);

            for (int i = 0; i < optimizations; i++)
            {
                var fix = Fix(map, wall);

                if (!fix)
                {
                    break;
                }
            }

            var interior = FloodFill.GetRegion(map.Center,
                false,
                p => map.IsOnMap(p) && map.Tiles[p.X, p.Y].Type == AnalysisTileType.NONE && !wall.Contains(p));

            var exterior = FloodFill.GetRegion(new Point(0, 0),
                false,
                p => map.IsOnMap(p) && map.Tiles[p.X, p.Y].Type == AnalysisTileType.NONE && !wall.Contains(p));

            foreach (var point in wall.ToList())
            {
                var has_interior = false;
                var has_exterior = false;
                foreach (var n in map.GetNeighbours(point, true))
                {
                    if (interior.Contains(n))
                    {
                        has_interior = true;
                    }
                    else if (exterior.Contains(n))
                    {
                        has_exterior = true;
                    }
                }

                if (has_interior == false || has_exterior == false)
                {
                    wall.Remove(point);
                    //Debug.WriteLine($"Removed {point.X} {point.Y}");
                }
            }

            return wall.ToList();
        }

        private static bool Fix(AnalysisMap map, HashSet<Point> wall)
        {
            var work = false;
            
            foreach (var point in wall.ToList())
            {
                var neighbourhood = GetNeighbourhood(map, wall, point);

                if (neighbourhood.Remove)
                {
                    wall.Remove(point);
                    work = true;
                    //Debug.WriteLine($"Removed {point.X} {point.Y}");
                }
                else
                {
                    var size = map.Size;
                    if (point.X < 2 || point.X > size - 3 || point.Y < 2 || point.Y > size - 3)
                    {
                        if (Border)
                        {
                            continue;
                        }
                    }

                    var center = map.Center;
                    var x = point.X - center.X;
                    var y = point.Y - center.Y;

                    var moves = new List<Point>();
                    
                    if (Math.Abs(x) > Math.Abs(y))
                    {
                        if (x >= 0)
                        {
                            moves.Add(new Point(1, 0));
                        }
                        else
                        {
                            moves.Add(new Point(-1, 0));
                        }

                        if (y >= 0)
                        {
                            moves.Add(new Point(0, 1));
                        }
                        else
                        {
                            moves.Add(new Point(0, -1));
                        }
                        /*
                        if (y >= 0)
                        {
                            moves.Add(new Point(0, -1));
                        }
                        else
                        {
                            moves.Add(new Point(0, 1));
                        }

                        if (x >= 0)
                        {
                            moves.Add(new Point(-1, 0));
                        }
                        else
                        {
                            moves.Add(new Point(1, 0));
                        }*/
                    }
                    else
                    {
                        if (y >= 0)
                        {
                            moves.Add(new Point(0, 1));
                        }
                        else
                        {
                            moves.Add(new Point(0, -1));
                        }

                        if (x >= 0)
                        {
                            moves.Add(new Point(1, 0));
                        }
                        else
                        {
                            moves.Add(new Point(-1, 0));
                        }
                        /*
                        if (x >= 0)
                        {
                            moves.Add(new Point(-1, 0));
                        }
                        else
                        {
                            moves.Add(new Point(1, 0));
                        }

                        if (y >= 0)
                        {
                            moves.Add(new Point(0, -1));
                        }
                        else
                        {
                            moves.Add(new Point(0, 1));
                        }*/
                    }

                    foreach (var move in moves)
                    {
                        if (TryMove(point, neighbourhood, move.X, move.Y, wall))
                        {
                            work = true;
                            break;
                        }
                    }
                }
            }

            return work;
        }

        private static bool TryMove(Point point, Neighbourhood neighbourhood, int dx, int dy, HashSet<Point> wall)
        {
            var npoint = new Point(point.X + dx, point.Y + dy);

            if (dx == 1 && neighbourhood.MoveXPos)
            {
                wall.Remove(point);
                wall.Add(npoint);
                return true;
            }
            else if (dx == -1 && neighbourhood.MoveXNeg)
            {
                wall.Remove(point);
                wall.Add(npoint);
                return true;
            }
            else if (dy == 1 && neighbourhood.MoveYPos)
            {
                wall.Remove(point);
                wall.Add(npoint);
                return true;
            }
            else if (dy == -1 && neighbourhood.MoveYNeg)
            {
                wall.Remove(point);
                wall.Add(npoint);
                return true;
            }

            return false;
        }

        private static void RemoveUselessPieces(AnalysisMap map, HashSet<Point> interior, HashSet<Point> wall)
        {
            foreach (var point in wall.ToList())
            {
                var has_interior = false;
                var has_exterior = false;

                foreach (var n in map.GetNeighbours(point))
                {
                    if (map.Tiles[n.X, n.Y].Type == AnalysisTileType.NONE && !wall.Contains(n))
                    {
                        if (interior.Contains(n))
                        {
                            has_interior = true;
                        }
                        else
                        {
                            has_exterior = true;
                        }
                    }
                }

                if (has_interior == false || has_exterior == false)
                {
                    wall.Remove(point);
                    Debug.WriteLine($"Removed wall piece {point.X} {point.Y}");
                }
            }
        }

        private static Neighbourhood GetNeighbourhood(AnalysisMap map, HashSet<Point> wall, Point point)
        {
            var neighbourhood = new Neighbourhood();
            var size = map.Size;
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    var p = new Point(point.X + dx, point.Y + dy);

                    if (p.X >= 0 && p.X < size && p.Y >= 0 && p.Y < size)
                    {
                        var tile = map.Tiles[p.X, p.Y];
                        
                        if (tile.Type != AnalysisTileType.NONE || wall.Contains(p))
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

            lock (Neighbourhoods)
            {
                if (Neighbourhoods.TryGetValue(neighbourhood.Hash, out Neighbourhood n))
                {
                    return n;
                }
                else
                {
                    neighbourhood.Recheck();
                    Neighbourhoods.Add(neighbourhood.Hash, neighbourhood);
                    return neighbourhood;
                }
            }
        }
    }
}
