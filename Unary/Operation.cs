using AoE2Lib.Bots.GameElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Unary
{
    internal abstract class Operation
    {
        public static List<Operation> GetOperations(Unary unary) => Operations.ContainsKey(unary) ? Operations[unary].ToList() : new List<Operation>();
        private static readonly Dictionary<Unary, HashSet<Operation>> Operations = new Dictionary<Unary, HashSet<Operation>>();

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

            foreach (var unit in unary.GameState.MyPlayer.Units.Where(u => u.Targetable))
            {
                if (!taken.Contains(unit))
                {
                    yield return unit;
                }
            }
        }

        public List<Unit> Units => _Units.ToList();
        private readonly Unary Unary;
        private readonly HashSet<Unit> _Units = new HashSet<Unit>();

        public Operation(Unary unary)
        {
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

            if (_Units.Count == 0)
            {
                Operations[Unary].Remove(this);
            }
        }

        internal void UpdateInternal()
        {
            Update();
        }
    }
}
