using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unary
{
    public static class Algorithms
    {
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
    }
}
