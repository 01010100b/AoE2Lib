using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unary.Algorithms
{
    public static class Pathing
    {
        public static Dictionary<T, int> GetAllPathDistances<T>(IEnumerable<T> initial, Func<T, IEnumerable<T>> get_neighbours, int max_distance = int.MaxValue)
        {
            var dict = new Dictionary<T, int>();
            var queue = new Queue<T>();
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
                    foreach (var child in get_neighbours(parent).Where(c => !dict.ContainsKey(c)))
                    {
                        dict.Add(child, d + 1);
                        queue.Enqueue(child);
                    }
                }
            }

            return dict;
        }
    }
}
