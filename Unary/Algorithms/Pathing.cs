using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unary.Algorithms
{
    public static class Pathing
    {
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
