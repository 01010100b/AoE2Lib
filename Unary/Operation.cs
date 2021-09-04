using AoE2Lib.Bots;
using AoE2Lib.Bots.GameElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Unary
{
    internal abstract class Operation
    {
        private static readonly Dictionary<Unary, HashSet<Operation>> Operations = new Dictionary<Unary, HashSet<Operation>>();

        public static void ClearOperations(Unary unary)
        {
            if (Operations.TryGetValue(unary, out HashSet<Operation> ops))
            {
                ops.Clear();
                Operations.Remove(unary);
            }
        }

        public static List<Operation> GetOperations(Unary unary)
        {
            return Operations.ContainsKey(unary) ? Operations[unary].ToList() : new List<Operation>();
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
        public abstract int UnitCapacity { get; }
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

            if (!Operations.ContainsKey(Unary))
            {
                Operations.Add(Unary, new HashSet<Operation>());
            }

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
        }

        public void RemoveUnit(Unit unit)
        {
            _Units.Remove(unit);
        }

        public void Clear()
        {
            _Units.Clear();
        }

        internal void UpdateInternal()
        {
            if (_Units.Count == 0)
            {
                Operations[Unary].Remove(this);

                return;
            }

            foreach (var unit in _Units)
            {
                unit.RequestUpdate();
            }

            Unary.Log.Debug($"Updating operation {GetHashCode()}");
            Update();
        }
    }
}
