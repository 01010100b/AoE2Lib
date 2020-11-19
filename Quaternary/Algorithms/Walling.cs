using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Quaternary.Algorithms.AnalysisMap;

namespace Quaternary.Algorithms
{
    public static class Walling
    {
        public static List<Point> GenerateWall(AnalysisMap map, IEnumerable<Point> goals)
        {
            var wall = new HashSet<Point>();
            foreach (var pos in GrahamScan.GetConvexHull(goals))
            {
                wall.Add(pos);
            }
            wall.RemoveWhere(p => map.Tiles[p.X, p.Y].Type != AnalysisTileType.NONE);

            var interior = new HashSet<Point>();
            foreach (var point in FloodFill.GetInterior(map.Center,
                p => map.GetNeighbours(p),
                p => map.Tiles[p.X, p.Y].Type == AnalysisTileType.NONE && !wall.Contains(p)))
            {
                interior.Add(point);
            }

            RemoveUselessPieces(map, interior, wall);

            return wall.ToList();
        }
        private static void RemoveUselessPieces(AnalysisMap map, HashSet<Point> interior, HashSet<Point> wall)
        {
            // remove useless pieces

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
    }
}
