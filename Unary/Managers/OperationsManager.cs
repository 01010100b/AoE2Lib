using AoE2Lib;
using AoE2Lib.Bots;
using AoE2Lib.Bots.GameElements;
using AoE2Lib.Bots.Modules;
using Protos.Expert.Action;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Unary.Managers
{
    class OperationsManager : Manager
    {
        public IEnumerable<Operation> Operations => _Operations.Keys;
        private readonly Dictionary<Operation, HashSet<Unit>> _Operations = new Dictionary<Operation, HashSet<Unit>>();

        public IEnumerable<Unit> FreeUnits => _FreeUnits;
        private readonly HashSet<Unit> _FreeUnits = new HashSet<Unit>();

        public OperationsManager(Unary unary) : base(unary)
        {

        }

        public void RegisterOperation<T>(T operation) where T : Operation
        {
            if (!_Operations.ContainsKey(operation))
            {
                _Operations.Add(operation, new HashSet<Unit>());
            }
        }

        public void RemoveOperation<T>(T operation) where T : Operation
        {
            if (_Operations.TryGetValue(operation, out HashSet<Unit> units))
            {
                foreach (var unit in units)
                {
                    RemoveUnitFromOperation(operation, unit);
                }

                _Operations.Remove(operation);
            }
        }

        public IEnumerable<Unit> GetUnitsForOperation(Operation operation)
        {
            return _Operations[operation];
        }

        public void AddUnitToOperation(Operation operation, Unit unit)
        {
            if (operation == null || unit == null)
            {
                return;
            }

            if (_Operations[operation].Contains(unit))
            {
                return;
            }

            _FreeUnits.Remove(unit);
            foreach (var units in _Operations.Values)
            {
                units.Remove(unit);
            }

            _Operations[operation].Add(unit);
        }

        public void RemoveUnitFromOperation(Operation operation, Unit unit)
        {
            if (_Operations[operation].Contains(unit))
            {
                _Operations[operation].Remove(unit);
                _FreeUnits.Add(unit);
            }
        }

        public override void Update()
        {
            

            if (Unary.Tick == 5)
            {
                var command = new Command();
                command.Add(new UpSetAttackStance() { InConstUnitId = -1, InConstAttackStance = (int)UnitStance.NO_ATTACK });

                ExecuteCommand(command);
            }

            var units = Unary.UnitsModule;
            var current_units = new HashSet<Unit>();

            foreach (var unit in units.Units.Values.Where(u => u.PlayerNumber == Unary.PlayerNumber && u.Updated && u.Targetable && u[ObjectData.HITPOINTS] > 0))
            {
                current_units.Add(unit);
            }

            foreach (var op in _Operations.Values)
            {
                foreach (var unit in op.ToList())
                {
                    if (!current_units.Contains(unit))
                    {
                        op.Remove(unit);
                    }
                    else
                    {
                        current_units.Remove(unit);
                    }
                }
            }

            _FreeUnits.Clear();
            foreach (var unit in current_units)
            {
                _FreeUnits.Add(unit);
            }

            foreach (var operation in Operations.ToList())
            {
                operation.Update();

                foreach (var unit in operation.Units)
                {
                    unit.RequestUpdate();
                }
            }
        }
    }
}
