
using AoE2Lib.Utils;
using Protos.Expert.Fact;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quaternary.Algorithms
{
    public static class GrahamScan
    {
        public static IEnumerable<Point> GetConvexHull(IEnumerable<Point> points)
        {
            var p0 = new Point(int.MaxValue, int.MaxValue);
            var remaining = new List<Point>();
            foreach (var point in points.Distinct())
            {
                remaining.Add(point);

                if (point.Y <= p0.Y)
                {
                    if (point.Y < p0.Y || point.X < p0.X)
                    {
                        p0 = point;
                    }
                }
            }
            remaining.Remove(p0);

            var pos0 = Position.FromPoint(p0.X, p0.Y);
            var angles = new Dictionary<double, Point>();
            foreach (var point in remaining)
            {
                var angle = Position.FromPoint(point.X - p0.X, point.Y - p0.Y).Angle;

                if (angles.ContainsKey(angle))
                {
                    var p = Position.FromPoint(angles[angle].X, angles[angle].Y);
                    if (p.DistanceTo(pos0) < Position.FromPoint(point.X, point.Y).DistanceTo(pos0))
                    {
                        angles[angle] = point;
                    }
                }
                else
                {
                    angles.Add(angle, point);
                }
            }

            remaining.Clear();
            foreach (var point in angles.Values)
            {
                remaining.Add(point);
            }

            remaining.Sort((a, b) =>
            {
                var pa = Position.FromPoint(a.X - p0.X, a.Y - p0.Y).Angle;
                var pb = Position.FromPoint(b.X - p0.X, b.Y - p0.Y).Angle;

                return pa.CompareTo(pb);
            });

            var hull = new List<Point>() { p0 };

            foreach (var point in remaining)
            {
                while (hull.Count >= 2 && Clockwise(hull[hull.Count - 2], hull[hull.Count - 1], point))
                {
                    hull.RemoveAt(hull.Count - 1);
                }

                hull.Add(point);
            }

            remaining.Clear();
            
            for (int i = 0; i < hull.Count; i++)
            {
                var current = hull[i];
                var next = hull[(i + 1) % hull.Count];

                //Debug.WriteLine($"from {current.X} {current.Y} to {next.X} {next.Y}");

                remaining.AddRange(Map.GetLine(current, next));
            }

            return remaining.Distinct();
        }

        private static bool Clockwise(Point a, Point b, Point c)
        {
            var pa = Position.FromPoint(b.X - a.X, b.Y - a.Y);
            var pb = Position.FromPoint(c.X - b.X, c.Y - b.Y);

            var angle = pb.AngleFrom(pa);

            return angle > Math.PI;
        }
    }
}
