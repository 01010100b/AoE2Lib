using AoE2Lib;
using AoE2Lib.Bots;
using AoE2Lib.Bots.GameElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unary.Algorithms;

namespace Unary
{
    abstract class UnitController
    {
        public readonly Unit Unit;
        public readonly Unary Unary;
        public readonly PotentialField PotentialField = new();

        public UnitController(Unit unit, Unary unary)
        {
            Unit = unit;
            Unary = unary;
        }

        public void Update()
        {
            Tick();
        }

        protected abstract void Tick();

        protected void PerformPotentialFieldStep(int min_next_attack = int.MinValue, int max_next_attack = int.MaxValue)
        {

        }

        private Position GetMovementPosition(Position current)
        {
            var best_pos = current;
            var best_val = double.MaxValue;

            var positions = new List<Position>();
            positions.Add(current);

            var angle = 0d;
            var radius = 0.5d;
            for (int i = 0; i < 8; i++)
            {
                var dpos = Position.FromPolar(angle, radius);
                var pos = current + dpos;
                positions.Add(pos);
                angle += Math.PI / 4;
            }

            angle = 0d;
            radius = 1.1 * Unary.GameState.GameTimePerTick.TotalSeconds * Unit[ObjectData.SPEED] / 100d;
            for (int i = 0; i < 16; i++)
            {
                var dpos = Position.FromPolar(angle, radius);
                var pos = current + dpos;
                positions.Add(pos);
                angle += Math.PI / 8;
            }

            foreach (var position in positions)
            {
                var field = PotentialField.GetStrength(position);

                if (field < best_val)
                {
                    best_pos = position;
                    best_val = field;
                }
            }

            return best_pos;
        }
    }
}
