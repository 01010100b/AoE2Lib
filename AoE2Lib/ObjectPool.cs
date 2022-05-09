using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace AoE2Lib
{
    public static class ObjectPool
    {
        private const int MAX_POOL_SIZE = 1000;

        private static readonly ConcurrentDictionary<Type, ConcurrentQueue<object>> Pools = new();

        public static T Get<T>(Func<T> create, Action<T> reset)
        {
            var pool = GetPool(typeof(T));

            if (pool.TryDequeue(out object obj))
            {
                var o = (T)obj;
                reset(o);

                return o;
            }
            else
            {
                return create();
            }
        }

        public static void Add(object obj)
        {
            var pool = GetPool(obj.GetType());

            if (pool.Count < MAX_POOL_SIZE)
            {
                pool.Enqueue(obj);
            }
        }

        private static ConcurrentQueue<object> GetPool(Type type)
        {
            return Pools.GetOrAdd(type, x => new ConcurrentQueue<object>());
        }
    }
}
