using AoE2Lib.Bots.GameElements;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace AoE2Lib.Bots
{
    public static class Utils
    {
        public static Rectangle GetUnitFootprint(int x, int y, int width, int height, int border = 0)
        {
            width += 2 * border;
            height += 2 * border;

            var x_start = x - (width / 2);
            var x_end = x + (width / 2);
            if (width % 2 == 0)
            {
                x_end--;
            }

            var y_start = y - (height / 2);
            var y_end = y + (height / 2);
            if (height % 2 == 0)
            {
                y_end--;
            }

            return new Rectangle(x_start, y_start, x_end - x_start + 1, y_end - y_start + 1);
        }

        public static double GetGatherRate(double raw_rate, double walk_distance, double walk_speed, double max_carry)
        {
            var gather_time = max_carry / raw_rate;
            var walk_time = walk_distance / walk_speed;

            return max_carry / (gather_time + walk_time);
        }
    }
}
