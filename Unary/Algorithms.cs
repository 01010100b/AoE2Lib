using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unary
{
    public static class Algorithms
    {
        public static void AddAllPathDistances<TNode>(Dictionary<TNode, int> distances, Func<TNode, IReadOnlyList<TNode>> get_neighbours, int max_distance = int.MaxValue)
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
                    var neighbours = get_neighbours(parent);

                    for (int i = 0; i < neighbours.Count; i++)
                    {
                        var child = neighbours[i];

                        if (!distances.ContainsKey(child))
                        {
                            distances.Add(child, d + 1);
                            queue.Enqueue(new KeyValuePair<TNode, int>(child, d + 1));
                        }
                    }
                }
            }
        }

        public static void AddPath<TNode>(List<TNode> path, TNode start, TNode end, Func<TNode, IReadOnlyList<TNode>> get_neighbours)
        {
            throw new NotImplementedException();
        }
    }
}
