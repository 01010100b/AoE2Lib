using AoE2Lib.Bots;
using AoE2Lib.Bots.GameElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unary
{
    abstract class UnitController
    {
        public readonly Unit Unit;
        public readonly Unary Unary;

        private readonly List<Unit> RelevantUnits = new();

        public UnitController(Unit unit, Unary unary)
        {
            Unit = unit;
            Unary = unary;

            Unary.UnitsManager.SetController(Unit, this);
        }

        public void Update()
        {
            Tick();
        }

        protected abstract void Tick();

        protected double GetFieldStrength(Position position, Position target_position, double target_radius)
        {
            var target_strength = Math.Abs(position.DistanceTo(target_position) - target_radius);

            throw new NotImplementedException();
        }

        private double GetStrength(double x, double min_range, double strength, double exp)
        {
            x = Math.Min(min_range, x);
            x = Math.Max(0.01, x);

            return strength * Math.Pow(x, -exp);
        }
    }
}
