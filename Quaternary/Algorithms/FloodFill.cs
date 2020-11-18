using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quaternary.Algorithms
{
    public static class FloodFill
    {
        public static List<Point> GetInterior(Point start, Func<Point, IEnumerable<Point>> neighbours, Func<Point, bool> included)
        {
            var seen = new HashSet<Point>();
            seen.Add(start);
            var queue = new Queue<Point>();
            queue.Enqueue(start);

            var interior = new List<Point>();

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                interior.Add(current);
                
                foreach (var neighbour in neighbours(current).Where(n => !seen.Contains(n)))
                {
                    if (included(neighbour))
                    {
                        queue.Enqueue(neighbour);
                    }

                    seen.Add(neighbour);
                }
            }

            return interior;
        }
    }
}
