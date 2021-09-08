using AoE2Lib.Bots;
using AoE2Lib.Bots.GameElements;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Unary
{
    internal abstract class Operation
    {
        private static readonly ConcurrentDictionary<Unary, HashSet<Operation>> Operations = new ConcurrentDictionary<Unary, HashSet<Operation>>();

        public static void ClearOperations(Unary unary)
        {
            if (Operations.TryGetValue(unary, out HashSet<Operation> ops))
            {
                ops.Clear();
                Operations.Remove(unary, out _);
            }
        }

        public static List<Operation> GetOperations(Unary unary)
        {
            if (Operations.TryGetValue(unary, out HashSet<Operation> ops))
            {
                return ops.ToList();
            }
            else
            {
                return new List<Operation>();
            }
        }

        public static IEnumerable<Unit> GetFreeUnits(Unary unary)
        {
            var taken = new HashSet<Unit>();
            foreach (var op in GetOperations(unary))
            {
                foreach (var unit in op.Units)
                {
                    taken.Add(unit);
                }
            }

            foreach (var unit in unary.GameState.MyPlayer.GetUnits().Where(u => u.Targetable))
            {
                if (!taken.Contains(unit))
                {
                    yield return unit;
                }
            }
        }

        public abstract Position Position { get; }
        public int UnitCount => _Units.Count;
        public List<Unit> Units => _Units.ToList();

        protected readonly Unary Unary;
        private readonly HashSet<Unit> _Units = new HashSet<Unit>();

        public Operation(Unary unary)
        {
            if (unary == null)
            {
                throw new ArgumentNullException(nameof(unary));
            }

            Unary = unary;

            Operations.TryAdd(Unary, new HashSet<Operation>());

            Operations[Unary].Add(this);
        }

        public abstract void Update();

        public void AddUnit(Unit unit)
        {
            foreach (var op in Operations[Unary])
            {
                op.RemoveUnit(unit);
            }

            _Units.Add(unit);
            Operations[Unary].Add(this);

            Unary.Log.Debug($"Added unit {unit.Id} to operation {ToString()}");
        }

        public void RemoveUnit(Unit unit)
        {
            if (_Units.Contains(unit))
            {
                _Units.Remove(unit);
                Unary.Log.Debug($"Removed unit {unit.Id} from operation {ToString()}");
            }
        }

        public void Clear()
        {
            _Units.Clear();
            Unary.Log.Debug($"Cleared operation {ToString()}");
        }

        public void Stop()
        {
            Clear();
            Operations[Unary].Remove(this);
            Unary.Log.Debug($"Stopped operation {ToString()}");
        }

        public override string ToString()
        {
            return $"{GetType().Name}({Position})-{GetHashCode()}";
        }

        internal void UpdateInternal()
        {
            foreach (var unit in _Units)
            {
                unit.RequestUpdate();
            }

            Unary.Log.Debug($"Updating operation {ToString()}");
            Update();
        }
    }
}
