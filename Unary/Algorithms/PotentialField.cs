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
        public Position TargetPosition { get; set; } = new Position(-1, -1);

        public double GetStrength(Position position)
        {
            var field = 0d;

            var strength = TargetPosition.PointX >= 0 && TargetPosition.PointY >= 0 ? 1d : 0d;
            var min_range = 0;
            var max_range = 1e6;
            var charge = 1d;
            var charge_position = TargetPosition;

            field += GetContribution(position, charge, charge_position, strength, min_range, max_range);

            return field;
        }

        private double GetContribution(Position position, double charge, Position charge_position, double strength, double min_range, double max_range)
        {
            var d = Math.Max(0.01, position.DistanceTo(charge_position));

            if (d <= min_range)
            {
                d = min_range;
            }
            else if (d >= max_range)
            {
                d = max_range;
            }

            return strength * charge * d;
        }
    }
}
