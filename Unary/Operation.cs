using AoE2Lib;
using AoE2Lib.Bots.GameElements;
using AoE2Lib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unary.Managers;

namespace Unary
{
    abstract class Operation
    {
        public readonly OperationsManager Manager;
        public IEnumerable<Unit> Units => Manager.GetUnitsForOperation(this);

        public Operation(OperationsManager manager)
        {
            Manager = manager;
            manager.RegisterOperation(this);
        }

        public void Stop()
        {
            Manager.RemoveOperation(this);
        }

        public void AddUnit(Unit unit)
        {
            Manager.AddUnitToOperation(this, unit);
        }

        public void RemoveUnit(Unit unit)
        {
            Manager.RemoveUnitFromOperation(this, unit);
        }

        public void ClearUnits()
        {
            foreach (var unit in Units.ToList())
            {
                RemoveUnit(unit);
            }
        }

        public abstract void Update();

        
    }
}
