using AoE2Lib.Bots;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unary.Algorithms
{
    class PotentialField
    {
        public double FriendlyStrength { get; set; } = -5;
        public double FriendlyMaxRange { get; set; } = 2;
        public double EnemyStrength { get; set; } = 0;

        public double GetStrengthAtPosition(Position position, Position move_position, double move_radius,
            IEnumerable<KeyValuePair<double, Position>> friendlies = null,
            IEnumerable<ValueTuple<double, Position, double>> enemies = null)
        {
            var field = 0d;

            // movement field
            field += GetContribution(position, 1d, move_position, 1d, move_radius, double.MaxValue);

            // friendlies field
            if (friendlies != null)
            {
                foreach (var friendly in friendlies)
                {
                    field += GetContribution(position, friendly.Key, friendly.Value, FriendlyStrength, 0, FriendlyMaxRange);
                }
            }

            // enemies field
            if (enemies != null)
            {
                var val = 0d;

                foreach (var enemy in enemies)
                {
                    var v = GetContribution(position, enemy.Item1, enemy.Item2, EnemyStrength, 0, enemy.Item3);
                    val = Math.Max(v, val);
                }

                field += val;
            }

            return field;
        }

        private double GetContribution(Position position, double charge, Position charge_position, double strength, double min_range, double max_range)
        {
            var dist = position.DistanceTo(charge_position);
            dist = Math.Max(min_range, dist);
            dist = Math.Min(max_range, dist);
            dist = Math.Max(0.01, dist);

            return strength * charge * dist;
        }
    }
}
