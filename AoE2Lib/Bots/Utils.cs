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
            
            foreach (var node in initial)
            {
                dict.Add(node, 0);
            }

            AddAllPathDistances(dict, get_neighbours, max_distance);

            return dict;
        }

        public static void AddAllPathDistances<TNode>(Dictionary<TNode, int> distances, Func<TNode, IEnumerable<TNode>> get_neighbours, int max_distance = int.MaxValue)
        {
            var queue = new Queue<KeyValuePair<TNode, int>>();

            foreach (var kvp in distances)
            {
                if (queue.Count == 0 || queue.Peek().Value == kvp.Value)
                {
                    queue.Enqueue(kvp);
                }
                else if (queue.Peek().Value < kvp.Value)
                {
                    queue.Clear();
                    queue.Enqueue(kvp);
                }
            }

            while (queue.Count > 0)
            {
                var parent = queue.Dequeue().Key;
                var d = distances[parent];

                if (d < max_distance)
                {
                    foreach (var child in get_neighbours(parent))
                    {
                        if (!distances.ContainsKey(child))
                        {
                            distances.Add(child, d + 1);
                            queue.Enqueue(new KeyValuePair<TNode, int>(child, d + 1));
                        }
                    }
                }
            }
        }

        public static List<TNode> GetPath<TNode>(TNode from, TNode to, Func<TNode, IEnumerable<TNode>> get_neighbours, Func<TNode, double> get_cost, Func<TNode, TNode, double> get_heuristic)
        {
            throw new NotImplementedException();
        }
    }
}
