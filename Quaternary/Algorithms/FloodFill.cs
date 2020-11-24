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
        public static List<Point> GetRegion(Point start, bool diagonal, Func<Point, bool> included)
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
                
                foreach (var neighbour in GetNeighbours(current, diagonal).Where(n => !seen.Contains(n)))
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

        private static IEnumerable<Point> GetNeighbours(Point a, bool diagonal)
        {
            var x = a.X - 1;
            var y = a.Y;
            yield return new Point(x, y);

            x = a.X + 1;
            y = a.Y;
            yield return new Point(x, y);

            x = a.X;
            y = a.Y - 1;
            yield return new Point(x, y);

            x = a.X;
            y = a.Y + 1;
            yield return new Point(x, y);

            if (diagonal)
            {
                x = a.X - 1;
                y = a.Y - 1;
                yield return new Point(x, y);

                x = a.X - 1;
                y = a.Y + 1;
                yield return new Point(x, y);

                x = a.X + 1;
                y = a.Y - 1;
                yield return new Point(x, y);

                x = a.X + 1;
                y = a.Y + 1;
                yield return new Point(x, y);
            }
        }
    }
}
