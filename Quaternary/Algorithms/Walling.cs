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
using static Quaternary.Modules.WallingModule;

namespace Quaternary.Algorithms
{
    public static class Walling
    {
        

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
