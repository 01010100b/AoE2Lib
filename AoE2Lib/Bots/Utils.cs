using AoE2Lib.Bots.GameElements;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace AoE2Lib.Bots
{
    public static class Utils
    {
        public static Rectangle GetUnitFootprint(int x, int y, int width, int height, int border = 0)
        {
            width += 2 * border;
            height += 2 * border;

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

        public static Dictionary<TNode, int> GetAllPathDistances<TNode>(IEnumerable<TNode> initial, Func<TNode, IEnumerable<TNode>> get_neighbours, int max_distance = int.MaxValue)
        {
            var dict = new Dictionary<TNode, int>();
            var queue = new Queue<TNode>();
            foreach (var start in initial)
            {
                dict.Add(start, 0);
                queue.Enqueue(start);
            }

            while (queue.Count > 0)
            {
                var parent = queue.Dequeue();
                var d = dict[parent];

                if (d < max_distance)
                {
                    foreach (var child in get_neighbours(parent))
                    {
                        if (!dict.ContainsKey(child))
                        {
                            dict.Add(child, d + 1);
                            queue.Enqueue(child);
                        }
                    }
                }
            }

            return dict;
        }

        public static List<TNode> GetPath<TNode>(TNode from, TNode to, Func<TNode, IEnumerable<TNode>> get_neighbours, Func<TNode, double> get_cost, Func<TNode, TNode, double> get_heuristic)
        {
            throw new NotImplementedException();
        }
    }
}
