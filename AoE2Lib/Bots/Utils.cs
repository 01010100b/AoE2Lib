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

        public static int GetDamage(Dictionary<int, int> attacks, Dictionary<int, int> armours)
        {
            throw new NotImplementedException();

            const int PIERCE = 3;
            const int MELEE = 4;

            var dmg = 0;
            /*
            if (DatUnit == null || target.DatUnit == null)
            {
                return 1;
            }

            var me = DatUnit;
            var enemy = target.DatUnit;

            foreach (var attack in me.Attacks)
            {
                if (attack.Id == PIERCE)
                {
                    dmg += Math.Max(0, this[ObjectData.BASE_ATTACK] - target[ObjectData.PIERCE_ARMOR]);
                }
                else if (attack.Id == MELEE)
                {
                    dmg += Math.Max(0, this[ObjectData.BASE_ATTACK] - target[ObjectData.STRIKE_ARMOR]);
                }
                else
                {
                    foreach (var armor in enemy.Armors)
                    {
                        if (attack.Id == armor.Id)
                        {
                            dmg += Math.Max(0, attack.Amount - armor.Amount);
                        }
                    }
                }
            }
            */
        }
    }
}
