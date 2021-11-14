using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace AoE2Lib
{
    public static class ObjectPool
    {
        private static readonly ConcurrentDictionary<Type, ConcurrentQueue<object>> Pools = new ConcurrentDictionary<Type, ConcurrentQueue<object>>();

        public static T Get<T>(Func<T> create, Action<T> reset)
        {
            var pool = GetPool<T>();

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

        public static void Return<T>(T obj)
        {
            var pool = GetPool<T>();
            pool.Enqueue(obj);
        }

        private static ConcurrentQueue<object> GetPool<T>()
        {
            var type = typeof(T);

            return Pools.GetOrAdd(type, x => new ConcurrentQueue<object>());
        }
    }
}
